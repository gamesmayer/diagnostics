// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1204StaticElementsMustAppearBeforeInstanceElements>;

    public class GM1204UnitTests
    {
        [Fact]
        public async Task StaticBeforeInstance_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public static void A() { }
    public void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyStatic_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public static void A() { }
    public static void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyInstance_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void A() { }
    public void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DifferentAccessGroups_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public static void A() { }
    public void B() { }
    private static void C() { }
    private void D() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticMethodAfterInstanceMethod_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void A() { }
    public static void {|GM1204:B|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticFieldAfterInstanceField_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int a;
    public static {|GM1204:int b|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstFieldAfterInstanceField_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int a;
    public const {|GM1204:int B = 1|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
