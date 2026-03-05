namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0005CodeFixProviderTests
    {
        [Fact]
        public async Task TwoAttributesSeparatedByComma_Fix()
        {
            var testCode = @"
{|GM0005:[System.Serializable, System.Obsolete]|}
class Foo { }";
            var fixedCode = @"
[System.Serializable]
[System.Obsolete]
class Foo { }";
            var test = new CSharpCodeFixTest<GM0005Analyzer, GM0005CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeAttributesSeparatedByComma_Fix()
        {
            var testCode = @"
{|GM0005:[System.Serializable, System.Obsolete, System.CLSCompliant(true)]|}
class Foo { }";
            var fixedCode = @"
[System.Serializable]
[System.Obsolete]
[System.CLSCompliant(true)]
class Foo { }";
            var test = new CSharpCodeFixTest<GM0005Analyzer, GM0005CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AttributesOnMethod_Fix()
        {
            var testCode = @"
class Foo
{
    {|GM0005:[System.Obsolete, System.CLSCompliant(true)]|}
    public void Bar() { }
}";
            var fixedCode = @"
class Foo
{
    [System.Obsolete]
    [System.CLSCompliant(true)]
    public void Bar() { }
}";
            var test = new CSharpCodeFixTest<GM0005Analyzer, GM0005CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AttributesOnProperty_Fix()
        {
            var testCode = @"
class Foo
{
    {|GM0005:[System.Obsolete, System.CLSCompliant(true)]|}
    public int Bar { get; set; }
}";
            var fixedCode = @"
class Foo
{
    [System.Obsolete]
    [System.CLSCompliant(true)]
    public int Bar { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0005Analyzer, GM0005CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
