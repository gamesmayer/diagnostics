namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0071Analyzer>;

    public class GM0071AnalyzerTests
    {
        [Fact]
        public async Task NonAutoProperty_AccessorsSeparatedByBlankLine_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds
    {
        get => state.worldGateTaskIds;

        protected set => state.worldGateTaskIds = value;
    }
}

class State { public List<string> worldGateTaskIds = new List<string>(); }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_AccessorsWithoutBlankLine_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds
    {
        get => state.worldGateTaskIds;
        {|GM0071:protected|} set => state.worldGateTaskIds = value;
    }
}

class State { public List<string> worldGateTaskIds = new List<string>(); }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

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
        public async Task NonAutoProperty_SingleAccessor_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X
    {
        get => _x;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_BlockBodied_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X
    {
        get { return _x; }
        {|GM0071:set|} { _x = value; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoProperty_BlockBodied_WithBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X
    {
        get { return _x; }

        set { _x = value; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
