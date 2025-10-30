//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug"); // "Debug" or "Release"
var coverage = Argument("coverage", ""); // "coverage", "xml" or "cobertura"
var versionToApply = Argument("app-version", ""); // e.g. 0.2.0 or 0.2.0-beta
var gitUserName = Argument("gitUserName", "github-actions");
var gitUserEmail = Argument("gitUserEmail", "actions@github.com");
var propsFile = "./Directory.Build.props";

Dictionary<string, Dictionary<string, List<string>>> testCategories = new Dictionary<string, Dictionary<string, List<string>>>{
    {"Unit", new Dictionary<string, List<string>>{
        {"RegistryManager", new List<string>{"Manager_RegistryList", "Manager_RegistryAdd", "Manager_RegistryRemove", "Manager_RegistryUpdate"}},
        {"ServiceManager", new List<string>{"Manager_ServiceInstall", "Manager_ServiceUpdate", "Manager_ServiceRemove"}},
        {"StackManager", new List<string>{"Manager_StackList", "Manager_StackStart", "Manager_StackRestart", "Manager_StackStop"}}
    }},
    {"Integration", new Dictionary<string, List<string>>{
        {"RegistryCommand", new List<string>{"Command_RegistryHelp", "Command_RegistryList", "Command_RegistryAdd", "Command_RegistryRemove", "Command_RegistryUpdate" } },
        {"ServiceCommand", new List<string>{"Command_ServiceHelp", "Command_ServiceInstall", "Command_ServiceUpdate", "Command_ServiceRemove" } },
        {"StackCommand", new List<string>{"Command_StackHelp", "Command_StackList", "Command_StackStart", "Command_StackRestart", "Command_StackStop" } }
    }},
};

//////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////

void UpdateVersion(string version)
{
    if(!System.IO.File.Exists(propsFile))
        throw new CakeException($"File not found: {propsFile}");

    var text = System.IO.File.ReadAllText(propsFile);
    text = System.Text.RegularExpressions.Regex.Replace(text, "<Version>.*?</Version>", $"<Version>{version}</Version>");
    text = System.Text.RegularExpressions.Regex.Replace(text, "<AssemblyVersion>.*?</AssemblyVersion>", $"<AssemblyVersion>{version}.0</AssemblyVersion>");
    text = System.Text.RegularExpressions.Regex.Replace(text, "<FileVersion>.*?</FileVersion>", $"<FileVersion>{version}.0</FileVersion>");
    text = System.Text.RegularExpressions.Regex.Replace(text, "<InformationalVersion>.*?</InformationalVersion>", $"<InformationalVersion>{version}+build</InformationalVersion>");
    System.IO.File.WriteAllText(propsFile, text, System.Text.Encoding.UTF8);
    Information("Updated version in {0} to {1}", propsFile, version);
}

void CommitAndPush(string message, bool createTag = false)
{
    StartProcess("git", new ProcessSettings { Arguments = $"config user.name \"{gitUserName}\"" });
    StartProcess("git", new ProcessSettings { Arguments = $"config user.email \"{gitUserEmail}\"" });
    StartProcess("git", new ProcessSettings { Arguments = "add Directory.Build.props" });
    StartProcess("git", new ProcessSettings { Arguments = $"commit -m \"{message}\" || true" });
    StartProcess("git", new ProcessSettings { Arguments = "push" });

    if (createTag)
    {
        var tagName = "v" + versionToApply;
        StartProcess("git", new ProcessSettings { Arguments = $"tag -f {tagName}" });
        StartProcess("git", new ProcessSettings { Arguments = "push origin --tags" });
        Information("Created tag {0}", tagName);
    }
}

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

foreach (var testType in testCategories)
{
    string coverageArgs = !string.IsNullOrEmpty(coverage) ? $" --coverage --coverage-output-format cobertura --coverage-output {testType.Key}/coverage.cobertura.xml" : "";

    Task($"Test_{testType.Key}")
        .IsDependentOn("Build")
        .Does(() =>
        {
            Information($"Running {testType.Key} tests... {coverage}");

            StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"run -c {configuration} --no-build --no-restore --project src/Tests/ -- --filter-query /[Category={testType.Key}]"
                            + " --xunit-info "
                            + coverageArgs,
                RedirectStandardOutput = false,
                Silent = false
            });
        });

    foreach (var testGroup in testType.Value)
    {
        Task($"Test_{testGroup.Key}")
            .IsDependentOn("Build")
            .Does(() =>
            {
                Information($"Running {testGroup.Key} tests...");

                StartProcess("dotnet", new ProcessSettings
                {
                    Arguments = $"run -c {configuration} --no-build --no-restore --project src/Tests/ -- --filter-query /[Category={testGroup.Key}] --xunit-info",
                    RedirectStandardOutput = false,
                    Silent = false
                });
            });

        foreach (var testUnit in testGroup.Value)
        {
            Task($"Test_{testUnit}")
                .IsDependentOn("Build")
                .Does(() =>
                {
                    Information($"Running {testUnit} tests...");

                    StartProcess("dotnet", new ProcessSettings
                    {
                        Arguments = $"run -c {configuration} --no-build --no-restore --project src/Tests/ -- --filter-query /[Category={testUnit}] --xunit-info",
                        RedirectStandardOutput = false,
                        Silent = false
                    });
                });
        }
    }
}

Task("Test")
    .IsDependentOn("Test_Unit")
    .IsDependentOn("Test_Integration");

if (!string.IsNullOrEmpty(coverage))
{
    Task("Test_With_Reportgenerator")
        .IsDependentOn("Test_Unit")
        .IsDependentOn("Test_Integration")
        .Does(() =>
        {
            Information($"Create HTML coverage report...");

            foreach (var testType in testCategories)
            {
                StartProcess("dotnet", new ProcessSettings
                {
                    Arguments = $"reportgenerator -reports:src/Tests/bin/{configuration}/net9.0/TestResults/{testType.Key}/coverage.cobertura.xml"
                                + $" --xunit-info -targetdir:src/Tests/bin/{configuration}/net9.0/TestResults/{testType.Key}/",
                    RedirectStandardOutput = false,
                    Silent = false
                });
            }
        });
}

Task("Version.Beta")
  .Does(() =>
{
    if (string.IsNullOrWhiteSpace(versionToApply))
        versionToApply = "0.1.0-beta";

    UpdateVersion(versionToApply);
    // CommitAndPush($"chore: apply beta version {versionToApply}");
});

Task("Version.Release")
  .Does(() =>
{
    if (string.IsNullOrWhiteSpace(versionToApply))
        throw new CakeException("Version must be provided to Version.Release task");

    UpdateVersion(versionToApply);
    // CommitAndPush($"chore: release version {versionToApply}", createTag: true);
});

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);