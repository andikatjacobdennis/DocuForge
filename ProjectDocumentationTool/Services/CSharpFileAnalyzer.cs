 using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    public class CSharpFileAnalyzer
    {
        private readonly ILogger<CSharpFileAnalyzer> _logger;

        public CSharpFileAnalyzer(ILogger<CSharpFileAnalyzer> logger)
        {
            _logger = logger;
        }

        public List<ClassInfo> AnalyzeFile(string filePath)
        {
            _logger.LogInformation("Starting analysis of file: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("The file '{FilePath}' was not found.", filePath);
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }

            try
            {
                var fileContent = File.ReadAllText(filePath);
                _logger.LogDebug("Successfully read file content.");

                var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                var root = syntaxTree.GetCompilationUnitRoot();

                var classList = new List<ClassInfo>();

                // Analyze classes
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDecl in classDeclarations)
                {
                    _logger.LogDebug("Analyzing class: {ClassName}", classDecl.Identifier.Text);

                    var classInfo = new ClassInfo
                    {
                        Name = classDecl.Identifier.Text,
                        Namespace = GetNamespace(classDecl),
                        BaseClass = classDecl.BaseList?.Types
                            .FirstOrDefault(t => t.Type is SimpleNameSyntax)?.ToString(),
                        Interfaces = classDecl.BaseList?.Types
                            .Where(t => t.Type is IdentifierNameSyntax)
                            .Select(t => t.ToString())
                            .ToList(),
                        GenericParameters = classDecl.TypeParameterList?.Parameters
                            .Select(tp => tp.Identifier.Text)
                            .ToList(),
                        XmlComment = GetXmlComment(classDecl)
                    };

                    // Analyze properties
                    var properties = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                    foreach (var property in properties)
                    {
                        _logger.LogTrace("Found property: {PropertyName}", property.Identifier.Text);

                        classInfo.Properties.Add(new PropertyInfo
                        {
                            Name = property.Identifier.Text,
                            Type = property.Type.ToString(),
                            Accessibility = GetAccessibility(property.Modifiers),
                            XmlComment = GetXmlComment(property)
                        });
                    }

                    // Analyze methods
                    var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
                    foreach (var method in methods)
                    {
                        _logger.LogTrace("Found method: {MethodName}", method.Identifier.Text);

                        classInfo.Methods.Add(new MethodInfo
                        {
                            Name = method.Identifier.Text,
                            ReturnType = method.ReturnType.ToString(),
                            Accessibility = GetAccessibility(method.Modifiers),
                            Parameters = method.ParameterList.Parameters.Select(param => new ParameterInfo
                            {
                                Name = param.Identifier.Text,
                                Type = param.Type.ToString()
                            }).ToList(),
                            XmlComment = GetXmlComment(method)
                        });
                    }

                    // Analyze delegates
                    var delegates = classDecl.DescendantNodes().OfType<DelegateDeclarationSyntax>();
                    foreach (var del in delegates)
                    {
                        _logger.LogTrace("Found delegate: {DelegateName}", del.Identifier.Text);

                        classInfo.Delegates.Add(new DelegateInfo
                        {
                            Name = del.Identifier.Text,
                            ReturnType = del.ReturnType.ToString(),
                            Accessibility = GetAccessibility(del.Modifiers),
                            Parameters = del.ParameterList.Parameters.Select(param => new ParameterInfo
                            {
                                Name = param.Identifier.Text,
                                Type = param.Type.ToString()
                            }).ToList(),
                            XmlComment = GetXmlComment(del)
                        });
                    }

                    // Analyze events
                    var events = classDecl.DescendantNodes().OfType<EventDeclarationSyntax>();
                    foreach (var evt in events)
                    {
                        _logger.LogTrace("Found event: {EventName}", evt.Identifier.Text);

                        classInfo.Events.Add(new EventInfo
                        {
                            Name = evt.Identifier.Text,
                            Type = evt.Type.ToString(),
                            Accessibility = GetAccessibility(evt.Modifiers),
                            XmlComment = GetXmlComment(evt)
                        });
                    }

                    classList.Add(classInfo);
                }

                // Analyze enums
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                foreach (var enumDecl in enums)
                {
                    _logger.LogDebug("Analyzing enum: {EnumName}", enumDecl.Identifier.Text);

                    classList.Add(new ClassInfo
                    {
                        Name = enumDecl.Identifier.Text,
                        Namespace = GetNamespace(enumDecl),
                        XmlComment = GetXmlComment(enumDecl),
                        Properties = new List<PropertyInfo>(), // Enums have no properties but may fit in ClassInfo
                        Methods = new List<MethodInfo>(),
                        Delegates = new List<DelegateInfo>(),
                        Events = new List<EventInfo>(),
                        Interfaces = null,
                        GenericParameters = null
                    });
                }

                _logger.LogInformation("File analysis completed successfully for: {FilePath}", filePath);
                return classList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during file analysis for: {FilePath}", filePath);
                throw;
            }
        }

        private string GetNamespace(SyntaxNode node)
        {
            var namespaceDecl = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var ns = namespaceDecl?.Name.ToString();
            _logger.LogTrace("Namespace determined: {Namespace}", ns);
            return ns;
        }

        private string GetAccessibility(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(SyntaxKind.PublicKeyword))
                return "public";
            if (modifiers.Any(SyntaxKind.PrivateKeyword))
                return "private";
            if (modifiers.Any(SyntaxKind.ProtectedKeyword))
                return "protected";
            if (modifiers.Any(SyntaxKind.InternalKeyword))
                return "internal";

            return "private"; // Default accessibility
        }

        private string GetXmlComment(SyntaxNode node)
        {
            return string.Empty;
            var trivia = node.GetLeadingTrivia()
                .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
            var comment = trivia.ToString().Trim();

            // Escape characters that can interfere with Markdown
            return System.Web.HttpUtility.HtmlEncode(comment);
        }

    }
}