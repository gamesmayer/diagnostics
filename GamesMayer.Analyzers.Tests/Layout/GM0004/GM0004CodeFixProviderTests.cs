namespace GamesMayer.Analyzers.Tests.Layout.GM0004
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0004CodeFixProviderTests
    {
        [Fact]
        public async Task AttributeAndMemberOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete] {|GM0004:public|} string Name { get; set; }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    public string Name { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0004Analyzer, GM0004CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AttributeAndMethodOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    [System.Obsolete] {|GM0004:public|} void Bar() { }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    public void Bar() { }
}";
            var test = new CSharpCodeFixTest<GM0004Analyzer, GM0004CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
