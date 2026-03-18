namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0038CodeFixProviderTests
    {
        [Fact]
        public async Task UnderIndentedSegment_Fix()
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
            var fixedCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .ToList();
    }
}";

            var test = new CSharpCodeFixTest<GM0038Analyzer, GM0038CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task OverIndentedSegment_Fix()
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
            var fixedCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .ToList();
    }
}";

            var test = new CSharpCodeFixTest<GM0038Analyzer, GM0038CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ReturnStatementUnderIndentedSegment_Fix()
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
            var fixedCode = @"using System.Linq;

class C
{
    int[] M()
    {
        return new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .ToArray();
    }
}";

            var test = new CSharpCodeFixTest<GM0038Analyzer, GM0038CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NestedChainInArgumentUnderIndentedSegment_Fix()
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
            var fixedCode = @"class C
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

            var test = new CSharpCodeFixTest<GM0038Analyzer, GM0038CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
