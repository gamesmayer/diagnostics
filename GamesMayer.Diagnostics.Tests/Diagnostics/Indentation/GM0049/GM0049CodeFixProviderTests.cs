namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0049CodeFixProviderTests
    {
        [Fact]
        public async Task WrappedNotIndented_Fix()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
        {|GM0049:a.Length > 0;|}
    }
}";
            var fixedCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
            a.Length > 0;
    }
}";

            var test = new CSharpCodeFixTest<GM0049Analyzer, GM0049CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task WrappedOverIndented_Fix()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
                {|GM0049:a.Length > 0;|}
    }
}";
            var fixedCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
            a.Length > 0;
    }
}";

            var test = new CSharpCodeFixTest<GM0049Analyzer, GM0049CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleWrappedItems_FixAll()
        {
            var testCode = @"class C
{
    void M(string a, string b, string c)
    {
        bool value = a != null &&
        {|GM0049:a.Length > 0 &&|}
        {|GM0049:b.Length > 0 &&|}
            c != null;
    }
}";
            var fixedCode = @"class C
{
    void M(string a, string b, string c)
    {
        bool value = a != null &&
            a.Length > 0 &&
            b.Length > 0 &&
            c != null;
    }
}";

            var test = new CSharpCodeFixTest<GM0049Analyzer, GM0049CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };

            await test.RunAsync();
        }
    }
}
