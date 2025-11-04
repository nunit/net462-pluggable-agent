// Load the recipe
#load nuget:?package=NUnit.Cake.Recipe&version=1.6.0-alpha.6
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/recipe/*.cake

BuildSettings.Initialize
(
    context: Context,
    title: "Net462PluggableAgent",
    solutionFile: "net462-pluggable-agent.sln",
    unitTests: "**/*.tests.exe",
    githubOwner: "NUnit",
    githubRepository: "net462-pluggable-agent",
    exemptFiles: new[] { "ProcessUtils.cs" }
);

var PackageTests = new PackageTest[]
{
    new PackageTest(1, "Net462PackageTest")
    {
        Description = "Run mock-assembly.dll targeting .NET 4.6.2",
        Arguments = "testdata/net462/mock-assembly.dll",
        ExpectedResult = new ExpectedResult("Failed")
        {
            Total = 35, Passed = 21, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
            Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
        }
    },
    new PackageTest(1, "Net462X86PackageTest")
    {
        Description = "Run mock-assembly-x86.dll targeting .NET 4.6.2",
        Arguments = "testdata/net462/mock-assembly-x86.dll",
        ExpectedResult = new ExpectedResult("Failed")
        {
            Total = 35, Passed = 21, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
            Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly-x86.dll") }
        }
    },
    new PackageTest(1, "Net462WindowsFormsTest")
    {
        Description = "Run test using windows forms under .NET 4.6.2",
        Arguments = "testdata/net462/windows-test.dll",
        ExpectedResult = new ExpectedResult("Passed")
        {
            Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
            Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("windows-test.dll") }
        }
    }
};

BuildSettings.Packages.Add(new NuGetPackage(
    id: "NUnit.Extension.Net462PluggableAgent",
    source: BuildSettings.NuGetDirectory + "Net462PluggableAgent.nuspec",
    checks: new PackageCheck[]
    {
        HasFiles("LICENSE.txt", "README.md", "nunit_256.png"),
        HasDirectory("tools").WithFiles(
            "nunit-agent-launcher-net462.dll", "nunit.engine.api.dll"),
        HasDirectory("tools/agent").WithFiles(
            "nunit-agent-net462.exe", "nunit-agent-net462-x86.exe", "nunit.engine.api.dll", "nunit.common.dll", 
            "nunit.extensibility.api.dll", "nunit.extensibility.dll", "nunit.agent.core.dll", "TestCentric.Metadata.dll")
    },
    //symbols: new PackageCheck[]
    testRunner: new AgentRunner(
        BuildSettings.NuGetTestDirectory + "NUnit.Extension.Net462PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/nunit-agent-net462.exe"),
    tests: PackageTests
    ));

BuildSettings.Packages.Add(new ChocolateyPackage(
    "nunit-extension-net462-pluggable-agent",
    source: BuildSettings.ChocolateyDirectory + "net462-pluggable-agent.nuspec",
    checks: new PackageCheck[]
    {
        HasDirectory("tools").WithFiles(
            "LICENSE.txt", "README.md", "nunit_256.png", "VERIFICATION.txt",
            "nunit-agent-launcher-net462.dll", "nunit.engine.api.dll"),
        HasDirectory("tools/agent").WithFiles(
            "nunit-agent-net462.exe", "nunit-agent-net462-x86.exe", "nunit.engine.api.dll", "nunit.common.dll",
            "nunit.extensibility.api.dll", "nunit.extensibility.dll", "nunit.agent.core.dll", "TestCentric.Metadata.dll")
    },
    testRunner: new AgentRunner(
        BuildSettings.ChocolateyTestDirectory + "nunit-extension-net462-pluggable-agent." + BuildSettings.PackageVersion + "/tools/agent/nunit-agent-net462.exe"),
    tests: PackageTests));

//////////////////////////////////////////////////////////////////////
// RECOVER FROM RELEASE ERRORS
//////////////////////////////////////////////////////////////////////

Task("PublishToNuGet")
    .Description("""
	Publishes packages to NuGet for an alpha, beta, rc or final release. If not,
	or if the --nopush option was used, a message is displayed.
	""")
    .Does(() =>
    {
        if (!BuildSettings.ShouldPublishToNuGet)
            Information("Nothing to publish to NuGet from this run.");
        else if (CommandLineOptions.NoPush)
            Information("NoPush option suppressing publication to NuGet");
        else
            foreach (var package in BuildSettings.Packages)
            {
                var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
                var packagePath = BuildSettings.PackageDirectory + packageName;
                try
                {
                    if (package.PackageType == PackageType.NuGet)
                        NuGetPush(packagePath, new NuGetPushSettings() { ApiKey = BuildSettings.NuGetApiKey, Source = BuildSettings.NuGetPushUrl });
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    throw;
                }
            }
    });

Task("PublishToChocolatey")
    .Description("""
	Publishes packages to Chocolatey for an alpha, beta, rc or final release.
	If not, or if the --nopush option was used, a message is displayed.
	""")
    .Does(() =>
    {
        if (!BuildSettings.ShouldPublishToChocolatey)
            Information("Nothing to publish to Chocolatey from this run.");
        else if (CommandLineOptions.NoPush)
            Information("NoPush option suppressing publication to Chocolatey");
        else
            foreach (var package in BuildSettings.Packages)
            {
                var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
                var packagePath = BuildSettings.PackageDirectory + packageName;
                try
                {
                    if (package.PackageType == PackageType.Chocolatey)
                        ChocolateyPush(packagePath, new ChocolateyPushSettings() { ApiKey = BuildSettings.ChocolateyApiKey, Source = BuildSettings.ChocolateyPushUrl });
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    throw;
                }
            }
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
