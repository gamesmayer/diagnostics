namespace GamesMayer.Diagnostics.Tests
{
    using System.Linq;
    using GamesMayer.Diagnostics.Utils;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;

    public class CommentUtilsTests
    {
        [Fact]
        public void HasCommentTrivia_SingleLineComment_ReturnsTrue()
        {
            var code = @"class C
{
    void M()
    {
        // comment
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var block = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body!;

            Assert.True(CommentUtils.HasCommentTrivia(block.CloseBraceToken.LeadingTrivia));
        }

        [Fact]
        public void HasCommentTrivia_MultiLineComment_ReturnsTrue()
        {
            var code = @"class C
{
    void M()
    {
        /* comment */
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var block = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body!;

            Assert.True(CommentUtils.HasCommentTrivia(block.CloseBraceToken.LeadingTrivia));
        }

        [Fact]
        public void HasCommentTrivia_SingleLineDocComment_ReturnsTrue()
        {
            var code = @"class C
{
    void M()
    {
        /// <summary>doc</summary>
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var block = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body!;

            Assert.True(CommentUtils.HasCommentTrivia(block.CloseBraceToken.LeadingTrivia));
        }

        [Fact]
        public void HasCommentTrivia_WhitespaceOnly_ReturnsFalse()
        {
            var code = @"class C
{
    void M()
    {
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var block = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body!;

            Assert.False(CommentUtils.HasCommentTrivia(block.CloseBraceToken.LeadingTrivia));
        }

        [Fact]
        public void HasCommentTrivia_EmptyTriviaList_ReturnsFalse()
        {
            var triviaList = SyntaxFactory.TriviaList();

            Assert.False(CommentUtils.HasCommentTrivia(triviaList));
        }
    }
}
