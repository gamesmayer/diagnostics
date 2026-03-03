// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules.GM1200
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1200Analyzer>;

    public class GM1200AnalyzerTests
    {
        [Fact]
        public async Task UsingAtFileLevel_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoUsings_NoDiagnostic()
        {
            var testCode = @"namespace Foo { class Bar { } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsingInsideNamespace_Diagnostic()
        {
            var testCode = @"namespace Foo
{
    {|GM1200:using System;|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleUsingsInsideNamespace_DiagnosticForEach()
        {
            var testCode = @"namespace Foo
{
    {|GM1200:using System;|}
    {|GM1200:using System.Collections;|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsingOutsideAndInsideNamespace_DiagnosticOnlyForInside()
        {
            var testCode = @"using System;
namespace Foo
{
    {|GM1200:using System.Collections;|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
