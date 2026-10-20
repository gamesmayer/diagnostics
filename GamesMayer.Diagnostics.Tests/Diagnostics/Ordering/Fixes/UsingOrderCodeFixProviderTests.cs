namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class UsingOrderCodeFixProviderTests
    {
        [Fact]
        public async Task UnsortedUsings_SortsSystemFirstThenAliases_Fix()
        {
            var testCode = @"// File header.
using System.Text;
{|GM1210:using System.Collections;|}
using Xunit;
using Moq = System.Object;
class C { }";
            var fixedCode = @"// File header.
using System.Collections;
using System.Text;
using Xunit;
using Moq = System.Object;
class C { }";
            var test = new CSharpCodeFixTest<GM1210Analyzer, UsingOrderCodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            test.TestState.ExpectedDiagnostics.Add(Microsoft.CodeAnalysis.Testing.DiagnosticResult.CompilerError("CS0246").WithSpan(4, 7, 4, 12).WithArguments("Xunit"));
            test.FixedState.ExpectedDiagnostics.Add(Microsoft.CodeAnalysis.Testing.DiagnosticResult.CompilerError("CS0246").WithSpan(4, 7, 4, 12).WithArguments("Xunit"));
            await test.RunAsync();
        }
    }
}
