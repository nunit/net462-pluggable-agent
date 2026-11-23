// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
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
        public Net462InProcessTestAgent(Guid agentId) : base(agentId)
        {
        }
    }
}
