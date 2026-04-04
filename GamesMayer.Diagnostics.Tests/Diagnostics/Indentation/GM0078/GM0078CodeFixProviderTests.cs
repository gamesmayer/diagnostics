namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0078CodeFixProviderTests
    {
        [Fact]
        public async Task OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        {|GM0078:}|}
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0078:}|}
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NestedBlock_InnerOpenBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
            {|GM0078:{|}
            int x = 1;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CorrectlyIndented_NoFix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassOpenBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
    {|GM0078:{|}
}";
            var fixedCode = @"class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassCloseBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0078:}|}";
            var fixedCode = @"class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NamespaceOpenBrace_OverIndented_Fix()
        {
            var testCode = @"namespace MyNamespace
    {|GM0078:{|}
    class Foo
    {
    }
}";
            var fixedCode = @"namespace MyNamespace
{
    class Foo
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NamespaceCloseBrace_OverIndented_Fix()
        {
            var testCode = @"namespace MyNamespace
{
    class Foo
    {
    }
    {|GM0078:}|}";
            var fixedCode = @"namespace MyNamespace
{
    class Foo
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
