// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using NUnit.Common;
using NUnit.Engine.Extensibility;
using NUnit.Extensibility;

namespace NUnit.Engine.Agents
{
    [Extension(Description = "Pluggable agent running tests under .NET 4.6.2", EngineVersion = "4.0.0")]
    [ExtensionProperty("AgentType", "LocalProcess")]
    [ExtensionProperty("TargetFramework", ".NETFramework,Version=4.6.2")]
    public class Net462AgentLauncher : IAgentLauncher
    {
        private static readonly string LAUNCHER_DIR = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private const string RUNTIME_IDENTIFIER = ".NETFramework";
        private static readonly Version RUNTIME_VERSION = new Version(4, 6, 2);
        private static readonly FrameworkName TARGET_FRAMEWORK = new FrameworkName(RUNTIME_IDENTIFIER, RUNTIME_VERSION);
        
        public string AgentName => "Net462Agent";
        public TestAgentType AgentType => TestAgentType.LocalProcess;

        private string AgentPath => Path.Combine(LAUNCHER_DIR, "agent/nunit-agent-net462.exe");
        private string X86AgentPath => Path.Combine(LAUNCHER_DIR, "agent/nunit-agent-net462-x86.exe");

        public TestAgentInfo AgentInfo => new TestAgentInfo(
            GetType().Name,
            TestAgentType.LocalProcess,
            TARGET_FRAMEWORK);

        public bool CanCreateAgent(TestPackage package)
        {
            // Get target runtime from package
            string runtimeSetting = package.Settings.GetValueOrDefault(SettingDefinitions.TargetFrameworkName);
            var targetRuntime = new FrameworkName(runtimeSetting);
            bool runAsX86 = package.Settings.GetValueOrDefault(SettingDefinitions.RunAsX86);

            return targetRuntime.Identifier == RUNTIME_IDENTIFIER && targetRuntime.Version.Major <= RUNTIME_VERSION.Major;
        }

        public Process CreateAgent(Guid agentId, string agencyUrl, TestPackage package)
        {
            // Should not be called unless we have previously checked CanCreateAgent
            if (!CanCreateAgent(package))
                throw new ArgumentException("Unable to create agent. Check result of CanCreateAgent before calling CreateAgent.", nameof(package));

            var process = new Process()
            {
                EnableRaisingEvents = true
            };

            // Access package settings
            var settings = package.Settings;
            bool runAsX86 = settings.GetValueOrDefault(SettingDefinitions.RunAsX86);
            bool debugTests = settings.GetValueOrDefault(SettingDefinitions.DebugTests);
            bool debugAgent = settings.GetValueOrDefault(SettingDefinitions.DebugAgent);
            string traceLevel = settings.GetValueOrDefault(SettingDefinitions.InternalTraceLevel);
            bool loadUserProfile = settings.GetValueOrDefault(SettingDefinitions.LoadUserProfile);
            string workDirectory = settings.GetValueOrDefault(SettingDefinitions.WorkDirectory);

            var sb = new StringBuilder($"--agentId={agentId} --agencyUrl={agencyUrl} --pid={Process.GetCurrentProcess().Id}");

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            if (traceLevel != "Off")
                sb.Append(" --trace=").EscapeProcessArgument(traceLevel);
            if (debugAgent)
                sb.Append(" --debug-agent");
            if (debugTests)
                sb.Append(" --debug-tests");
            if (workDirectory != string.Empty)
                sb.Append(" --work=").EscapeProcessArgument(workDirectory);

            string arguments = sb.ToString();

            var startInfo = process.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.LoadUserProfile = loadUserProfile;

            startInfo.FileName = runAsX86 ? X86AgentPath : AgentPath;
            startInfo.Arguments = arguments;

            return process;
        }
    }
}
#endif
