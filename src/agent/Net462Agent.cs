// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System.Reflection;

namespace NUnit.Agents
{
    public class Net462Agent : NUnitAgent<Net462Agent>
    {
        private static readonly Logger log = InternalTrace.GetLogger(typeof(Net462Agent));

        public static void Main(string[] args)
        {
            log.Debug("Using nunit.agent.core version " +
                typeof(NUnitAgent<Net462Agent>).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            Execute(args);
        }
    }
}
