// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.Ordering.GM1216
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1216Analyzer>;

    public class GM1216AnalyzerTests
    {
        [Fact]
        public async Task RegularThenStaticThenAlias_NoDiagnostic()
        {
            var testCode = @"using System;
using static System.Math;
using Str = System.String;";
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
        public async Task OnlyStaticUsings_NoDiagnostic()
        {
            var testCode = @"using static System.Math;
using static System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticBeforeRegular_Diagnostic()
        {
            var testCode = @"using static System.Math;
{|GM1216:using System;|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StaticAfterAlias_Diagnostic()
        {
            var testCode = @"using Str = System.String;
{|GM1216:using static System.Math;|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task RegularThenAlias_NoDiagnostic()
        {
            var testCode = @"using System;
using Str = System.String;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
