namespace ProjectDocumentationTool.Models
{
    // Model to store detailed file information within a project
    public class FileInfoModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Size { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
