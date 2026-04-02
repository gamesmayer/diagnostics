namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0101Analyzer>;

    public class GM0101AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBetweenEmptyBrackets_ArrayType_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int[] arr = new int[3];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenEmptyBrackets_ArrayType_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int{|GM0101:[|} ] arr = new int[3];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenEmptyBrackets_JaggedArray_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int[][] arr = new int[3][];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenEmptyBrackets_JaggedArray_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int{|GM0101:[|} ]{|GM0101:[|} ] arr = new int[3]{|GM0101:[|} ];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenEmptyBrackets_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int[ ] arr = new int[3];
}";

            var test = new CSharpAnalyzerTest<GM0101Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0101.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenEmptyBrackets_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int{|GM0101:[|}] arr = new int[3];
}";

            var test = new CSharpAnalyzerTest<GM0101Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0101.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyBrackets_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar(int[] arr) => arr[0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
