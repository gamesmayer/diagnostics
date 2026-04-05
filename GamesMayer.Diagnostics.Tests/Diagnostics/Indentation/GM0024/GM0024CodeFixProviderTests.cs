namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0024CodeFixProviderTests
    {
        [Fact]
        public async Task ClassWithParentNotIndented_Fix()
        {
            var testCode = @"
class BaseType { }

class Foo :
{|GM0024:BaseType|}
{
}
";
            var fixedCode = @"
class BaseType { }

class Foo :
    BaseType
{
}
";

            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task InterfaceWithSecondParentWronglyIndented_Fix()
        {
            var testCode = @"
interface IFoo { }
interface IBar { }

interface IBaz :
    IFoo,
{|GM0024:IBar|}
{
}
";
            var fixedCode = @"
interface IFoo { }
interface IBar { }

interface IBaz :
    IFoo,
    IBar
{
}
";

            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}