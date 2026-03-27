namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0059CodeFixProviderTests
    {
        [Fact]
        public async Task MethodCallNamedArgumentWithSpaceBeforeColon_Fix()
        {
            var testCode = @"class Foo
{
    void Bar(int value) { }

    void M()
    {
        Bar({|GM0059:value :|} 42);
    }
}";

            var fixedCode = @"class Foo
{
    void Bar(int value) { }

    void M()
    {
        Bar(value: 42);
    }
}";

            var test = new CSharpCodeFixTest<GM0059Analyzer, GM0059CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConstructorCallNamedArgumentWithLineBreakBeforeColon_Fix()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    void M()
    {
        _ = new List<int>({|GM0059:capacity
            :|} 10);
    }
}";

            var fixedCode = @"using System.Collections.Generic;

class Foo
{
    void M()
    {
        _ = new List<int>(capacity: 10);
    }
}";

            var test = new CSharpCodeFixTest<GM0059Analyzer, GM0059CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}