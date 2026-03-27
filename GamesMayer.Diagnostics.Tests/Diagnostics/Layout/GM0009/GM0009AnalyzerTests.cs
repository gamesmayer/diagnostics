namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0009Analyzer>;

    public class GM0009AnalyzerTests
    {
        [Fact]
        public async Task ClassDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"public class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericClassDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"public class Foo<T> { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericClassDeclarationWithLineBreakBeforeTypeParams_Diagnostic()
        {
            var testCode = @"{|GM0009:public class Foo
<T>|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"{|GM0009:public
class Foo|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassDeclarationWithMultipleModifiers_NoDiagnostic()
        {
            var testCode = @"public sealed class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassDeclarationWithLineBreakBetweenModifiers_Diagnostic()
        {
            var testCode = @"{|GM0009:public
sealed
class Foo|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int field;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:private
    int field|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclarationWithInitializerAndLineBreakBeforeEquals_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:private int field
        =|} 1;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclarationWithInitializerAndMultilineValue_NoDiagnostic()
        {
            var testCode = @"using System.Linq;
class Foo
{
    private int[] field = new[] { 1, 2, 3 }
        .Where(x => x > 1)
        .ToArray();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Property { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    int Property|} { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyDeclarationWithInitializerAndLineBreakBeforeEquals_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public int Property
        =|} 1;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyDeclarationWithInitializerAndMultilineValue_NoDiagnostic()
        {
            var testCode = @"using System.Linq;
class Foo
{
    public int[] Property = new[] { 1, 2, 3 }
        .Where(x => x > 1)
        .ToArray();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericMethodDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericMethodDeclarationWithLineBreakBeforeTypeParams_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public void Method
    <T>|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    void Method|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationWithLineBreakBetweenTypeAndName_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public void
    Method|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    Foo|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StructDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"{|GM0009:public
struct Bar|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"{|GM0009:public
interface IFoo|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EventDeclarationOnSingleLine_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo
{
    public event EventHandler MyEvent { add { } remove { } }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EventDeclarationWithLineBreak_Diagnostic()
        {
            var testCode = @"using System;
class Foo
{
    {|GM0009:public event
    EventHandler MyEvent|} { add { } remove { } }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MemberWithAttributes_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo
{
    [Obsolete]
    public int Property { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MemberWithAttributesAndLineBreak_Diagnostic()
        {
            var testCode = @"using System;
class Foo
{
    [Obsolete]
    {|GM0009:public
    int Property|} { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
