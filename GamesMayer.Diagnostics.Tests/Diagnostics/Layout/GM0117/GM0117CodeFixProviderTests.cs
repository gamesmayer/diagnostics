namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0117CodeFixProviderTests
    {
        [Fact]
        public async Task FirstItem_OnOpenBraceLine_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    { {|GM0117:0|},
        1
    };
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AdjacentItems_OnSameLine_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0, {|GM0117:1|}
    };
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LocalVariable_AdjacentItems_OnSameLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0, {|GM0117:1|}
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0,
            1
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Argument_AdjacentItems_OnSameLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            0, {|GM0117:1|}
        });
    }

    void Use(int[] arr) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            0,
            1
        });
    }

    void Use(int[] arr) { }
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AnonymousObjectMember_FirstItemOnOpenBraceLine_Fix()
        {
            var testCode = @"class C
{
    object M(string id)
    {
        return new
        {
            players = new[]
            { {|GM0117:id|},
                id
            }
        };
    }
}";
            var fixedCode = @"class C
{
    object M(string id)
    {
        return new
        {
            players = new[]
            {
                id,
                id
            }
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ReturnValue_AdjacentItems_OnSameLine_Fix()
        {
            var testCode = @"class C
{
    int[] M()
    {
        return new[]
        {
            0, {|GM0117:1|}
        };
    }
}";
            var fixedCode = @"class C
{
    int[] M()
    {
        return new[]
        {
            0,
            1
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0117Analyzer, GM0117CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
