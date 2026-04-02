namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0089CodeFixProviderTests
    {
        [Fact]
        public async Task Spaces_IfStatement_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if {|GM0089:(|} b ) { }
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool b)
    {
        if (b) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0089Analyzer, GM0089CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaces_IfStatement_EnabledTrue_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if {|GM0089:(|}b) { }
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool b)
    {
        if ( b ) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0089Analyzer, GM0089CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0089.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0089.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task Spaces_ParenthesizedExpression_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return {|GM0089:(|}a + b );
    }
}";
            var fixedCode = @"class Foo
{
    int M(int a, int b)
    {
        return (a + b);
    }
}";

            var test = new CSharpCodeFixTest<GM0089Analyzer, GM0089CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task Spaces_TupleType_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    List<{|GM0089:(|} string key, object value )> parameters = new();
}";
            var fixedCode = @"using System.Collections.Generic;
class Foo
{
    List<(string key, object value)> parameters = new();
}";

            var test = new CSharpCodeFixTest<GM0089Analyzer, GM0089CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task Spaces_CastExpression_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void M(object o)
    {
        var x = {|GM0089:(|}int )o;
    }
}";
            var fixedCode = @"class Foo
{
    void M(object o)
    {
        var x = (int)o;
    }
}";

            var test = new CSharpCodeFixTest<GM0089Analyzer, GM0089CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
