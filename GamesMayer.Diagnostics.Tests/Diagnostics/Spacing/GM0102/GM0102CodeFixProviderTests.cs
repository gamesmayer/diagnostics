namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0102CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBetweenBrackets_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|} 0 ];
}";
            var fixedCode = @"class Foo
{
    char Bar(string s) => s[0];
}";

            var test = new CSharpCodeFixTest<GM0102Analyzer, GM0102CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenBrackets_EnabledTrue_AddsSpaces()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|}0];
}";
            var fixedCode = @"class Foo
{
    char Bar(string s) => s[ 0 ];
}";

            var test = new CSharpCodeFixTest<GM0102Analyzer, GM0102CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceOnlyAfterOpenBracket_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|} 0];
}";
            var fixedCode = @"class Foo
{
    char Bar(string s) => s[0];
}";

            var test = new CSharpCodeFixTest<GM0102Analyzer, GM0102CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBetweenBrackets_Attribute_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"using System;
{|GM0102:[|} Obsolete ]
class Foo { }";
            var fixedCode = @"using System;
[Obsolete]
class Foo { }";

            var test = new CSharpCodeFixTest<GM0102Analyzer, GM0102CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
