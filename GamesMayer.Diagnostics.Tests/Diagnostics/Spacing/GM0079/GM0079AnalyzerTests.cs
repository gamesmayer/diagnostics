namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0079Analyzer>;

    public class GM0079AnalyzerTests
    {
        [Fact]
        public async Task SingleSpacesBetweenTokens_NoDiagnostic()
        {
            var testCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet { get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleSpacesBeforeOpenBrace_Diagnostic()
        {
            var testCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet{|GM0079:    |}{ get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleSpacesBetweenTypeAndIdentifier_Diagnostic()
        {
            var testCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity{|GM0079:  |}SoftWallet { get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewLineBetweenTokens_NoDiagnostic()
        {
            var testCode = @"public class WalletSaveEntity
{
    public WalletSaveEntity SoftWallet
    { get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleTokenGapsOnSameLine_MultipleDiagnostics()
        {
            var testCode = @"public class WalletSaveEntity
{
    public{|GM0079:  |}WalletSaveEntity{|GM0079:   |}SoftWallet{|GM0079:    |}{ get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
