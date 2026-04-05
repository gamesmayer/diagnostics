namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0139CodeFixProviderTests
    {
        [Fact]
        public async Task GenericTypeIdentifierAndLessThanOnDifferentLines_Fix()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new {|GM0139:List
            <int>|}();
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
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task GenericTypeWithBlankLineBetweenIdentifierAndLessThan_Fix()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new {|GM0139:List

            <int>|}();
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
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ArrayTypeElementAndBracketOnDifferentLines_Fix()
        {
            var testCode = @"
class Test {
    public {|GM0139:string
        []|} Field;
}
";
            var fixedCode = @"
class Test {
    public string[] Field;
}
";
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NullableTypeElementAndQuestionMarkOnDifferentLines_Fix()
        {
            var testCode = @"
class Test {
    public {|GM0139:int
        ?|} Field;
}
";
            var fixedCode = @"
class Test {
    public int? Field;
}
";
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PointerTypeElementAndAsteriskOnDifferentLines_Fix()
        {
            var testCode = @"
class Test {
    unsafe public {|GM0139:int
        *|} Field;
}
";
            var fixedCode = @"
class Test {
    unsafe public int* Field;
}
";
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task QualifiedNameLeftAndDotOnDifferentLines_Fix()
        {
            var testCode = @"
class Outer { public class Inner { } }
class Test {
    public {|GM0139:Outer
        .Inner|} Field;
}
";
            var fixedCode = @"
class Outer { public class Inner { } }
class Test {
    public Outer.Inner Field;
}
";
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TupleTypeSpanningMultipleLines_Fix()
        {
            var testCode = @"
class Test {
    public {|GM0139:(int,
        string)|} Field;
}
";
            var fixedCode = @"
class Test {
    public (int, string) Field;
}
";
            var test = new CSharpCodeFixTest<GM0139Analyzer, GM0139CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
