namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0132CodeFixProviderTests
    {
        [Fact]
        public async Task DefaultThreshold_SingleParentOnOwnLine_FixToDeclarationLine()
        {
            var testCode = @"
class Bar { }

class Foo :
    {|GM0132:Bar|}
{
}
";
            var fixedCode = @"
class Bar { }

class Foo : Bar
{
}
";

            var test = new CSharpCodeFixTest<GM0132Analyzer, GM0132CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task DefaultThreshold_SecondParentOnDeclarationLine_FixToOwnLine()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo :
    BaseType, {|GM0132:IFoo|}
{
}
";
            var fixedCode = @"
interface IFoo { }
class BaseType { }

class Foo :
    BaseType,
    IFoo
{
}
";

            var test = new CSharpCodeFixTest<GM0132Analyzer, GM0132CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdThree_SecondParentOnOwnLine_FixToDeclarationLine()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo : BaseType,
    {|GM0132:IFoo|}
{
}
";
            var fixedCode = @"
interface IFoo { }
class BaseType { }

class Foo : BaseType, IFoo
{
}
";

            var test = new CSharpCodeFixTest<GM0132Analyzer, GM0132CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0132.threshold = 3"));

            await test.RunAsync();
        }
    }
}