namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0129CodeFixProviderTests
    {
        [Fact]
        public async Task ImplicitObjectCreation_DefaultSetting_FixToExplicit()
        {
            var testCode = @"class Item { }
class C
{
    Item field = {|GM0129:new|}();
}";

            var fixedCode = @"class Item { }
class C
{
    Item field = new Item();
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ExplicitObjectCreation_EnabledFalse_FixToImplicit()
        {
            var testCode = @"using System.Collections.Generic;
class Item { }
class C
{
    Item field = new {|GM0129:Item|}();

    void M()
    {
        field = new {|GM0129:Item|}();

        var list = new List<Item>();
        list.Add(new {|GM0129:Item|}());
    }
}";

            var fixedCode = @"using System.Collections.Generic;
class Item { }
class C
{
    Item field = new();

    void M()
    {
        field = new();

        var list = new List<Item>();
        list.Add(new());
    }
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_InNestedInvocationArgument_DefaultSetting_FixToExplicit()
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

            var fixedCode = @"struct Vector2 { public float x; }
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
        _ = GUILayout.Width(consoleStyle.CalcSize(new GUIContent(""Saved as "")).x);
    }
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_NestedClass_DefaultSetting_FixToExplicitWithContainingType()
        {
            var testCode = @"class Outer
{
    public class Inner { public Inner(int x) { } }
}

class C
{
    Outer.Inner field = {|GM0129:new|}(42);
}";

            var fixedCode = @"class Outer
{
    public class Inner { public Inner(int x) { } }
}

class C
{
    Outer.Inner field = new Outer.Inner(42);
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_AmbiguousNamespace_DefaultSetting_FixToExplicitWithNamespace()
        {
            var testCode = @"namespace A
{
    public class Foo { }
}

namespace B
{
    using A;

    public class Foo { }

    class C
    {
        A.Foo field = {|GM0129:new|}();
    }
}";

            var fixedCode = @"namespace A
{
    public class Foo { }
}

namespace B
{
    using A;

    public class Foo { }

    class C
    {
        A.Foo field = new A.Foo();
    }
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ExplicitObjectCreation_InNestedInvocationArgument_EnabledFalse_FixToImplicit()
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

            var fixedCode = @"struct Vector2 { public float x; }
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
        _ = GUILayout.Width(consoleStyle.CalcSize(new(""Saved as "")).x);
    }
}";

            var test = new CSharpCodeFixTest<GM0129Analyzer, GM0129CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0129.enabled = false"));

            await test.RunAsync();
        }
    }
}
