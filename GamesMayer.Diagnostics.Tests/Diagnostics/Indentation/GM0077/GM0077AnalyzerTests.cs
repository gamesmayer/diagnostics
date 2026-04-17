namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0077Analyzer>;

    public class GM0077AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedMethod_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UnindentedStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM0077:int|} x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OverIndentedStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
            {|GM0077:int|} x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleStatements_OneWrong_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0077:int|} y = 2;
        int z = 3;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_WrongIndent_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
{|GM0077:int|} x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockWithDirectives_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
#if SOME_DEFINE
        int x = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _value;

    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberNotIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
{|GM0077:private|} int _value;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberOverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
        {|GM0077:private|} int _value;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMember_MultipleAttributes_AllCorrect_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    [A]
    [B]
    private int _value;
}

class A : System.Attribute { }
class B : System.Attribute { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMember_MultipleAttributes_SecondOverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    [A]
          {|GM0077:[|}B]
    private int _value;
}

class A : System.Attribute { }
class B : System.Attribute { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMember_MultipleAttributes_BothWrong_Diagnostic()
        {
            var testCode = @"class Foo
{
        {|GM0077:[|}A]
          {|GM0077:[|}B]
    private int _value;
}

class A : System.Attribute { }
class B : System.Attribute { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessorCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Value
    {
        get;
        set;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessorNotIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Value
    {
{|GM0077:get|};
        set;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaStatement_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaStatement_InlineBrace_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () => { return; };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaStatement_EmptyBody_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaStatement_UnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        {|GM0077:return|};
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AnonymousMethodStatement_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = delegate
        {
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── Collection / object initializer tests ─────────────────────────────

        [Fact]
        public async Task CollectionInitializer_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new System.Collections.Generic.List<int>
        {
            1,
            2
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CollectionInitializer_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new System.Collections.Generic.List<int> { 1, 2 };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CollectionInitializer_ItemOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new System.Collections.Generic.List<int>
        {
                {|GM0077:1|},
            2
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CollectionInitializer_ItemUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new System.Collections.Generic.List<int>
        {
{|GM0077:1|},
            2
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CollectionInitializer_AsArgument_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new System.Collections.Generic.List<int>
        {
            1,
            2
        });
    }

    void Use(System.Collections.Generic.List<int> list) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CollectionInitializer_AsArgument_ItemOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new System.Collections.Generic.List<int>
        {
                {|GM0077:1|},
            2
        });
    }

    void Use(System.Collections.Generic.List<int> list) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_ItemOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
                {|GM0077:A|} = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_AsArgument_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int[] values)
    {
        Use(new int[]
        {
            1,
            2
        });
    }

    void Use(int[] values) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_AsArgument_ItemUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M(int[] values)
    {
        Use(new int[]
        {
        {|GM0077:1|},
            2
        });
    }

    void Use(int[] values) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── catch / finally tests ─────────────────────────────────────────────

        [Fact]
        public async Task CatchClause_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchClause_UnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
{|GM0077:catch|} (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchClause_OverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
                {|GM0077:catch|} (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FinallyClause_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        finally
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FinallyClause_UnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
{|GM0077:finally|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatchFinally_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }
        finally
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatchFinally_BothWrong_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
{|GM0077:catch|} (System.Exception)
        {
        }
{|GM0077:finally|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── else / else if tests ───────────────────────────────────────────────

        [Fact]
        public async Task ElseClause_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseClause_UnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
{|GM0077:else|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseClause_OverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
                {|GM0077:else|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseIfClause_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else if (false)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseIfClause_UnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
{|GM0077:else|} if (false)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseIfChain_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else if (false)
        {
        }
        else
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── comment trivia indentation tests ─────────────────────────────────

        [Fact]
        public async Task SingleLineComment_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        // comment
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineComment_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
                {|GM0077:// comment|}
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineComment_UnderIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0077:// comment|}
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineComment_BeforeClosingBrace_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        // comment
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineComment_BeforeClosingBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
                {|GM0077:// comment|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineComment_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        /* comment */
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineComment_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
                {|GM0077:/* comment */|}
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── namespace member indentation tests ────────────────────────────────

        [Fact]
        public async Task NamespaceMember_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"namespace Foo
{
    class Bar { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceMember_OverIndented_Diagnostic()
        {
            var testCode = @"namespace Foo
{
        {|GM0077:class|} Bar { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceMember_UnderIndented_Diagnostic()
        {
            var testCode = @"namespace Foo
{
{|GM0077:class|} Bar { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceMember_AttributeOverIndented_Diagnostic()
        {
            var testCode = @"namespace Foo
{
        {|GM0077:[|} System.Obsolete]
    class Bar { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceMember_AttributeCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"namespace Foo
{
    [System.Obsolete]
    class Bar { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
