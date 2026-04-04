namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0058CodeFixProviderTests
    {
        [Fact]
        public async Task ColonOnNextLine_InheritanceClause_MovesToSameLine()
        {
            var testCode = @"interface I { }

class C
    {|GM0058::|} I
{
}";
            var fixedCode = @"interface I { }

class C : I
{
}";

            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ColonOnNextLine_ConstructorInitializer_MovesToSameLine()
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
            var fixedCode = @"class Base
{
    public Base(int x) { }
}

class C : Base
{
    public C(int x) : base(x) { }
}";

            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ColonOnNextLine_WhereConstraintClause_MovesToSameLine()
        {
            var testCode = @"class C
{
    public void Foo<T>()
        where T
            {|GM0058::|} System.IDisposable { }
}";
            var fixedCode = @"class C
{
    public void Foo<T>()
        where T : System.IDisposable { }
}";

            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
