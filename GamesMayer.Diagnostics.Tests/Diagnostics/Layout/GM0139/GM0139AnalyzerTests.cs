namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0139Analyzer>;

    public class GM0139AnalyzerTests
    {
        // GenericName

        [Fact]
        public async Task GenericTypeOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new List<int>();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericTypeIdentifierAndLessThanOnDifferentLines_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GenericTypeWithBlankLineBetweenIdentifierAndLessThan_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ArrayType

        [Fact]
        public async Task ArrayTypeOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public string[] Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayTypeElementAndBracketOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:string
        []|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayTypeWithBlankLineBetweenElementAndBracket_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:string

        []|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // NullableType

        [Fact]
        public async Task NullableTypeOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public int? Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NullableTypeElementAndQuestionMarkOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:int
        ?|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // PointerType

        [Fact]
        public async Task PointerTypeOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    unsafe public int* Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PointerTypeElementAndAsteriskOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Test {
    unsafe public {|GM0139:int
        *|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // QualifiedName

        [Fact]
        public async Task QualifiedNameOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public System.Text.StringBuilder Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task QualifiedNameLeftAndDotOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:System
        .Text|}.StringBuilder Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task QualifiedNameDotAndRightOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Outer { public class Inner { } }
class Test {
    public {|GM0139:Outer.
        Inner|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // TupleType

        [Fact]
        public async Task TupleTypeOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public (int, string) Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TupleTypeSpanningMultipleLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:(int,
        string)|} Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // AliasQualifiedName

        [Fact]
        public async Task AliasQualifiedNameOnSingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public global::System.String Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AliasQualifiedNameAliasAndColonColonOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public {|GM0139:global
        ::System|}.String Field;
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
