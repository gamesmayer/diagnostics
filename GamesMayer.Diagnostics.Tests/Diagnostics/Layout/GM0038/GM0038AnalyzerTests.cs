namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0038Analyzer>;

    public class GM0038AnalyzerTests
    {
        [Fact]
        public async Task SingleLineChain_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }.Where(x => x > 1).ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineChainCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineChainSegmentUnderIndented_Diagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
        {|GM0038:.|}Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineChainSegmentOverIndented_Diagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
                {|GM0038:.|}Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleSegmentsWrongIndentation_MultipleDiagnostics()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
        {|GM0038:.|}Where(x => x > 1)
        {|GM0038:.|}ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FirstSegmentCorrectSecondWrong_SingleDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
        {|GM0038:.|}ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnStatementWithCorrectlyIndentedChain_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    int[] M()
    {
        return new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .ToArray();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnStatementWithWronglyIndentedChain_Diagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    int[] M()
    {
        return new[] { 1, 2, 3 }
        {|GM0038:.|}Where(x => x > 1)
            .ToArray();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowExpressionWithCorrectlyIndentedChain_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    int[] M() => new[] { 1, 2, 3 }
        .Where(x => x > 1)
        .ToArray();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowExpressionWithWronglyIndentedChain_Diagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    int[] M() => new[] { 1, 2, 3 }
    {|GM0038:.|}Where(x => x > 1)
        .ToArray();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionStatementWithCorrectlyIndentedChain_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<int> list = new List<int>();
        list
            .Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionStatementWithWronglyIndentedChain_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        List<int> list = new List<int>();
        list
        {|GM0038:.|}Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InlineChainPartOnSameLine_OnlyNewLineSegmentsChecked()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }.Where(x => x > 1)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedChainInArgumentWrongIndentation_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var rotationSequence = DOTween.Sequence()
            .Append
            (
                geometryTransform.DOLocalRotate(new Vector3(rotationAngle, 0f, rotationAngle), rotationDuration)
                {|GM0038:.|}SetRelative()
                {|GM0038:.|}SetEase(Ease.Linear)
            );
    }

    GeometryTransform geometryTransform;
    float rotationAngle;
    float rotationDuration;
}

class GeometryTransform
{
    public GeometryTransform DOLocalRotate(Vector3 value, float duration) => this;
    public GeometryTransform SetRelative() => this;
    public GeometryTransform SetEase(Ease ease) => this;
}

class Sequence
{
    public Sequence Append(GeometryTransform transform) => this;
}

static class DOTween
{
    public static Sequence Sequence() => new Sequence();
}

struct Vector3
{
    public Vector3(float x, float y, float z) { }
}

enum Ease
{
    Linear,
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedChainInArgumentCorrectIndentation_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var rotationSequence = DOTween.Sequence()
            .Append
            (
                geometryTransform.DOLocalRotate(new Vector3(rotationAngle, 0f, rotationAngle), rotationDuration)
                    .SetRelative()
                    .SetEase(Ease.Linear)
            );
    }

    GeometryTransform geometryTransform;
    float rotationAngle;
    float rotationDuration;
}

class GeometryTransform
{
    public GeometryTransform DOLocalRotate(Vector3 value, float duration) => this;
    public GeometryTransform SetRelative() => this;
    public GeometryTransform SetEase(Ease ease) => this;
}

class Sequence
{
    public Sequence Append(GeometryTransform transform) => this;
}

static class DOTween
{
    public static Sequence Sequence() => new Sequence();
}

struct Vector3
{
    public Vector3(float x, float y, float z) { }
}

enum Ease
{
    Linear,
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
