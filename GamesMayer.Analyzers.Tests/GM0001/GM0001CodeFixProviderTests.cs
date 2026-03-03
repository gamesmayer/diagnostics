namespace GamesMayer.Analyzers.Tests.GM0001
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0001CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenUsings_Fix()
        {
            var testCode = @"using System;
{|GM0001:
|}using System.Collections;
class Foo { }";
            var fixedCode = @"using System;
using System.Collections;
class Foo { }";
            var test = new CSharpCodeFixTest<GM0001Analyzer, GM0001CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenUsings_Fix()
        {
            var testCode = @"using System;
{|GM0001:
|}
using System.Collections;
class Foo { }";
            var fixedCode = @"using System;
using System.Collections;
class Foo { }";
            var test = new CSharpCodeFixTest<GM0001Analyzer, GM0001CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
