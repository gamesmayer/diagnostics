namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class ElementOrderCodeFixProviderTests
    {
        [Fact]
        public async Task FieldAfterMethod_MovesFieldFirst_Fix()
        {
            var testCode = @"class C
{
    public void M()
    {
    }

    /// <summary>Counter.</summary>
    private {|GM1201:int x|};
}";
            var fixedCode = @"class C
{
    /// <summary>Counter.</summary>
    private int x;

    public void M()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM1201Analyzer, ElementOrderCodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PublicAfterPrivate_MovesPublicFirst_Fix()
        {
            var testCode = @"class C
{
    private void A()
    {
    }

    public void {|GM1202:B|}()
    {
    }
}";
            var fixedCode = @"class C
{
    public void B()
    {
    }

    private void A()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM1202Analyzer, ElementOrderCodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task StaticAfterInstance_MovesStaticFirst_Fix()
        {
            var testCode = @"class C
{
    private int a;

    private static {|GM1204:int b|};
}";
            var fixedCode = @"class C
{
    private static int b;

    private int a;
}";
            var test = new CSharpCodeFixTest<GM1204Analyzer, ElementOrderCodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PreprocessorDirective_NoFix()
        {
            var testCode = @"class C
{
    public void M()
    {
    }

#region State
    private {|GM1201:int x|};
#endregion
}";
            var test = new CSharpCodeFixTest<GM1201Analyzer, ElementOrderCodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
