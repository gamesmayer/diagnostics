namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0058Analyzer>;

    public class GM0058AnalyzerTests
    {
        [Fact]
        public async Task ColonOnSameLine_InheritanceClause_NoDiagnostic()
        {
            var testCode = @"interface I { }

class C : I
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnNextLine_InheritanceClause_Diagnostic()
        {
            var testCode = @"interface I { }

class C
    {|GM0058::|} I
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnSameLine_ConstructorInitializer_NoDiagnostic()
        {
            var testCode = @"class Base
{
    public Base(int x) { }
}

class C : Base
{
    public C(int x) : base(x) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnNextLine_ConstructorInitializer_Diagnostic()
        {
            var testCode = @"class Base
{
    public Base(int x) { }
}

class C : Base
{
    public C(int x)
        {|GM0058::|} base(x) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnSameLine_WhereConstraintClause_NoDiagnostic()
        {
            var testCode = @"class C
{
    public void Foo<T>()
        where T : System.IDisposable { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnNextLine_WhereConstraintClause_Diagnostic()
        {
            var testCode = @"class C
{
    public void Foo<T>()
        where T
            {|GM0058::|} System.IDisposable { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnSameLine_MultipleBaseTypes_NoDiagnostic()
        {
            var testCode = @"interface I1 { }
interface I2 { }

class C : I1, I2
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnNextLine_MultipleBaseTypes_Diagnostic()
        {
            var testCode = @"interface I1 { }
interface I2 { }

class C
    {|GM0058::|} I1, I2
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnSameLine_Interface_NoDiagnostic()
        {
            var testCode = @"interface I1 { }

interface I2 : I1
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ColonOnNextLine_Interface_Diagnostic()
        {
            var testCode = @"interface I1 { }

interface I2
    {|GM0058::|} I1
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
