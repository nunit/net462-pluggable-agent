// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using NUnit.Common;
using NUnit.Framework;

namespace NUnit.Engine.Agents
{
    public class Net462AgentLauncherTests
    {
        private static readonly Guid AGENTID = Guid.NewGuid();
        private const string AGENCY_URL = "tcp://127.0.0.1:1234/TestAgency";
        private const string AGENT_NAME = "nunit-agent-net462.exe";
        private const string X86_AGENT_NAME = "nunit-agent-net462-x86.exe";
        private static string AGENT_DIR = Path.Combine(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory), "agent");
        private static string TESTDATA_DIR = Path.Combine(TestContext.CurrentContext.TestDirectory, "../testdata");

        // Constants used for settings
        private const string NETFX = ".NETFramework";
        private const string NETCORE = ".NETCoreApp";
        private const string NET20 = $"{NETFX},Version=v2.0";
        private const string NET30 = $"{NETFX},Version=v3.0";
        private const string NET35 = $"{NETFX},Version=v3.5";
        private const string NET40 = $"{NETFX},Version=v4.0";
        private const string NET45 = $"{NETFX},Version=v4.5";
        private const string NET462 = $"{NETFX},Version=v4.6.2";
        private const string NET481 = $"{NETFX},Version=v4.8.1";
        private const string NETCORE11 = $"{NETCORE},Version=1.1";
        private const string NETCORE21 = $"{NETCORE},Version=2.1";
        private const string NETCORE31 = $"{NETCORE},Version=3.1";
        private const string NET50 = $"{NETCORE},Version=5.0";
        private const string NET60 = $"{NETCORE},Version=6.0";
        private const string NET70 = $"{NETCORE},Version=7.0";
        private const string NET80 = $"{NETCORE},Version=8.0";
        private const string NET90 = $"{NETCORE},Version=9.0";
        private const string NET10 = $"{NETCORE},Version=10.0";

        static readonly char SEP = Path.DirectorySeparatorChar;
        static readonly char ALT = Path.AltDirectorySeparatorChar;

        private static readonly string[] RUNTIMES = new string[]
        {
            NET20, NET30, NET35, NET40, NET45, NET462, NET481,
            NETCORE11, NETCORE21, NETCORE31, NET50, NET60, NET70, NET80, NET90, NET10
        };

        private static readonly string[] SUPPORTED = new string[] {
            NET20, NET30, NET35, NET40, NET45, NET462, NET481,
        };

        private Net462AgentLauncher _launcher;
        private TestPackage _package;

        [SetUp]
        public void SetUp()
        {
            _launcher = new Net462AgentLauncher();
            _package = new TestPackage("junk.dll");
        }

        /// <summary>Advertises the runtime actually targeted by the .NET Framework 4.6.2 agent executable.</summary>
        [Test]
        public void AgentInfoReportsNet462Runtime()
        {
            Assert.That(_launcher.AgentInfo.TargetRuntime.Identifier, Is.EqualTo(NETFX));
            Assert.That(_launcher.AgentInfo.TargetRuntime.Version, Is.EqualTo(new Version(4, 6, 2)));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateAgent(string runtime)
        {
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(false), true);

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateAgent(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateX86Agent(string runtime)
        {
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(true), true);

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateAgent(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateAgent(string runtime)
        {
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(false), true);

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, false);
            }
            else
            {
                var action = () => _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                Assert.That(action, Throws.ArgumentException);
            }
        }

        private static void CheckAgentPath(Process process, bool x86)
        {
            string agentName = x86 ? X86_AGENT_NAME : AGENT_NAME;
            string agentPath = Path.Combine(AGENT_DIR, agentName).Replace(ALT, SEP);

            Assert.That(process.StartInfo.FileName.Replace(ALT, SEP), Is.EqualTo(agentPath));
            Assert.That(File.Exists(agentPath), $"No agent at {agentPath}");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AgentPathDoesNotContainParentSegments(bool runAsX86)
        {
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(NET462), true);
            _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(runAsX86), true);

            using var process = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);

            // Extended Windows paths do not resolve parent-directory segments.
            Assert.That(process.StartInfo.FileName.Split(SEP, ALT), Does.Not.Contain(".."));
        }

