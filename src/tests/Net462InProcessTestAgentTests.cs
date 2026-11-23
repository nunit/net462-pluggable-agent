// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

#if NETFRAMEWORK
using System;
using System.Linq;
using System.Reflection;
using NUnit.Extensibility;
using NUnit.Framework;

namespace NUnit.Engine.Agents
{
    /// <summary>
    /// The engine's InProcessAgentProvider finds this agent at its extension point
    /// and selects it by an exact match on the TargetFramework property.
    /// </summary>
    public class Net462InProcessTestAgentTests
    {
        private static readonly Type AGENT_TYPE = typeof(Net462InProcessTestAgent);

        [Test]
        public void IsExtensionAtInProcessAgentsPath()
        {
            var extension = AGENT_TYPE.GetCustomAttribute<ExtensionAttribute>();

            Assert.That(extension, Is.Not.Null);
            Assert.That(extension!.Path, Is.EqualTo("/NUnit/Engine/InProcessAgents"));
        }

        [TestCase("AgentType", "InProcess")]
        [TestCase("TargetFramework", ".NETFramework,Version=v4.6.2")]
        public void HasExtensionProperty(string name, string value)
        {
            var values = AGENT_TYPE.GetCustomAttributes<ExtensionPropertyAttribute>()
                .Where(p => p.Name == name)
                .Select(p => p.Value);

            Assert.That(values, Is.EqualTo(new[] { value }));
        }

        [Test]
        public void CanBeCreatedWithAgentId()
        {
            var agentId = Guid.NewGuid();

            // The engine creates the agent through reflection, passing its agent id.
            var agent = Activator.CreateInstance(AGENT_TYPE, agentId);

            Assert.That(agent, Is.InstanceOf<ITestAgent>());
            Assert.That(((TestAgent)agent!).Id, Is.EqualTo(agentId));
        }
    }
}
#endif
