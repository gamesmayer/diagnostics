using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GamesMayer.Diagnostics.Utils
{
    public static class CommentUtils
    {
        public static bool HasCommentTrivia(SyntaxTriviaList triviaList)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
