//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

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
    DotNetBuild("./Lampman.sln", new DotNetBuildSettings {
        Configuration = "Release"
    });
});


Task("Test-RegistryCommand")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Running RegistryCommand tests...");

    // Equivalent of:
    // dotnet run --no-build --no-restore --project Lampman.Tests/ -- --filter-query /[Category=RegistryCommand] --xunit-info

    StartProcess("dotnet", new ProcessSettings
    {
        Arguments = "run --no-build --no-restore --project Lampman.Tests/ -- --filter-query /[Category=RegistryCommand] " +
                    "--xunit-info",
        RedirectStandardOutput = false,
        Silent = false
    });
});

Task("Test-Coverage-RegistryCommand")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Running RegistryCommand tests with coverage...");

    // Equivalent of:
    // dotnet run --no-build --no-restore --project Lampman.Tests/ -- --filter-query /[Category=RegistryCommand] --coverage --coverage-output-format cobertura --coverage-output RegistryCommand/coverage.cobertura.xml --xunit-info

    StartProcess("dotnet", new ProcessSettings {
        Arguments = "run --no-build --no-restore --project Lampman.Tests/ -- --filter-query /[Category=RegistryCommand] " +
                    "--coverage --coverage-output-format cobertura " +
                    "--coverage-output RegistryCommand/coverage.cobertura.xml " +
                    "--xunit-info",
        RedirectStandardOutput = false,
        Silent = false
    });
});

// Task("Test-Unit")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     DotNetTest("./Lampman.Tests/Lampman.Tests.csproj", new DotNetTestSettings {
//         Configuration = "Release",
//         Filter = "Category=Unit",
//         NoBuild = true
//     });
// });

// Task("Test-Integration")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     // Start mock server here if needed
//     Information("Starting MockRegistry...");
//     // You can call dotnet run on Lampman.MockRegistry

//     DotNetTest("./Lampman.Tests/Lampman.Tests.csproj", new DotNetTestSettings {
//         Configuration = "Release",
//         Filter = "Category=Integration",
//         NoBuild = true
//     });

//     // Stop mock server if started manually
// });

// Task("Pack")
//     .IsDependentOn("Test-Unit")
//     .IsDependentOn("Test-Integration")
//     .Does(() =>
// {
//     DotNetPublish("./Lampman.Cli/Lampman.Cli.csproj", new DotNetPublishSettings {
//         Configuration = "Release",
//         Runtime = "win-x64",
//         OutputDirectory = "./artifacts/win-x64"
//     });

//     Zip("./artifacts/win-x64", "./artifacts/Lampman.Cli.zip");
// });

// Task("Default")
//     .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
