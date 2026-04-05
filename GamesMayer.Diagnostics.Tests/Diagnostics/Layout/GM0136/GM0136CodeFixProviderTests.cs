namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0136CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineTypeArgList_Dictionary_Fix()
        {
            var testCode = @"
using System.Collections.Generic;

class Foo
{
    void Test()
    {
        var dict = new Dictionary{|GM0136:<
            int, string>|}();
    }
}
";
            var fixedCode = @"
using System.Collections.Generic;

class Foo
{
    void Test()
    {
        var dict = new Dictionary<int, string>();
    }
}
";

            var test = new CSharpCodeFixTest<GM0136Analyzer, GM0136CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineTypeArgList_MethodCall_Fix()
        {
            var testCode = @"
class Foo
{
    static T Create<T>() where T : new() => new T();

    void Test()
    {
        Create{|GM0136:<
            int>|}();
    }
}
";
            var fixedCode = @"
class Foo
{
    static T Create<T>() where T : new() => new T();

    void Test()
    {
        Create<int>();
    }
}
";

            var test = new CSharpCodeFixTest<GM0136Analyzer, GM0136CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineThreeTypeArgs_Fix()
        {
            var testCode = @"
class Foo
{
    static void Bar<T1, T2, T3>() { }

    void Test()
    {
        Bar{|GM0136:<
            int,
            string,
            bool>|}();
    }
}
";
            var fixedCode = @"
class Foo
{
    static void Bar<T1, T2, T3>() { }

    void Test()
    {
        Bar<int, string, bool>();
    }
}
";

            var test = new CSharpCodeFixTest<GM0136Analyzer, GM0136CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
