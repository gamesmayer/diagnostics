namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0104CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceInsideAngles_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo{|GM0104:<|} T > { }";
            var fixedCode = @"class Foo<T> { }";

            var test = new CSharpCodeFixTest<GM0104Analyzer, GM0104CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceInsideAngles_EnabledTrue_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void M{|GM0104:<|}T>() { }
}";
            var fixedCode = @"class Foo
{
    void M< T >() { }
}";

            var test = new CSharpCodeFixTest<GM0104Analyzer, GM0104CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0104.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0104.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceInsideAngles_MultipleTypeArguments_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class DialogueLineStartedEvent { }
class DialogueLineStartedDTO { }
interface IListenOnly<TA, TB> { }

class Foo
{
    public IListenOnly{|GM0104:<|} DialogueLineStartedEvent, DialogueLineStartedDTO > DialogueLineStarted { get; }
}";
            var fixedCode = @"class DialogueLineStartedEvent { }
class DialogueLineStartedDTO { }
interface IListenOnly<TA, TB> { }

class Foo
{
    public IListenOnly<DialogueLineStartedEvent, DialogueLineStartedDTO> DialogueLineStarted { get; }
}";

            var test = new CSharpCodeFixTest<GM0104Analyzer, GM0104CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
