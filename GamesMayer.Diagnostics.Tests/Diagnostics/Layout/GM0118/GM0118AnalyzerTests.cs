namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0118Analyzer>;

    public class GM0118AnalyzerTests
    {
        [Fact]
        public async Task SingleLineArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1, 2, 3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_CommaAtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_CommaOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1
            {|GM0118:,|}
            2
            {|GM0118:,|}
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_CommaAtStartOfNextItemLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1
            {|GM0118:,|} 2
            {|GM0118:,|} 3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_CommaOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void Foo
    (
        int a
        {|GM0118:,|}
        int b
        {|GM0118:,|}
        int c
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_CommaAtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void Foo
    (
        int a,
        int b,
        int c
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributeArgumentList_CommaOnOwnLine_Diagnostic()
        {
            var testCode = @"using System;
[AttributeUsage(AttributeTargets.Class
    {|GM0118:,|} AllowMultiple = true)]
class MyAttribute : Attribute { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TypeArgumentList_CommaOnOwnLine_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class C
{
    void M()
    {
        var d = new Dictionary<int
            {|GM0118:,|} string>();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_CommaAtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    static readonly int[] x = new[]
    {
        0,
        1,
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_CommaOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    static readonly int[] x = new[]
    {
        0
        {|GM0118:,|} 1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_CommaOnOwnLineBetweenObjectInitializers_Diagnostic()
        {
            var testCode = @"class Entry { public string name; }
class C
{
    static readonly Entry[] items = new[]
    {
        new Entry { name = ""a"" }
        {|GM0118:,|}
        new Entry { name = ""b"" }
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_TrailingComma_NoDiagnostic()
        {
            var testCode = @"class Entry { public string name; public string url; }
class C
{
    static readonly Entry item = new Entry
    {
        name = ""a"",
        url = ""b"",
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
