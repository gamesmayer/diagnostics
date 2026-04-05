namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0138CodeFixProviderTests
    {
        [Fact]
        public async Task ArrowOnNextLine_Fix()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = {|GM0138:(a, b)
            => a + b|};
    }
}
";
            var fixedCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = (a, b) => a + b;
    }
}
";

            var test = new CSharpCodeFixTest<GM0138Analyzer, GM0138CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ArrowSeparatedByBlankLine_Fix()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = {|GM0138:(a, b)

            => a + b|};
    }
}
";
            var fixedCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = (a, b) => a + b;
    }
}
";

            var test = new CSharpCodeFixTest<GM0138Analyzer, GM0138CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyParamsArrowOnNextLine_Fix()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Action f = {|GM0138:()
            => { }|};
    }
}
";
            var fixedCode = @"
using System;

class Foo
{
    void Test()
    {
        Action f = () => { };
    }
}
";

            var test = new CSharpCodeFixTest<GM0138Analyzer, GM0138CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SimpleLambdaArrowOnNextLine_Fix()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int> f = {|GM0138:x
            => x + 1|};
    }
}
";
            var fixedCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int> f = x => x + 1;
    }
}
";

            var test = new CSharpCodeFixTest<GM0138Analyzer, GM0138CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
