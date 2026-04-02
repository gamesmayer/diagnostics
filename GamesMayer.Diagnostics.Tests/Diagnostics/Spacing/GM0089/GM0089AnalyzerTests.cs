namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0089Analyzer>;

    public class GM0089AnalyzerTests
    {
        [Fact]
        public async Task NoSpaces_IfStatement_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if (b) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_IfStatement_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if {|GM0089:(|} b ) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_IfStatement_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if {|GM0089:(|}b) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0089Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0089.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task Spaces_IfStatement_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        if ( b ) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0089Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0089.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaces_WhileStatement_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        while (b) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_WhileStatement_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool b)
    {
        while {|GM0089:(|} b ) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_ParenthesizedExpression_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return (a + b);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_ParenthesizedExpression_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return {|GM0089:(|}a + b );
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_CastExpression_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(object o)
    {
        var x = (int)o;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_CastExpression_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(object o)
    {
        var x = {|GM0089:(|}int )o;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_TupleType_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    List<(string key, object value)> parameters = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_TupleType_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    List<{|GM0089:(|} string key, object value )> parameters = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_TupleExpression_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    (int, int) M() => (1, 2);
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_TupleExpression_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    (int, int) M() => {|GM0089:(|} 1, 2 );
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaces_ForStatement_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_ForStatement_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        for ( int i = 0; i < 10; i++ ) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0089Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0089.enabled = true"));

            await test.RunAsync();
        }
    }
}
