namespace GamesMayer.Analyzers.Tests.Layout.GM0002
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0002CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineAutoProperty_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0002:public string Name
    {
        get;
        set;
    }|}
}";
            var fixedCode = @"class Foo
{
    public string Name { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0002Analyzer, GM0002CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineAutoPropertyWithPrivateSet_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0002:public string Name
    {
        get;
        private set;
    }|}
}";
            var fixedCode = @"class Foo
{
    public string Name { get; private set; }
}";
            var test = new CSharpCodeFixTest<GM0002Analyzer, GM0002CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
