namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0017CodeFixProviderTests
    {
        [Fact]
        public async Task IfStatement_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        bool isTrue = true;
        {|GM0017:if (
            isTrue)|}
        { }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        bool isTrue = true;
        if (isTrue)
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0017Analyzer, GM0017CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ForEachStatement_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var items = new int[0];
        {|GM0017:foreach (
            var i in items)|}
        { }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var items = new int[0];
        foreach (var i in items)
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0017Analyzer, GM0017CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CatchClause_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        try { }
        {|GM0017:catch (
            System.Exception ex)|}
        { _ = ex; }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        try { }
        catch (System.Exception ex)
        { _ = ex; }
    }
}";
            var test = new CSharpCodeFixTest<GM0017Analyzer, GM0017CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IfStatement_ExpandedCall_CollapsesWithoutSpaceBeforeParen_Fix()
        {
            var testCode = @"class Foo
{
    bool Any(int a, int b) => a == b;

    void M(int x)
    {
        {|GM0017:if (Any
        (
            x,
            1
        ))|}
        { }
    }
}";
            var fixedCode = @"class Foo
{
    bool Any(int a, int b) => a == b;

    void M(int x)
    {
        if (Any(x, 1))
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0017Analyzer, GM0017CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IfStatement_StringWithDoubleSpaces_KeepsString_Fix()
        {
            var testCode = @"class Foo
{
    void M(string s)
    {
        {|GM0017:if (s ==
            ""a  b"")|}
        { }
    }
}";
            var fixedCode = @"class Foo
{
    void M(string s)
    {
        if (s == ""a  b"")
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0017Analyzer, GM0017CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
