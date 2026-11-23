// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnitLite;

namespace NUnit.Engine.Agents
{
    class Program
    {
        static int Main(string[] args)
        {
            return new AutoRun().Execute(args);
        }
    }
}
