namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0024Analyzer>;

    public class GM0024AnalyzerTests
    {
        [Fact]
        public async Task ClassWithParentCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"
class BaseType { }

class Foo :
    BaseType
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceWithParentsCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"
interface IFoo { }
interface IBar { }

interface IBaz :
    IFoo,
    IBar
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithParentNotIndented_Diagnostic()
        {
            var testCode = @"
class BaseType { }

class Foo :
{|GM0024:BaseType|}
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithParentOverIndented_Diagnostic()
        {
            var testCode = @"
class BaseType { }

class Foo :
        {|GM0024:BaseType|}
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceWithSecondParentWronglyIndented_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParentTypeOnSameLine_NoDiagnostic()
        {
            var testCode = @"
class BaseType { }

class Foo : BaseType
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}