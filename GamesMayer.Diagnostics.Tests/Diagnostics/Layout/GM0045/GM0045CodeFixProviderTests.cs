namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0045CodeFixProviderTests
    {
        [Fact]
        public async Task ExpressionBody_SameIndentAsArrow_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        {|GM0045:x + 1|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionBody_LessIndentThanArrow_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
    {|GM0045:x + 1|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionBody_OverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
                {|GM0045:x + 1|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ParenthesizedLambda_WrongIndent_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int, int> f = (x, y) =>
        {|GM0045:x + y|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int, int> f = (x, y) =>
            x + y;
    }
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionBody_AlreadyCorrect_NoFix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionBodiedProperty_WrongIndent_Fix()
        {
            var testCode = @"using System.Linq;
class C
{
    string[] characters = new string[0];
    string[] CharacterKeys =>
    {|GM0045:characters.Select(c => c).ToArray()|};
}";
            var fixedCode = @"using System.Linq;
class C
{
    string[] characters = new string[0];
    string[] CharacterKeys =>
        characters.Select(c => c).ToArray();
}";
            var test = new CSharpCodeFixTest<GM0045Analyzer, GM0045CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
