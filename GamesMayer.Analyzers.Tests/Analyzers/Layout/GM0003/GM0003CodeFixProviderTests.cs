namespace GamesMayer.Analyzers.Tests.Analyzers.Layout.GM0003
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0003CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineAfterAttributeOnProperty_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public string Name { get; set; }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    public string Name { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0003Analyzer, GM0003CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineAfterAttributeOnMethod_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public void Bar() { }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    public void Bar() { }
}";
            var test = new CSharpCodeFixTest<GM0003Analyzer, GM0003CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
