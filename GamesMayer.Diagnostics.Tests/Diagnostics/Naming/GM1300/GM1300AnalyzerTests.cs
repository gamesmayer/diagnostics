namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM1300Analyzer>;

    public class GM1300AnalyzerTests
    {
        [Fact]
        public async Task UpperCaseClass_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseClass_Diagnostic()
        {
            var testCode = @"class {|GM1300:foo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseEnum_NoDiagnostic()
        {
            var testCode = @"enum Color { Red, Green, Blue }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseEnum_Diagnostic()
        {
            var testCode = @"enum {|GM1300:color|} { Red, Green, Blue }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseEnumMember_Diagnostic()
        {
            var testCode = @"enum Color { {|GM1300:red|}, Green, Blue }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EnumMemberWithUnderscoreDigit_NoDiagnostic()
        {
            var testCode = @"enum Color { _1Red, Green }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseStruct_NoDiagnostic()
        {
            var testCode = @"struct Point { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseStruct_Diagnostic()
        {
            var testCode = @"struct {|GM1300:point|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseDelegate_NoDiagnostic()
        {
            var testCode = @"delegate void MyDelegate();";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseDelegate_Diagnostic()
        {
            var testCode = @"delegate void {|GM1300:myDelegate|}();";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseMethod_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void MyMethod() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseMethod_Diagnostic()
        {
            var testCode = @"
class Foo
{
    void {|GM1300:myMethod|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OverrideMethod_OnlyBaseDiagnostic()
        {
            var testCode = @"
class Base
{
    public virtual void {|GM1300:myMethod|}() { }
}
class Derived : Base
{
    public override void myMethod() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseProperty_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    int MyProperty { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseProperty_Diagnostic()
        {
            var testCode = @"
class Foo
{
    int {|GM1300:myProperty|} { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OverrideProperty_OnlyBaseDiagnostic()
        {
            var testCode = @"
class Base
{
    public virtual int {|GM1300:myProperty|} { get; set; }
}
class Derived : Base
{
    public override int myProperty { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceImplicitImplementation_OnlyInterfaceDiagnostic()
        {
            var testCode = @"
interface IFoo
{
    void {|GM1300:myMethod|}();
}
class Foo : IFoo
{
    public void myMethod() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UpperCaseNamespace_NoDiagnostic()
        {
            var testCode = @"
namespace MyApp
{
    class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LowerCaseNamespaceComponent_Diagnostic()
        {
            var testCode = @"
namespace {|GM1300:myApp|}
{
    class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task QualifiedNamespace_LowerCaseComponent_Diagnostic()
        {
            var testCode = @"
namespace MyApp.{|GM1300:utils|}
{
    class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
