// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM1209Analyzer>;

    public class GM1209AnalyzerTests
    {
        [Fact]
        public async Task RegularBeforeAlias_NoDiagnostic()
        {
            var testCode = @"using System;
using Str = System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyAliasUsings_NoDiagnostic()
        {
            var testCode = @"using Str = System.String;
using Int = System.Int32;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OnlyRegularUsings_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AliasBeforeRegular_Diagnostic()
        {
            var testCode = @"{|GM1209:using Str = System.String;|}
using System;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AliasBeforeStaticUsing_NoDiagnostic()
        {
            var testCode = @"using Str = System.String;
using static System.Math;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AliasInMiddleBeforeRegular_Diagnostic()
        {
            var testCode = @"using System;
{|GM1209:using Str = System.String;|}
using System.Collections;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
