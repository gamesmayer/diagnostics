namespace GamesMayer.Diagnostics.Tests
{
    using System.Linq;
    using GamesMayer.Diagnostics.Utils;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;

    public class BlankLineDetectionUtilsTests
    {
        [Fact]
        public void Lambda_WithSingleBlankLine_ReturnsTrueAndBlankLineStart()
        {
            var code = @"class C
{
    void M()
    {
        System.Func<int> f = () =>

            42;
    }
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var sourceText = tree.GetText();

            var lambda = root.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().Single();
            var bodyFirstToken = lambda.Body.GetFirstToken();

            var success = BlankLineDetectionUtils.TryGetFirstBlankLineStart(
                sourceText,
                tree,
                lambda.ArrowToken,
                bodyFirstToken,
                out var blankLineStart);

            Assert.True(success);
            Assert.Equal(sourceText.Lines[5].Start, blankLineStart);
        }

        [Fact]
        public void Lambda_WithCommentBetween_ReturnsFalse()
        {
            var code = @"class C
{
    void M()
    {
        System.Func<int> f = () =>
            // comment
            42;
    }
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var sourceText = tree.GetText();

            var lambda = root.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().Single();
            var bodyFirstToken = lambda.Body.GetFirstToken();

            var success = BlankLineDetectionUtils.TryGetFirstBlankLineStart(
                sourceText,
                tree,
                lambda.ArrowToken,
                bodyFirstToken,
                out _);

            Assert.False(success);
        }

        [Fact]
        public void InheritanceClause_WithBlankLineAfterColon_ReturnsTrue()
        {
            var code = @"interface IFoo { }

class C :

    IFoo
{
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var sourceText = tree.GetText();

            var baseList = root.DescendantNodes().OfType<BaseListSyntax>().Single();
            var firstBaseTypeToken = baseList.Types[0].GetFirstToken();

            var success = BlankLineDetectionUtils.TryGetFirstBlankLineStart(
                sourceText,
                tree,
                baseList.ColonToken,
                firstBaseTypeToken,
                out var blankLineStart);

            Assert.True(success);
            Assert.Equal(sourceText.Lines[3].Start, blankLineStart);
        }
    }
}
