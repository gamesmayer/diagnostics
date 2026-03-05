// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM1211Analyzer>;

    public class GM1211AnalyzerTests
    {
        [Fact]
        public async Task AliasesAlphabetical_NoDiagnostic()
        {
            var testCode = @"using A = System.Int32;
using B = System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleAlias_NoDiagnostic()
        {
            var testCode = @"using Str = System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoAliases_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeAliasesAlphabetical_NoDiagnostic()
        {
            var testCode = @"using A = System.Int32;
using B = System.String;
using C = System.Object;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AliasesOutOfAlphabeticalOrder_Diagnostic()
        {
            var testCode = @"using B = System.String;
{|GM1211:using A = System.Int32;|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThirdAliasOutOfOrder_Diagnostic()
        {
            var testCode = @"using A = System.Int32;
using C = System.Object;
{|GM1211:using B = System.String;|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
