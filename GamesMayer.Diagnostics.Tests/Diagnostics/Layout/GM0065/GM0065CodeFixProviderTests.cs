namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0065CodeFixProviderTests
    {
        [Fact]
        public async Task Return_MissingBlankLine_Fix()
        {
            var testCode = @"class Foo
{
    bool M()
    {
        bool isUnlocked = true;
        {|GM0065:return isUnlocked;|}
    }
}";
            var fixedCode = @"class Foo
{
    bool M()
    {
        bool isUnlocked = true;

        return isUnlocked;
    }
}";
            var test = new CSharpCodeFixTest<GM0065Analyzer, GM0065CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Return_MissingBlankLine_InNestedBlock_Fix()
        {
            var testCode = @"class Foo
{
    bool M(bool condition)
    {
        if (condition)
        {
            bool isUnlocked = true;
            {|GM0065:return isUnlocked;|}
        }

        return false;
    }
}";
            var fixedCode = @"class Foo
{
    bool M(bool condition)
    {
        if (condition)
        {
            bool isUnlocked = true;

            return isUnlocked;
        }

        return false;
    }
}";
            var test = new CSharpCodeFixTest<GM0065Analyzer, GM0065CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
