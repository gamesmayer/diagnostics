namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0060CodeFixProviderTests
    {
        [Fact]
        public async Task AssignmentTarget_DotAtStartOfNewLine_Fix()
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
            var fixedCode = @"class Inner { public int Value; }
class Outer { public Inner Inner = new Inner(); }

class Foo
{
    Outer outer = new Outer();

    void M()
    {
        outer.Inner.Value = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0060Analyzer, GM0060CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AssignmentTarget_TrailingDot_Fix()
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
            var fixedCode = @"class Inner { public int Value; }
class Outer { public Inner Inner = new Inner(); }

class Foo
{
    Outer outer = new Outer();

    void M()
    {
        outer.Inner.Value = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0060Analyzer, GM0060CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodChainReceiver_MultipleSegmentsSplit_Fix()
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
            var fixedCode = @"using System;

class EarnedData { public event Action<int> AddListener = delegate { }; }
class HardWallet { public EarnedData Earned = new EarnedData(); }
class WalletSystem { public HardWallet HardWallet = new HardWallet(); }

class Foo
{
    WalletSystem walletSystem = new WalletSystem();

    void M()
    {
        walletSystem.HardWallet.Earned.AddListener += OnEarned;
    }

    void OnEarned(int x) { }
}";
            var test = new CSharpCodeFixTest<GM0060Analyzer, GM0060CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
