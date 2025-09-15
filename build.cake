//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug"); // "Debug" or "Release"
var coverage = Argument("coverage", ""); // "coverage", "xml" or "cobertura"

List<string> testCategories = [
    "Unit",
    "RegistryManager", "Manager_RegistryAdd", "Manager_RegistryRemove", "Manager_RegistryUpdate",
    "ServiceManager", "Manager_ServiceInstall", "Manager_ServiceUpdate", "Manager_ServiceRemove",
    "StackManager", "Manager_StackList", "Manager_StackStart", "Manager_StackRestart", "Manager_StackStop",
    "Integration",
    "RegistryCommand", "Command_RegistryHelp", "Command_RegistryList", "Command_RegistryAdd", "Command_RegistryRemove", "Command_RegistryUpdate",
    "ServiceCommand", "Command_ServiceHelp", "Command_ServiceInstall", "Command_ServiceUpdate", "Command_ServiceRemove",
    "StackCommand", "Command_StackHelp", "Command_StackList", "Command_StackStart", "Command_StackRestart", "Command_StackStop"
];

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory("./artifacts");
        CleanDirectories("./**/bin");
        CleanDirectories("./**/obj");
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore("./Lampman.sln");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild("./Lampman.sln", new DotNetBuildSettings
        {
            Configuration = configuration
        });
    });

foreach (var category in testCategories)
{
    string coverageArgs = !string.IsNullOrEmpty(coverage) ? $" --coverage --coverage-output-format cobertura --coverage-output {category}/coverage.cobertura.xml" : "";

    Task($"Test_{category}")
        .IsDependentOn("Build")
        .Does(() =>
        {
            Information($"Running {category} tests... {coverage}");

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"run -c {configuration} --no-build --no-restore --project src/Tests/ -- --filter-query /[Category={category}]"
                            + " --xunit-info "
                            + coverageArgs,
                RedirectStandardOutput = false,
                Silent = false
            });
        });
}

Task("Test")
    .IsDependentOn("Test_RegistryManager")
    .IsDependentOn("Test_ServiceManager")
    .IsDependentOn("Test_StackManager")
    .IsDependentOn("Test_RegistryCommand")
    .IsDependentOn("Test_ServiceCommand")
    .IsDependentOn("Test_StackCommand");

if (!string.IsNullOrEmpty(coverage))
{
    Task("Test_With_Reportgenerator")
        .IsDependentOn("Test_RegistryManager")
        .IsDependentOn("Test_ServiceManager")
        .IsDependentOn("Test_StackManager")
        .IsDependentOn("Test_RegistryCommand")
        .IsDependentOn("Test_ServiceCommand")
        .IsDependentOn("Test_StackCommand")
        .Does(() =>
        {
            Information($"Create HTML coverage report...");

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/RegistryCommand/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/RegistryCommand/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/RegistryManager/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/RegistryManager/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/ServiceCommand/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/ServiceCommand/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/ServiceManager/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/ServiceManager/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/StackCommand/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/StackCommand/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/StackManager/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/StackManager/",
                RedirectStandardOutput = false,
                Silent = false
            });
        });
}

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);