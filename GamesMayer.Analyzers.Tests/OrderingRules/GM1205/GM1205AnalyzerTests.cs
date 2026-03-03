// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules.GM1205
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1205Analyzer>;

    public class GM1205AnalyzerTests
    {
        [Fact]
        public async Task PublicPartialClass_NoDiagnostic()
        {
            var testCode = @"public partial class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InternalPartialClass_NoDiagnostic()
        {
            var testCode = @"internal partial class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonPartialClass_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicPartialStruct_NoDiagnostic()
        {
            var testCode = @"public partial struct Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicPartialInterface_NoDiagnostic()
        {
            var testCode = @"public partial interface IFoo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PartialClassWithoutAccess_Diagnostic()
        {
            var testCode = @"partial class {|GM1205:Foo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SealedPartialClassWithoutAccess_Diagnostic()
        {
            var testCode = @"sealed partial class {|GM1205:Foo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PartialStructWithoutAccess_Diagnostic()
        {
            var testCode = @"partial struct {|GM1205:Foo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PartialInterfaceWithoutAccess_Diagnostic()
        {
            var testCode = @"partial interface {|GM1205:IFoo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedPrivatePartialClass_NoDiagnostic()
        {
            var testCode = @"
public class Outer
{
    private partial class Inner { }
    private partial class Inner { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedPartialClassWithoutAccess_Diagnostic()
        {
            var testCode = @"
public class Outer
{
    partial class {|GM1205:Inner|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PublicPartialRecord_NoDiagnostic()
        {
            var testCode = @"public partial record Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PartialRecordWithoutAccess_Diagnostic()
        {
            var testCode = @"partial record {|GM1205:Foo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
