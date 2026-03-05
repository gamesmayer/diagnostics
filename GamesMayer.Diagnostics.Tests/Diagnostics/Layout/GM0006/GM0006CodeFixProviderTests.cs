namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0006CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenAttributesOnProperty_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0006:
|}    [System.CLSCompliant(true)]
    public string Name { get; set; }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    [System.CLSCompliant(true)]
    public string Name { get; set; }
}";

            var test = new CSharpCodeFixTest<GM0006Analyzer, GM0006CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineBetweenAttributesOnMethod_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0006:
|}    [System.CLSCompliant(true)]
    public void Bar() { }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    [System.CLSCompliant(true)]
    public void Bar() { }
}";

            var test = new CSharpCodeFixTest<GM0006Analyzer, GM0006CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
