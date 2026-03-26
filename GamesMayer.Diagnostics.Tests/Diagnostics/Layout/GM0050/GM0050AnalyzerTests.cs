namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0050Analyzer>;

    public class GM0050AnalyzerTests
    {
        [Fact]
        public async Task MultilineOperatorExpressionWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"class C
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenOperatorItems_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenOperatorItems_MultipleDiagnostics()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CommentBetweenOperatorItems_NoDiagnostic()
        {
            var testCode = @"class C
{
    bool M(bool gameplay, bool showcase, bool dialogue, Character c)
    {
        return (gameplay && c.GameplayEnabled) ||
            // allow explanatory comment between items
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
