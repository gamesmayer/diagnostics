namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0050CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenOperatorItems_Fix()
        {
            var testCode = @"class C
{
    bool M(bool gameplay, bool showcase, bool dialogue, Character c)
    {
        return (gameplay && c.GameplayEnabled) ||
{|GM0050:
|}            (showcase && c.ShowcaseEnabled) ||
            (dialogue && c.DialogueEnabled);
    }
}

class Character
{
    public bool GameplayEnabled { get; set; }
    public bool ShowcaseEnabled { get; set; }
    public bool DialogueEnabled { get; set; }
}";
            var fixedCode = @"class C
{
    bool M(bool gameplay, bool showcase, bool dialogue, Character c)
    {
        return (gameplay && c.GameplayEnabled) ||
            (showcase && c.ShowcaseEnabled) ||
            (dialogue && c.DialogueEnabled);
    }
}

class Character
{
    public bool GameplayEnabled { get; set; }
    public bool ShowcaseEnabled { get; set; }
    public bool DialogueEnabled { get; set; }
}";

            var test = new CSharpCodeFixTest<GM0050Analyzer, GM0050CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenOperatorItems_Fix()
        {
            var testCode = @"class C
{
    bool M(bool a, bool b, bool c, Character item)
    {
        return (a && item.GameplayEnabled) ||
{|GM0050:
|}{|GM0050:
|}            (b && item.ShowcaseEnabled) ||
            (c && item.DialogueEnabled);
    }
}

class Character
{
    public bool GameplayEnabled { get; set; }
    public bool ShowcaseEnabled { get; set; }
    public bool DialogueEnabled { get; set; }
}";
            var fixedCode = @"class C
{
    bool M(bool a, bool b, bool c, Character item)
    {
        return (a && item.GameplayEnabled) ||
            (b && item.ShowcaseEnabled) ||
            (c && item.DialogueEnabled);
    }
}

class Character
{
    public bool GameplayEnabled { get; set; }
    public bool ShowcaseEnabled { get; set; }
    public bool DialogueEnabled { get; set; }
}";

            var test = new CSharpCodeFixTest<GM0050Analyzer, GM0050CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };

            await test.RunAsync();
        }
    }
}
