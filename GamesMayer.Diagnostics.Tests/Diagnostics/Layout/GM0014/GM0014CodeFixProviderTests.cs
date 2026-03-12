namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0014CodeFixProviderTests
    {
        [Fact]
        public async Task Namespace_KeywordOnDifferentLineThanIdentifier_Fix()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar|}
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
        public async Task Namespace_KeywordOnDifferentLineThanThreeSegmentIdentifier_Fix()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar.Baz|}
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
        public async Task FileScopedNamespace_KeywordOnDifferentLineThanIdentifier_Fix()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar|};";
            var fixedCode = @"namespace Foo.Bar;";
            var test = new CSharpCodeFixTest<GM0014Analyzer, GM0014CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Namespace_IdentifierSpansMultipleLines_Fix()
        {
            var testCode = @"{|GM0014:namespace Foo
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
        public async Task FileScopedNamespace_IdentifierSpansMultipleLines_Fix()
        {
            var testCode = @"{|GM0014:namespace Foo
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
