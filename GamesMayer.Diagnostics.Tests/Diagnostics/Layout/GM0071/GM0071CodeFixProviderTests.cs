namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0071CodeFixProviderTests
    {
        [Fact]
        public async Task NonAutoProperty_ExpressionBodied_Fix()
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
            var fixedCode = @"using System.Collections.Generic;

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
            var test = new CSharpCodeFixTest<GM0071Analyzer, GM0071CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonAutoProperty_BlockBodied_Fix()
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
            var fixedCode = @"class Foo
{
    private int _x;

    public int X
    {
        get { return _x; }

        set { _x = value; }
    }
}";
            var test = new CSharpCodeFixTest<GM0071Analyzer, GM0071CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
