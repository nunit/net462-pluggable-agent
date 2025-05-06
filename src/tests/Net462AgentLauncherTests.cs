// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Runtime.Versioning;
using NUnit.Common;
using NUnit.Framework;
using System.Drawing.Text;

namespace NUnit.Engine.Agents
{
    public class Net462AgentLauncherTests
    {
        private static readonly Guid AGENTID = Guid.NewGuid();
        private const string AGENCY_URL = "tcp://127.0.0.1:1234/TestAgency";
        private const string AGENT_NAME = "nunit-agent-net462.exe";
        private const string X86_AGENT_NAME = "nunit-agent-net462-x86.exe";
        private static string AGENT_DIR = Path.Combine(TestContext.CurrentContext.TestDirectory, "agent");
        private static string TESTS_DIR = Path.Combine(TestContext.CurrentContext.TestDirectory, "tests");

        // Constants used for settings
        private const string NETFX = FrameworkIdentifiers.NetFramework;
        private const string NETCORE = FrameworkIdentifiers.NetCoreApp;
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

        private const string TARGET_RUNTIME_FRAMEWORK = NET462;
        private const string RUN_AS_X86 = "RunAsX86";
        private const string DEBUG_AGENT = "DebugAgent";
        private const string TRACE_LEVEL = "InternalTraceLevel";
        private const string WORK_DIRECTORY = "WorkDirectory";
        private const string LOAD_USER_PROFILE = "LoadUserProfile";

        static readonly char SEP = Path.DirectorySeparatorChar;
        static readonly char ALT = Path.AltDirectorySeparatorChar;

        private static readonly string[] RUNTIMES = new string[]
        {
            NET20, NET30, NET35, NET40, NET45, NET462, NET481,
            NETCORE11, NETCORE21, NETCORE31, NET50, NET60, NET70, NET80, NET90
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

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateAgent(string runtime)
        {
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[RUN_AS_X86] = false;

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateAgent(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateX86Agent(string runtime)
        {
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[RUN_AS_X86] = true;

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateAgent(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateAgent(string runtime)
        {
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[RUN_AS_X86] = false;

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, false);
            }
            else
            {
                Assert.That(() => _launcher.CreateAgent(AGENTID, AGENCY_URL, _package), Throws.ArgumentException);
            }
        }

        private void CheckAgentPath(Process process, bool x86)
        {
            string agentName = x86 ? X86_AGENT_NAME : AGENT_NAME;
            string agentPath = Path.Combine(AGENT_DIR, agentName).Replace(ALT, SEP);

            Assert.That(process.StartInfo.FileName.Replace(ALT, SEP), Is.EqualTo(agentPath));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateX86Process(string runtime)
        {
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[RUN_AS_X86] = true;

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, true);
                Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            }
            else
            {
                Assert.That(() => _launcher.CreateAgent(AGENTID, AGENCY_URL, _package), Throws.ArgumentException);
            }
        }

        private void CheckStandardProcessSettings(Process process)
        {
            Assert.That(process, Is.Not.Null);
            Assert.That(process.EnableRaisingEvents, Is.True, "EnableRaisingEvents");

            var startInfo = process.StartInfo;
            Assert.That(startInfo.UseShellExecute, Is.False, "UseShellExecute");
            Assert.That(startInfo.CreateNoWindow, Is.True, "CreateNoWindow");
            Assert.That(startInfo.LoadUserProfile, Is.False, "LoadUserProfile");
            Assert.That(startInfo.WorkingDirectory, Is.EqualTo(Environment.CurrentDirectory));

            var arguments = startInfo.Arguments;
            Assert.That(arguments, Does.Not.Contain("--trace="));
            Assert.That(arguments, Does.Not.Contain("--debug-agent"));
            Assert.That(arguments, Does.Not.Contain("--work="));
        }

        [Test]
        public void DebugAgentSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[DEBUG_AGENT] = true;
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--debug-agent"));
        }

        [Test]
        public void TraceLevelSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[TRACE_LEVEL] = "Debug";
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--trace=Debug"));
        }

        [Test]
        public void WorkDirectorySetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[WORK_DIRECTORY] = "WORKDIRECTORY";
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--work=WORKDIRECTORY"));
        }

        [Test]
        public void LoadUserProfileSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings[EnginePackageSettings.TargetFrameworkName] = runtime;
            _package.Settings[LOAD_USER_PROFILE] = true;
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, _package);
            Assert.That(agentProcess.StartInfo.LoadUserProfile, Is.True);
        }

        //[Test]
        public void ExecuteTestDirectly()
        {
            var package = new TestPackage(Path.Combine(TESTS_DIR, "net8.0/mock-assembly.dll")).SubPackages[0];
            package.AddSetting("TargetRuntimeFramework", "netcore-8.0");

            Assert.That(_launcher.CanCreateAgent(package));
            var agentProcess = _launcher.CreateAgent(AGENTID, AGENCY_URL, package);
            agentProcess.StartInfo.RedirectStandardOutput = true;
            agentProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };

            Console.WriteLine("Launching agent for direct execution");
            Assert.That(() => agentProcess.Start(), Throws.Nothing);
            agentProcess.BeginOutputReadLine();
            Assert.That(agentProcess.WaitForExit(5000), "Agent failed to terminate");
            Assert.That(agentProcess.ExitCode, Is.EqualTo(0));
        }
    }
}
