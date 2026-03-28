namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0060Analyzer>;

    public class GM0060AnalyzerTests
    {
        [Fact]
        public async Task PropertyChainOnOneLine_NoDiagnostic()
        {
            var testCode = @"class Inner { public int Value; }
class Outer { public Inner Inner = new Inner(); }

class Foo
{
    Outer outer = new Outer();

    void M()
    {
        outer.Inner.Value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // Assignment target — dot at start of new line
        [Fact]
        public async Task AssignmentTarget_DotAtStartOfNewLine_Diagnostic()
        {
            var testCode = @"class Inner { public int Value; }
class Outer { public Inner Inner = new Inner(); }

class Foo
{
    Outer outer = new Outer();

    void M()
    {
        outer
            {|GM0060:.Inner|}.Value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // Assignment target — trailing dot
        [Fact]
        public async Task AssignmentTarget_TrailingDot_Diagnostic()
        {
            var testCode = @"class Inner { public int Value; }
class Outer { public Inner Inner = new Inner(); }

class Foo
{
    Outer outer = new Outer();

    void M()
    {
        outer.Inner{|GM0060:.
            Value|} = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // Method-chain receiver — property chain split across lines
        [Fact]
        public async Task MethodChainReceiver_PropertyChainSplitAcrossLines_Diagnostic()
        {
            var testCode = @"using System;

class EarnedData { public event Action<int> AddListener = delegate { }; }
class HardWallet { public EarnedData Earned = new EarnedData(); }
class WalletSystem { public HardWallet HardWallet = new HardWallet(); }

class Foo
{
    WalletSystem walletSystem = new WalletSystem();

    void M()
    {
        walletSystem
            {|GM0060:.HardWallet|}.Earned.AddListener += OnEarned;
    }

    void OnEarned(int x) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // Method-chain receiver — multiple segments split (models the user's walletSystem example)
        [Fact]
        public async Task MethodChainReceiver_MultipleSegmentsSplit_MultipleDiagnostics()
        {
            var testCode = @"using System;

class EarnedData { public event Action<int> AddListener = delegate { }; }
class HardWallet { public EarnedData Earned = new EarnedData(); }
class WalletSystem { public HardWallet HardWallet = new HardWallet(); }

class Foo
{
    WalletSystem walletSystem = new WalletSystem();

    void M()
    {
        walletSystem
            {|GM0060:.HardWallet|}
            {|GM0060:.Earned|}.AddListener += OnEarned;
    }

    void OnEarned(int x) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
