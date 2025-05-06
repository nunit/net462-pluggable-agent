// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using NUnit.Framework;
using NUnit.Framework.Internal;

/// <summary>
/// MockAssembly is intended for those few tests that can only
/// be made to work by loading an entire assembly. Please don't
/// add any other entries or use it for other purposes.
/// </summary>
namespace NUnit.TestData
{
    namespace Assemblies
    {
        [TestFixture(Description="Fake Test Fixture")]
        [Category("FixtureCategory")]
        public class MockTestFixture
        {
            [Test(Description="Mock Test #1")]
            [Category("MockCategory")]
            [Property("Severity", "Critical")]
            public void TestWithDescription() { }

            [Test]
            protected static void NonPublicTest() { }

            [Test]
            public void FailingTest()
            {
                Console.Error.WriteLine("Immediate Error Message");
                Assert.Fail("Intentional failure");
            }

            [Test]
            public void WarningTest()
            {
                Assert.Warn("Warning Message");
            }

            [Test, Ignore("Ignore Message")]
            public void IgnoreTest() { }

            [Test, Explicit]
            public void ExplicitTest() { }

            [Test]
            public void NotRunnableTest(int a, int b) { }

            [Test]
            public void InconclusiveTest()
            {
                Assert.Inconclusive("No valid data");
            }

            [Test]
            public void TestWithException()
            {
                MethodThrowsException();
            }

            private static void MethodThrowsException()
            {
                throw new Exception("Intentional Exception");
            }
        }
    }

    namespace Singletons
    {
        [TestFixture]
        public class OneTestCase
        {
            [Test]
            public virtual void TestCase() { }
        }
    }

    namespace TestAssembly
    {
        [TestFixture]
        public class MockTestFixture
        {
            [Test]
            public void MyTest() { }
        }
    }

    [TestFixture, Ignore("BECAUSE")]
    public class IgnoredFixture
    {
        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }

        [Test]
        public void Test3() { }
    }

    [TestFixture, Explicit]
    public class ExplicitFixture
    {
        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }
    }

    [TestFixture]
    public class BadFixture
    {
        public BadFixture(int val) { }

        [Test]
        public void SomeTest() { }
    }

    [TestFixture]
    public class FixtureWithTestCases
    {
        [TestCase(2, 2, ExpectedResult=4)]
        [TestCase(9, 11, ExpectedResult=20)]
        public int MethodWithParameters(int x, int y)
        {
            return x + y;
        }

        [TestCase(2, 4)]
        [TestCase(9.2, 11.7)]
        public void GenericMethod<T>(T x, T y) { }
    }

    [TestFixture(5)]
    [TestFixture(42)]
    public class ParameterizedFixture
    {
        public ParameterizedFixture(int num) { }

        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }
    }

    [TestFixture(5)]
    [TestFixture(11.5)]
    public class GenericFixture<T>
    {
        public GenericFixture(T num) { }

        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }
    }

    [TestFixture]
    public class FixtureWithDispose : IDisposable
    {
        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }

#pragma warning disable CA1816
        public void Dispose()
        {
            throw new Exception("Exception in Dispose");
        }
#pragma warning restore CA1816
    }

    [TestFixture]
    public class FixtureWithOneTimeTearDown
    {
        [Test]
        public void Test1() { }

        [Test]
        public void Test2() { }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            throw new Exception("Exception in OneTimeTearDown");
        }
    }

    namespace TestSetUpFixture
    {
        [SetUpFixture]
        public class SetUpFixture
        {
            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                throw new Exception("Exception in SetUpFixture.OneTimeTearDown");
            }
        }

        [TestFixture]
        public class Fixture1
        {
            [Test]
            public void Test1() { }
        }

        [TestFixture]
        public class Fixture2
        {
            [Test]
            public void Test1() { }
        }
    }
}
