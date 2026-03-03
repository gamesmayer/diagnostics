// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1203ConstantsMustAppearBeforeFields>;

    public class GM1203UnitTests
    {
        [Fact]
        public async Task ConstBeforeField_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public const int A = 1;
    public int B = 2;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyConsts_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public const int A = 1;
    public const int B = 2;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyFields_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int A = 1;
    public int B = 2;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DifferentAccessGroups_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public const int A = 1;
    public int B = 2;
    private const int C = 3;
    private int D = 4;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstAfterField_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int B = 2;
    public const {|GM1203:int A = 1|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstAfterFieldSameAccessGroup_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int x = 1;
    private const {|GM1203:int Y = 2|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
