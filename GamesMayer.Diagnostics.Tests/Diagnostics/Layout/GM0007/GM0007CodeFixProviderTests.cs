namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0007CodeFixProviderTests
    {
        [Fact]
        public async Task MethodsWithoutBlankLine_Fix()
        {
            var testCode = @"class C
{
    void A() { }
    {|GM0007:void|} B() { }
}";

            var fixedCode = @"class C
{
    void A() { }

    void B() { }
}";

            var test = new CSharpCodeFixTest<GM0007Analyzer, GM0007CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConsecutiveFieldsWithoutBlankLine_Fix()
        {
            var testCode = @"class C
{
    private int first;
    {|GM0007:private|} int second;
}";

            var fixedCode = @"class C
{
    private int first;

    private int second;
}";

            var test = new CSharpCodeFixTest<GM0007Analyzer, GM0007CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task InterfaceMembersWithoutBlankLine_Fix()
        {
            var testCode = @"interface IWallet
{
    int Amount { get; }
    {|GM0007:void|} Earn(int amount);
}";

            var fixedCode = @"interface IWallet
{
    int Amount { get; }

    void Earn(int amount);
}";

            var test = new CSharpCodeFixTest<GM0007Analyzer, GM0007CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
