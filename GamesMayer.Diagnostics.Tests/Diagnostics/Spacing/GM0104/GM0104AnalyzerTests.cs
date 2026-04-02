namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0104Analyzer>;

    public class GM0104AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceInsideAngles_TypeParameter_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo<T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceInsideAngles_TypeParameter_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo{|GM0104:<|} T > { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceOnlyAfterLessThan_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo{|GM0104:<|} T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceOnlyBeforeGreaterThan_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo{|GM0104:<|}T > { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceInsideAngles_TypeArgument_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    System.Collections.Generic.List{|GM0104:<|} int > Values;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceInsideAngles_MultipleTypeArguments_DefaultSetting_Diagnostic()
        {
            var testCode = @"class DialogueLineStartedEvent { }
class DialogueLineStartedDTO { }
interface IListenOnly<TA, TB> { }

class Foo
{
    public IListenOnly{|GM0104:<|} DialogueLineStartedEvent, DialogueLineStartedDTO > DialogueLineStarted { get; }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceInsideAngles_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M< T >() { }
}";

            var test = new CSharpAnalyzerTest<GM0104Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0104.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceInsideAngles_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0104:<|}T>() { }
}";

            var test = new CSharpAnalyzerTest<GM0104Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0104.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultilineContent_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo<
    T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
