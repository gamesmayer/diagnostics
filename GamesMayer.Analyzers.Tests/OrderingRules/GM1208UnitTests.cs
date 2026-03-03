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
        GamesMayer.Analyzers.GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives>;

    public class GM1208UnitTests
    {
        [Fact]
        public async Task SystemBeforeOther_NoDiagnostic()
        {
            var testCode = @"using System;
using MyLib;";
            var test = new CSharpAnalyzerTest<GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task OnlySystemUsings_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyNonSystemUsings_NoDiagnostic()
        {
            var testCode = @"using MyLib;
using OtherLib;";
            var test = new CSharpAnalyzerTest<GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives, XUnitVerifier>
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
        public async Task OtherBeforeSystem_Diagnostic()
        {
            var testCode = @"using MyLib;
{|GM1208:using System;|}";
            var test = new CSharpAnalyzerTest<GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleOtherBeforeSystem_Diagnostic()
        {
            var testCode = @"using MyLib;
using OtherLib;
{|GM1208:using System;|}";
            var test = new CSharpAnalyzerTest<GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SystemAliasDoesNotCountAsSystem_NoDiagnostic()
        {
            var testCode = @"using MyLib;
using Str = System.String;";
            var test = new CSharpAnalyzerTest<GM1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives, XUnitVerifier>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await test.RunAsync();
        }
    }
}
