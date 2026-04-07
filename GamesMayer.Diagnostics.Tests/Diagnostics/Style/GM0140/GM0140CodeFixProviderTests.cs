namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0140CodeFixProviderTests
    {
        [Fact]
        public async Task UnusedUsing_Fix()
        {
            var testCode = @"{|GM0140:using System.Text;|}
class C { }";

            var fixedCode = @"class C { }";

            var test = new CSharpCodeFixTest<GM0140Analyzer, GM0140CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task UnusedUsing_MiddleOfList_Fix()
        {
            var testCode = @"using System;
{|GM0140:using System.Text;|}
class C
{
    void M()
    {
        Console.WriteLine();
    }
}";

            var fixedCode = @"using System;
class C
{
    void M()
    {
        Console.WriteLine();
    }
}";

            var test = new CSharpCodeFixTest<GM0140Analyzer, GM0140CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
