// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Reflection;
using NUnit.Extensibility;

namespace NUnit.Engine.Agents
{
    /// <summary>
    /// Runs tests in the runner's own process under .NET Framework. The engine selects
    /// in-process agents by an exact match on the TargetFramework property.
    /// </summary>
    [Extension(
        Description = "Agent for running tests in-process on .NET Framework 4.6.2 or higher",
        Path = "/NUnit/Engine/InProcessAgents")]
    [ExtensionProperty("AgentType", "InProcess")]
    [ExtensionProperty("TargetFramework", ".NETFramework,Version=v4.6.2")]
    public class Net462InProcessTestAgent : InProcessTestAgent
    {
        private static readonly Assembly AGENT_CORE = typeof(InProcessTestAgent).Assembly;

        static Net462InProcessTestAgent()
        {
            // The runner loads this agent by path from its package, so nunit.agent.core is loaded
            // by path too. Objects returned from the test AppDomain are resolved by assembly name,
            // which finds nothing beside the runner and so fails to match the loaded types. Answer
            // that lookup with the copy already loaded, so the runner needs no copy of its own.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAgentCore;
        }

        public Net462InProcessTestAgent(Guid agentId) : base(agentId)
        {
        }

        private static Assembly? ResolveAgentCore(object? sender, ResolveEventArgs args)
        {
            return AssemblyName.ReferenceMatchesDefinition(new AssemblyName(args.Name), AGENT_CORE.GetName())
                ? AGENT_CORE
                : null;
        }
    }
}
