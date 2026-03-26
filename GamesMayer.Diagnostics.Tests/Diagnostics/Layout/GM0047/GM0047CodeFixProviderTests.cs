namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0047CodeFixProviderTests
    {
        [Fact]
        public async Task Assignment_ValueOnNextLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter =
            {|GM0047:""value""|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter = ""value"";
    }
}";
            var test = new CSharpCodeFixTest<GM0047Analyzer, GM0047CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Declaration_ValueOnNextLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        string x =
            {|GM0047:""value""|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        string x = ""value"";
    }
}";
            var test = new CSharpCodeFixTest<GM0047Analyzer, GM0047CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Assignment_MultiLineValue_ValueStartsOnNextLine_Fix()
        {
            var testCode = @"class C
{
    string[] characters = new string[0];
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter =
            {|GM0047:characters[0]|};
    }
}";
            var fixedCode = @"class C
{
    string[] characters = new string[0];
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter = characters[0];
    }
}";
            var test = new CSharpCodeFixTest<GM0047Analyzer, GM0047CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
