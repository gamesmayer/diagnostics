// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules.GM1206
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1206Analyzer>;

    public class GM1206AnalyzerTests
    {
        [Fact]
        public async Task PublicStaticMethod_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public static void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicAbstractClass_NoDiagnostic()
        {
            var testCode = @"public abstract class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicStaticReadonlyField_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public static readonly int X = 1;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticBeforePublic_Diagnostic()
        {
            var testCode = @"class Foo
{
    static {|GM1206:public|} void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AbstractBeforeStatic_Diagnostic()
        {
            var testCode = @"abstract {|GM1206:static|} class Foo { }";
            var test = new CSharpAnalyzerTest<GM1206Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ReadonlyBeforeStatic_Diagnostic()
        {
            var testCode = @"class Foo
{
    readonly {|GM1206:static|} int X = 1;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticBeforePrivate_Diagnostic()
        {
            var testCode = @"class Foo
{
    static {|GM1206:private|} void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
