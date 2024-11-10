# DocuForge

**DocuForge** is a powerful .NET-based tool designed to analyze, visualize, and generate comprehensive technical documentation for your software projects. It helps developers and teams document project dependencies, build configurations, package references, and more—automatically and effortlessly.

## Features

- **Project Dependency Mapping**: Visualize the relationships between projects within a solution.
- **Package & Dependency Insights**: Analyze and document all NuGet packages and dependencies used across your projects.
- **PlantUML Diagram Generation**: Automatically generate PlantUML diagrams for clear and structured project visualizations.
- **Customizable Reports**: Generate JSON reports detailing your project structure, configurations, and dependencies.
- **Console Interface**: A simple and intuitive console interface to quickly analyze your solution and generate documentation.

## Installation

To get started, clone this repository and restore the project dependencies:

```bash
git clone https://github.com/yourusername/DocuForge.git
cd DocuForge
dotnet restore
```

### Building and Running the Application

1. **Build the solution**:
    ```bash
    dotnet build
    ```

2. **Run the application**:
    ```bash
    dotnet run --project DocuForge
    ```

## Usage

Once the application is running, you can analyze your solution and generate documentation by interacting with the console menu. It will allow you to analyze solution files, generate diagrams, and export project documentation.

## Contributing

Feel free to fork the project and submit pull requests. Contributions are welcome!

## License

This project is licensed under the MIT License.
