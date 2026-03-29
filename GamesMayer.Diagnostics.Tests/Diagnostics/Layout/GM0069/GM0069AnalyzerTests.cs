namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0069Analyzer>;

    public class GM0069AnalyzerTests
    {
        [Fact]
        public async Task AutoProperty_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool ShowLabel { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoProperty_SingleAccessor_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool ShowLabel { get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_ExpressionBodied_SingleLine_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds {|GM0069:{ get => state.worldGateTaskIds; set => state.worldGateTaskIds = value; }|}
}

class State { public List<string> worldGateTaskIds = new List<string>(); }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_ExpressionBodied_MultiLine_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds
    {
        get => state.worldGateTaskIds;
        set => state.worldGateTaskIds = value;
    }
}

class State { public List<string> worldGateTaskIds = new List<string>(); }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_BlockBodied_SingleLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X {|GM0069:{ get { return _x; } set { _x = value; } }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_ReadOnly_ExpressionBodied_SingleLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X {|GM0069:{ get => _x; }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_MixedAccessors_SingleLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X {|GM0069:{ get => _x; protected set => _x = value; }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
