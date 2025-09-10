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
    "Integration",
    "RegistryCommand", "Command_RegistryHelp", "Command_RegistryList", "Command_RegistryAdd", "Command_RegistryRemove", "Command_RegistryUpdate",
    "ServiceCommand", "Command_ServiceHelp", "Command_ServiceInstall", "Command_ServiceUpdate", "Command_ServiceRemove"
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
                Arguments = $"run -c {configuration} --no-build --no-restore --project Lampman.Tests/ -- --filter-query /[Category={category}]"
                            + " --xunit-info "
                            + coverageArgs,
                RedirectStandardOutput = false,
                Silent = false
            });
        });
}

if (!string.IsNullOrEmpty(coverage))
{
    Task("Test_With_Reportgenerator")
        .IsDependentOn("Test_Unit")
        .IsDependentOn("Test_Integration")
        .Does(() =>
        {
            Information($"Create HTML coverage report...");

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:Lampman.Tests/bin/{configuration}/net9.0/TestResults/Unit/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:Lampman.Tests/bin/{configuration}/net9.0/TestResults/Unit/",
                RedirectStandardOutput = false,
                Silent = false
            });

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"reportgenerator -reports:Lampman.Tests/bin/{configuration}/net9.0/TestResults/Integration/coverage.cobertura.xml"
                            + $" --xunit-info -targetdir:Lampman.Tests/bin/{configuration}/net9.0/TestResults/Integration/",
                RedirectStandardOutput = false,
                Silent = false
            });
        });
}

Task("Test")
    .IsDependentOn("Test_Unit")
    .IsDependentOn("Test_Integration");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);