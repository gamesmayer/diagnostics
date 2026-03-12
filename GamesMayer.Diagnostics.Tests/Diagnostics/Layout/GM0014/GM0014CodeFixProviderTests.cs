namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0014CodeFixProviderTests
    {
        [Fact]
        public async Task Namespace_TwoSegmentsOnMultipleLines_Fix()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar|}
{
}";
            var fixedCode = @"namespace Foo.Bar
{
}";
            var test = new CSharpCodeFixTest<GM0014Analyzer, GM0014CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Namespace_ThreeSegmentsOnMultipleLines_Fix()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar
    .Baz|}
{
}";
            var fixedCode = @"namespace Foo.Bar.Baz
{
}";
            var test = new CSharpCodeFixTest<GM0014Analyzer, GM0014CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FileScopedNamespace_TwoSegmentsOnMultipleLines_Fix()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar|};";
            var fixedCode = @"namespace Foo.Bar;";
            var test = new CSharpCodeFixTest<GM0014Analyzer, GM0014CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
