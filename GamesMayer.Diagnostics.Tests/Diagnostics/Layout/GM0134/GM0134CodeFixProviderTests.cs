namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0134CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBeforeColon_Fix()
        {
            var testCode = @"
interface IBar { }

class Foo
{|GM0134:|}
    : IBar
{
}
";
            var fixedCode = @"
interface IBar { }

class Foo
    : IBar
{
}
";

            var test = new CSharpCodeFixTest<GM0134Analyzer, GM0134CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineAfterColon_Fix()
        {
            var testCode = @"
interface IBar { }

class Foo :
{|GM0134:|}
    IBar
{
}
";
            var fixedCode = @"
interface IBar { }

class Foo :
    IBar
{
}
";

            var test = new CSharpCodeFixTest<GM0134Analyzer, GM0134CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}