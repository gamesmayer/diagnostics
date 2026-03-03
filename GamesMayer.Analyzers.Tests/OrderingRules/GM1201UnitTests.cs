// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests.OrderingRules
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1201ElementsMustAppearInTheCorrectOrder>;

    public class GM1201UnitTests
    {
        [Fact]
        public async Task FieldBeforeMethod_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    public void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldBeforeProperty_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int x;
    public int Y { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyBeforeMethod_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int X { get; set; }
    public void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceBeforeClass_NoDiagnostic()
        {
            var testCode = @"interface IFoo { }
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBeforeField_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Bar() { }
    private {|GM1201:int x|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBeforeProperty_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Bar() { }
    public int {|GM1201:X|} { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassBeforeInterface_Diagnostic()
        {
            var testCode = @"class Foo { }
interface {|GM1201:IFoo|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorBeforeField_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo() { }
    private {|GM1201:int x|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
