// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1207Analyzer>;

    public class GM1207AnalyzerTests
    {
        [Fact]
        public async Task ValidDeclaration_PublicClass_NoDiagnostic()
        {
            var testCode = @"public class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ValidDeclaration_ProtectedInternal_NoDiagnostic()
        {
            var testCode = @"
public class Outer
{
    protected internal class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ValidDeclaration_PrivateProtected_NoDiagnostic()
        {
            var testCode = @"
public class Outer
{
    private protected class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InvalidDeclaration_InternalProtected_Diagnostic()
        {
            var testCode = @"
public class Outer
{
    internal {|GM1207:protected|} class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InvalidDeclaration_ProtectedPrivate_Diagnostic()
        {
            var testCode = @"
public class Outer
{
    protected {|GM1207:private|} class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InvalidDeclaration_InternalProtectedMethod_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    internal {|GM1207:protected|} void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InvalidDeclaration_InternalProtectedProperty_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    internal {|GM1207:protected|} int Prop { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
