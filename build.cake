// Load the recipe
#load nuget:?package=NUnit.Cake.Recipe&version=1.5.0-alpha.1
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/recipe/*.cake

BuildSettings.Initialize
(
    context: Context,
    title: "Net462PluggableAgent",
    solutionFile: "net462-pluggable-agent.sln",
    unitTests: "**/*.tests.exe",
    githubOwner: "NUnit",
    githubRepository: "net462-pluggable-agent"
);

var MockAssemblyResult = new ExpectedResult("Failed")
{
    Total = 37,
    Passed = 23,
    Failed = 5,
    Warnings = 1,
    Inconclusive = 1,
    Skipped = 7,
    Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};

var WindowsFormsResult = new ExpectedResult("Passed")
{
    Total = 2,
    Passed = 2,
    Failed = 0,
    Warnings = 0,
    Inconclusive = 0,
    Skipped = 0,
    Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("windows-test.dll") }
};

var PackageTests = new PackageTest[] {
    new PackageTest(1, "Net462PackageTest")
    {
        Description = "Run mock-assembly.dll targeting .NET 4.6.2",
        Arguments = "testdata/net462/mock-assembly.dll",
        ExpectedResult = MockAssemblyResult,
    },
    new PackageTest(1, "Net462WindowsFormsTest")
    {
        Description = "Run test using windows forms under .NET 4.6.2",
        Arguments = "testdata/net462/windows-test.dll",
        ExpectedResult = WindowsFormsResult
    }
};

BuildSettings.Packages.Add(new NuGetPackage(
    id: "NUnit.Extension.Net462PluggableAgent",
    source: BuildSettings.NuGetDirectory + "Net462PluggableAgent.nuspec",
    checks: new PackageCheck[]
    {
        HasFiles("LICENSE.txt", "README.md", "nunit_256.png"),
        HasDirectory("tools").WithFiles(
            "nunit-agent-launcher-net462.dll", "nunit.engine.api.dll", "nunit.agent.core.dll"),
        HasDirectory("tools/agent").WithFiles(
            "nunit-agent-net462.exe", "nunit.engine.api.dll", "nunit.common.dll", 
            "nunit.extensibility.api.dll", "nunit.extensibility.dll", "nunit.agent.core.dll",
            "TestCentric.Metadata.dll")
    },
    //symbols: new PackageCheck[]
    testRunner: new AgentRunner(BuildSettings.NuGetTestDirectory + "NUnit.Extension.Net462PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/nunit-agent-net462.exe"),
    tests: PackageTests
    ));

//BuildSettings.Packages.Add(new ChocolateyPackage(
//    "nunit-extension-net462-pluggable-agent",
//    source: BuildSettings.ChocolateyDirectory + "net462-pluggable-agent.nuspec",
//    checks: new PackageCheck[]
//    {
//        HasDirectory("tools").WithFiles(
//            "LICENSE.txt", "README.md", "nunit_256.png", "VERIFICATION.txt",
//            "nunit-agent-launcher-net462.dll", "nunit.engine.api.dll", "nunit.agent.core.dll"),
//        HasDirectory("tools/agent").WithFiles(
//            "nunit-agent-net462.exe", "nunit.engine.api.dll", "nunit.common.dll",
//            "nunit.extensibility.api.dll", "nunit.extensibility.dll", "nunit.agent.core.dll",
//            "TestCentric.Metadata.dll")
//    },
////    packageContent: new PackageContent()
////        .WithRootFiles("../../nunit.png")
////        .WithDirectories(
////            new DirectoryContent("tools").WithFiles(
////                "../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
////                "net462-agent-launcher.dll", "net462-agent-launcher.pdb",
////                "NUnit.Extensibility.Api.dll", "NUnit.Engine.Api.dll"),
////            new DirectoryContent("tools/agent").WithFiles(
////                "agent/net462-agent.dll", "agent/net462-agent.pdb", "agent/net462-agent.dll.config", "agent/NUnit.Agent.Core.dll",
////                "agent/net462-agent.deps.json", $"agent/net462-agent.runtimeconfig.json",
////                "agent/NUnit.InternalTrace.dll", "agent/NUnit.Metadata.dll",
////                "agent/NUnit.Extensibility.dll", "agent/NUnit.Extensibility.Api.dll",
////                "agent/NUnit.Engine.Api.dll", "agent/Microsoft.Extensions.DependencyModel.dll")),
//    testRunner: new AgentRunner(BuildSettings.ChocolateyTestDirectory + "nunit-extension-net462-pluggable-agent." + BuildSettings.PackageVersion + "/tools/agent/nunit-agent-net462.dll"),
//    tests: PackageTests));

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
