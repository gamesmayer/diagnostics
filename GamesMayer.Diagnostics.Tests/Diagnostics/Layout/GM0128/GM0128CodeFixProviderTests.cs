namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0128CodeFixProviderTests
    {
        [Fact]
        public async Task NewAndTypeOnDifferentLines_Fix()
        {
            var testCode = @"
class Foo { }
class Test {
    public void Method() {
        var x = new
            {|GM0128:Foo|}();
    }
}
";
            var fixedCode = @"
class Foo { }
class Test {
    public void Method() {
        var x = new Foo();
    }
}
";
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<GM0128Analyzer, GM0128CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NewAndTypeOnSameLine_NoFix()
        {
            var testCode = @"
class Foo { }
class Test {
    public void Method() {
        var x = new Foo();
    }
}
";
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<GM0128Analyzer, GM0128CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NewAndGenericTypeOnDifferentLines_Fix()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new
            {|GM0128:List<int>|}();
    }
}
";
            var fixedCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new List<int>();
    }
}
";
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<GM0128Analyzer, GM0128CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NewAndImplicitArrayOnDifferentLines_Fix()
        {
            var testCode = @"
class Test {
    private string[] _messages = new
        {|GM0128:[|}] { ""Message 1"" };
}
";
            var fixedCode = @"
class Test {
    private string[] _messages = new [] { ""Message 1"" };
}
";
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<GM0128Analyzer, GM0128CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NewAndImplicitArrayOnSameLine_NoFix()
        {
            var testCode = @"
class Test {
    private string[] _messages = new[] { ""Message 1"" };
}
";
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<GM0128Analyzer, GM0128CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
