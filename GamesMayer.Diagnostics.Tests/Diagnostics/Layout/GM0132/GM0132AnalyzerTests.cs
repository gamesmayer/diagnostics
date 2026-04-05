namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0132Analyzer>;

    public class GM0132AnalyzerTests
    {
        [Fact]
        public async Task DefaultThreshold_SingleParentOnDeclarationLine_NoDiagnostic()
        {
            var testCode = @"
class Bar { }

class Foo : Bar
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultThreshold_SingleParentOnOwnLine_Diagnostic()
        {
            var testCode = @"
class Bar { }

class Foo :
    {|GM0132:Bar|}
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultThreshold_TwoParentsEachOnOwnLine_NoDiagnostic()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo :
    BaseType,
    IFoo
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultThreshold_TwoParentsOnDeclarationLine_Diagnostics()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo : {|GM0132:BaseType|}, {|GM0132:IFoo|}
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConfiguredThresholdThree_TwoParentsOnDeclarationLine_NoDiagnostic()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo : BaseType, IFoo
{
}
";

            var test = new CSharpAnalyzerTest<GM0132Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0132.threshold = 3"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdThree_TwoParentsOnOwnLines_Diagnostics()
        {
            var testCode = @"
interface IFoo { }
class BaseType { }

class Foo :
    {|GM0132:BaseType|},
    {|GM0132:IFoo|}
{
}
";

            var test = new CSharpAnalyzerTest<GM0132Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0132.threshold = 3"));

            await test.RunAsync();
        }
    }
}