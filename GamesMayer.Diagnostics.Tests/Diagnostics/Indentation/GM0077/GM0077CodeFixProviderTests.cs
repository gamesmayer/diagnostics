namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0077CodeFixProviderTests
    {
        [Fact]
        public async Task UnindentedStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM0077:int|} x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task OverIndentedStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
            {|GM0077:int|} x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
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
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NestedBlock_UnindentedStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
{|GM0077:int|} x = 1;
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
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassMember_Unindented_Fix()
        {
            var testCode = @"class Foo
{
{|GM0077:private|} int _value;
}";
            var fixedCode = @"class Foo
{
    private int _value;
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PropertyAccessor_Unindented_Fix()
        {
            var testCode = @"class Foo
{
    int Value
    {
{|GM0077:get|};
        set;
    }
}";
            var fixedCode = @"class Foo
{
    int Value
    {
        get;
        set;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
