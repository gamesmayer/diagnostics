namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0070Analyzer>;

    public class GM0070AnalyzerTests
    {
        [Fact]
        public async Task NonAutoProperty_AccessorsOnSeparateLines_NoDiagnostic()
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
        public async Task NonAutoProperty_AccessorsOnSameLine_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds
    {
        get => state.worldGateTaskIds; {|GM0070:protected set => state.worldGateTaskIds = value;|}
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
        public async Task NonAutoProperty_BlockBodied_AccessorsOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X
    {
        get { return _x; } {|GM0070:set { _x = value; }|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

    }
}
