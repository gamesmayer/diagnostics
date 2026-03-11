namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0010CodeFixProviderTests
    {
        [Fact]
        public async Task TwoConsecutiveBlankLines_Fix()
        {
            var testCode = @"class Foo
{

{|GM0010:|}
    void Method() { }
}";
            var fixedCode = @"class Foo
{

    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeConsecutiveBlankLines_Fix()
        {
            var testCode = @"class Foo
{

{|GM0010:|}
{|GM0010:|}
    void Method() { }
}";
            var fixedCode = @"class Foo
{

    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoConsecutiveBlankLinesBetweenMembers_Fix()
        {
            var testCode = @"class Foo
{
    void A() { }

{|GM0010:|}
    void B() { }
}";
            var fixedCode = @"class Foo
{
    void A() { }

    void B() { }
}";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoConsecutiveBlankLinesAtTopLevel_Fix()
        {
            var testCode = @"using System;

{|GM0010:|}
class Foo { }";
            var fixedCode = @"using System;

class Foo { }";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoConsecutiveBlankLinesAtEndOfFile_Fix()
        {
            var testCode = "class Foo { }\r\n\r\n{|GM0010:|}";
            var fixedCode = "class Foo { }\r\n";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeConsecutiveBlankLinesAtEndOfFile_Fix()
        {
            var testCode = "class Foo { }\r\n\r\n{|GM0010:|}\r\n{|GM0010:|}";
            var fixedCode = "class Foo { }\r\n";
            var test = new CSharpCodeFixTest<GM0010Analyzer, GM0010CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