#if NET6_0_OR_GREATER
        [TestCase(false)]
        [TestCase(true)]
        [Platform("Win")]
        public void DevicePathLauncherUsesOrdinaryExecutablePath(bool runAsX86)
        {
            var context = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);
            try
            {
                string assemblyPath = typeof(Net462AgentLauncher).Assembly.Location;
                var assembly = context.LoadFromAssemblyPath(@"\\?\" + assemblyPath);
                Assert.That(assembly.Location, Does.StartWith(@"\\?\"));
                var type = assembly.GetType(typeof(Net462AgentLauncher).FullName);
                var launcher = (NUnit.Engine.Extensibility.IAgentLauncher)Activator.CreateInstance(type);
                _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(NET462), true);
                _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(runAsX86), true);

                using var process = launcher.CreateAgent(AGENTID, AGENCY_URL, _package);

                string name = runAsX86 ? X86_AGENT_NAME : AGENT_NAME;
                string expected = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath)), "agent", name);
                Assert.That(process.StartInfo.FileName, Is.EqualTo(expected));
            }
            finally
            {
                context.Unload();
            }
        }
#endif

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateX86Process(string runtime)
        {
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.RunAsX86.WithValue(true), true);

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, true);
            }
            else
            {
                var action = () => _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                Assert.That(action, Throws.ArgumentException);
            }
        }

        private static void CheckStandardProcessSettings(Process process)
        {
            Assert.That(process, Is.Not.Null);
            Assert.That(process.EnableRaisingEvents, Is.True, "EnableRaisingEvents");

            var startInfo = process.StartInfo;
            Assert.That(startInfo.UseShellExecute, Is.False, "UseShellExecute");
            Assert.That(startInfo.CreateNoWindow, Is.True, "CreateNoWindow");
            if (Platform.IsWindows)
                Assert.That(startInfo.LoadUserProfile, Is.False, "LoadUserProfile");
            Assert.That(startInfo.WorkingDirectory, Is.EqualTo(Environment.CurrentDirectory));

            var arguments = startInfo.Arguments;
            Assert.That(arguments, Does.Contain("--trace=Warning"));
            Assert.That(arguments, Does.Not.Contain("--debug-agent"));
            Assert.That(arguments, Does.Not.Contain("--work="));
        }

        [Test]
        public void DebugAgentSetting()
        {
            var runtime = SUPPORTED[0];
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.DebugAgent.WithValue(true), true);
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--debug-agent"));
        }

        [TestCase("Off")]
        [TestCase("Error")]
        [TestCase("Warning")]
        [TestCase("Info")]
        [TestCase("Debug")]
        [TestCase("Verbose")]
        public void TraceLevelSetting(string traceLevel)
        {
            var runtime = SUPPORTED[0];
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.InternalTraceLevel.WithValue(traceLevel), true);
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain($"--trace={traceLevel}"));
        }

        [Test]
        public void WorkDirectorySetting()
        {
            var runtime = SUPPORTED[0];
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.WorkDirectory.WithValue("WORKDIRECTORY"), true);
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--work=WORKDIRECTORY"));
        }

        [Test, Platform("win")]
        public void LoadUserProfileSetting()
        {
            var runtime = SUPPORTED[0];
            _package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(runtime), true);
            _package.AddSetting(SettingDefinitions.LoadUserProfile.WithValue(true), true);
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            if (Platform.IsWindows)
                Assert.That(agentProcess.StartInfo.LoadUserProfile, Is.True);
        }

        //[Test]
        public void ExecuteTestDirectly()
        {
            var package = new TestPackage(Path.Combine(TESTDATA_DIR, "net462/mock-assembly.dll")).SubPackages[0];
            package.AddSetting(SettingDefinitions.TargetFrameworkName.WithValue(NET462), true);

            Assert.That(_launcher.CanCreateAgent(package));
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, package);
            agentProcess.StartInfo.RedirectStandardOutput = true;
            agentProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };

            Console.WriteLine("Launching agent for direct execution");
            var action = () => agentProcess.Start();
            Assert.That(action, Throws.Nothing);
            agentProcess.BeginOutputReadLine();
            Assert.That(agentProcess.WaitForExit(5000), "Agent failed to terminate");
            Assert.That(agentProcess.ExitCode, Is.EqualTo(0));
        }
    }
}
