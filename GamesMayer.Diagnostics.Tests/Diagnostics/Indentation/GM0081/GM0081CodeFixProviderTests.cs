namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0081CodeFixProviderTests
    {
        [Fact]
        public async Task CaseLabel_NotIndented_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
        {|GM0081:case 1:|}
            break;
        {|GM0081:default:|}
            break;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            break;
            default:
            break;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0081Analyzer, GM0081CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CaseLabel_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
                {|GM0081:case 1:|}
                    break;
                {|GM0081:default:|}
                    break;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                    break;
            default:
                    break;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0081Analyzer, GM0081CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NestedSwitch_InnerCaseWrongIndentation_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                switch (2)
                {
                {|GM0081:case 2:|}
                    break;
                }
                break;
            default:
                break;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                switch (2)
                {
                    case 2:
                    break;
                }
                break;
            default:
                break;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0081Analyzer, GM0081CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SwitchExpressionArms_MultiLine_Fix()
        {
            var testCode = @"class C
{
    int Problem(string detail, int code) => code;

    int M(int outcome)
    {
        return outcome switch
        {
    {|GM0081:0 => 200|},
    {|GM0081:1 => Problem
    (
        detail: ""Not enough."",
        code: 409
    )|},
    {|GM0081:_ => 404|}
        };
    }
}";
            var fixedCode = @"class C
{
    int Problem(string detail, int code) => code;

    int M(int outcome)
    {
        return outcome switch
        {
            0 => 200,
            1 => Problem
            (
                detail: ""Not enough."",
                code: 409
            ),
            _ => 404
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0081Analyzer, GM0081CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
