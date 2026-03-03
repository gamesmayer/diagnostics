// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.Ordering.GM1202
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1202Analyzer>;

    public class GM1202AnalyzerTests
    {
        [Fact]
        public async Task PublicBeforePrivate_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void A() { }
    private void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicBeforeInternal_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void A() { }
    internal void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DifferentKinds_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    public void A() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PrivateBeforePublicMethods_Diagnostic()
        {
            var testCode = @"class Foo
{
    private void A() { }
    public void {|GM1202:B|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InternalBeforePublicMethods_Diagnostic()
        {
            var testCode = @"class Foo
{
    internal void A() { }
    public void {|GM1202:B|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PrivateBeforePublicFields_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int a;
    public {|GM1202:int b|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PrivateBeforeProtectedMethods_Diagnostic()
        {
            var testCode = @"class Foo
{
    private void A() { }
    protected void {|GM1202:B|}() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
