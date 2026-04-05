using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics.Utils
{
    public static class BlankLineDetectionUtils
    {
        public static bool TryGetFirstBlankLineStart(
            SourceText sourceText,
            SyntaxTree syntaxTree,
            SyntaxToken fromToken,
            SyntaxToken toToken,
            out int blankLineStart)
        {
            blankLineStart = -1;

            if (fromToken == default || toToken == default)
            {
                return false;
            }

            var fromLine = syntaxTree.GetLineSpan(fromToken.Span).EndLinePosition.Line;
            var toLine = syntaxTree.GetLineSpan(toToken.Span).StartLinePosition.Line;

            if (toLine <= fromLine + 1)
            {
                return false;
            }

            for (int lineIndex = fromLine + 1; lineIndex < toLine; lineIndex++)
            {
                var lineText = sourceText.ToString(sourceText.Lines[lineIndex].Span);
                if (!string.IsNullOrWhiteSpace(lineText))
                {
                    return false;
                }
            }

            blankLineStart = sourceText.Lines[fromLine + 1].Start;
            return true;
        }
    }
}