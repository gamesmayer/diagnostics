namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0042CodeFixProviderTests
    {
        [Fact]
        public async Task NoBlankLineAfterLastUsing_Fix()
        {
            var testCode = @"using System;
{|GM0042:class|} Foo { }";
            var fixedCode = @"using System;

class Foo { }";
            var test = new CSharpCodeFixTest<GM0042Analyzer, GM0042CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleUsingsNoBlankLine_Fix()
        {
            var testCode = @"using System;
using System.Collections;
{|GM0042:class|} Foo { }";
            var fixedCode = @"using System;
using System.Collections;

class Foo { }";
            var test = new CSharpCodeFixTest<GM0042Analyzer, GM0042CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NoBlankLineBeforeNamespace_Fix()
        {
            var testCode = @"using System;
{|GM0042:namespace|} MyApp { }";
            var fixedCode = @"using System;

namespace MyApp { }";
            var test = new CSharpCodeFixTest<GM0042Analyzer, GM0042CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
