// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1214Analyzer>;

    public class GM1214AnalyzerTests
    {
        [Fact]
        public async Task ReadonlyBeforeNonReadonly_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private readonly int x;
    private int y;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyReadonlyFields_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private readonly int x;
    private readonly int y;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyNonReadonlyFields_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    private int y;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DifferentAccessGroups_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public readonly int x;
    public int y;
    private readonly int z;
    private int w;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReadonlyAfterNonReadonly_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    private readonly {|GM1214:int y|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReadonlyAfterNonReadonlySameAccessGroup_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int a;
    public readonly {|GM1214:int b|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonReadonlyMethodResetsTracking_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    public void Bar() { }
    private readonly int y;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
