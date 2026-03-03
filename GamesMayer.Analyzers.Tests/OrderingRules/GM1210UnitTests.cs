// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace>;

    public class GM1210UnitTests
    {
        [Fact]
        public async Task SystemUsingsAlphabetical_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;
using System.Text;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonSystemUsingsAlphabetical_NoDiagnostic()
        {
            var testCode = @"using Acme;
using Zoo;";
            var test = new CSharpAnalyzerTest<GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SingleUsing_NoDiagnostic()
        {
            var testCode = @"using System;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SystemOutOfAlphabeticalOrder_Diagnostic()
        {
            var testCode = @"using System.Text;
{|GM1210:using System;|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonSystemOutOfAlphabeticalOrder_Diagnostic()
        {
            var testCode = @"using Zoo;
{|GM1210:using Acme;|}";
            var test = new CSharpAnalyzerTest<GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AliasUsingsSkipped_NoDiagnostic()
        {
            var testCode = @"using Zoo;
using Str = System.String;
using Acme;";
            var test = new CSharpAnalyzerTest<GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task StaticUsingsSkipped_NoDiagnostic()
        {
            var testCode = @"using Zoo;
using static System.Math;
using Acme;";
            var test = new CSharpAnalyzerTest<GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }
    }
}
