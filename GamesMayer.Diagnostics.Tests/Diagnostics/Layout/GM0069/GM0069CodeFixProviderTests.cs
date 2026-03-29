namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0069CodeFixProviderTests
    {
        [Fact]
        public async Task NonAutoProperty_ExpressionBodied_Fix()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    private State state = new State();

    public List<string> WorldGateTaskIds {|GM0069:{ get => state.worldGateTaskIds; set => state.worldGateTaskIds = value; }|}
}

class State { public List<string> worldGateTaskIds = new List<string>(); }";
            var fixedCode = @"using System.Collections.Generic;

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
            var test = new CSharpCodeFixTest<GM0069Analyzer, GM0069CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonAutoProperty_ReadOnly_ExpressionBodied_Fix()
        {
            var testCode = @"class Foo
{
    private int _x;

    public int X {|GM0069:{ get => _x; }|}
}";
            var fixedCode = @"class Foo
{
    private int _x;

    public int X
    {
        get => _x;
    }
}";
            var test = new CSharpCodeFixTest<GM0069Analyzer, GM0069CodeFixProvider, XUnitVerifier>
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

    public int X {|GM0069:{ get { return _x; } set { _x = value; } }|}
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
            var test = new CSharpCodeFixTest<GM0069Analyzer, GM0069CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
