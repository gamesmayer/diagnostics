namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0129Analyzer>;

    public class GM0129AnalyzerTests
    {
        [Fact]
        public async Task ExplicitObjectCreation_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Item { }
class C
{
    Item field = new Item();

    void M()
    {
        Item local = new Item();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Item { }
class C
{
    Item field = {|GM0129:new|}();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExplicitObjectCreation_EnabledFalse_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Item { }
class C
{
    Item field = new {|GM0129:Item|}();

    void M()
    {
        Item local = new {|GM0129:Item|}();
        field = new {|GM0129:Item|}();

        var list = new List<Item>();
        list.Add(new {|GM0129:Item|}());
    }
}";

            var test = new CSharpAnalyzerTest<GM0129Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Item { }
class C
{
    Item field = new();

    void M()
    {
        Item local = new();
        field = new();

        var list = new List<Item>();
        list.Add(new());
    }
}";

            var test = new CSharpAnalyzerTest<GM0129Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_InNestedInvocationArgument_DefaultSetting_Diagnostic()
        {
            var testCode = @"struct Vector2 { public float x; }
class GUIContent
{
    public GUIContent(string text) { }
}

class GUIStyle
{
    public Vector2 CalcSize(GUIContent content) => default;
}

static class GUILayout
{
    public static object Width(float width) => null;
}

class C
{
    void M(GUIStyle consoleStyle)
    {
        _ = GUILayout.Width(consoleStyle.CalcSize({|GM0129:new|}(""Saved as "")).x);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExplicitObjectCreation_InNestedInvocationArgument_EnabledFalse_Diagnostic()
        {
            var testCode = @"struct Vector2 { public float x; }
class GUIContent
{
    public GUIContent(string text) { }
}

class GUIStyle
{
    public Vector2 CalcSize(GUIContent content) => default;
}

static class GUILayout
{
    public static object Width(float width) => null;
}

class C
{
    void M(GUIStyle consoleStyle)
    {
        _ = GUILayout.Width(consoleStyle.CalcSize(new {|GM0129:GUIContent|}(""Saved as "")).x);
    }
}";

            var test = new CSharpAnalyzerTest<GM0129Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            await test.RunAsync();
        }
    }
}
