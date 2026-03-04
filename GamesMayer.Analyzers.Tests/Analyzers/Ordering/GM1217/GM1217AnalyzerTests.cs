// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1217Analyzer>;

    public class GM1217AnalyzerTests
    {
        [Fact]
        public async Task SystemStaticsAlphabetical_NoDiagnostic()
        {
            var testCode = @"using static System.Math;
using static System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonSystemStaticsAlphabetical_NoDiagnostic()
        {
            var testCode = @"using static Acme.Foo;
using static Zoo.Bar;";
            var test = new CSharpAnalyzerTest<GM1217Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SingleStaticUsing_NoDiagnostic()
        {
            var testCode = @"using static System.Math;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SystemStaticsOutOfOrder_Diagnostic()
        {
            var testCode = @"{|GM1217:using static System.String;|}
using static System.Math;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonSystemStaticsOutOfOrder_Diagnostic()
        {
            var testCode = @"{|GM1217:using static Zoo.Bar;|}
using static Acme.Foo;";
            var test = new CSharpAnalyzerTest<GM1217Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonSystemStaticBeforeSystemStatic_Diagnostic()
        {
            var testCode = @"{|GM1217:using static Zoo.Bar;|}
using static System.Math;";
            var test = new CSharpAnalyzerTest<GM1217Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }
    }
}
