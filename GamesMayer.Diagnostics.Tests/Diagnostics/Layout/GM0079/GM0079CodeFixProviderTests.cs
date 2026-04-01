namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0079CodeFixProviderTests
    {
        [Fact]
        public async Task MultipleSpacesBeforeOpenBrace_Fix()
        {
            var testCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet{|GM0079:    |}{ get; }
}";
            var fixedCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet { get; }
}";
            var test = new CSharpCodeFixTest<GM0079Analyzer, GM0079CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleTokenGapsOnSameLine_Fix()
        {
            var testCode = @"public class WalletSaveEntity
{
    public{|GM0079:  |}WalletSaveEntity{|GM0079:   |}SoftWallet{|GM0079:    |}{ get; }
}";
            var fixedCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet { get; }
}";
            var test = new CSharpCodeFixTest<GM0079Analyzer, GM0079CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
