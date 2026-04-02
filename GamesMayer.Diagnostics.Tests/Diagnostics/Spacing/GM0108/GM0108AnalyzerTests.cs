namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0108Analyzer>;

    public class GM0108AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceAfterType_FieldDeclaration_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int>{|GM0108:items|} = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterType_FieldDeclaration_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int> items = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterType_LocalVariable_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    void M()
    {
        List<int>{|GM0108:value|} = new();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterType_Parameter_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    void M(List<int>{|GM0108:value|}) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterType_TupleElement_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    (List<int>{|GM0108:first|}, int second) pair = (new(), 2);
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterType_EnabledFalse_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int> {|GM0108:value|} = new();
}";

            var test = new CSharpAnalyzerTest<GM0108Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0108.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterType_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int>value = new();
}";

            var test = new CSharpAnalyzerTest<GM0108Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0108.enabled = false"));

            await test.RunAsync();
        }
    }
}
