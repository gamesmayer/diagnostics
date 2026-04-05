namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0077CodeFixProviderTests
    {
        [Fact]
        public async Task UnindentedStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM0077:int|} x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task OverIndentedStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
            {|GM0077:int|} x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CorrectlyIndented_NoFix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NestedBlock_UnindentedStatement_Fix()
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
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassMember_Unindented_Fix()
        {
            var testCode = @"class Foo
{
{|GM0077:private|} int _value;
}";
            var fixedCode = @"class Foo
{
    private int _value;
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PropertyAccessor_Unindented_Fix()
        {
            var testCode = @"class Foo
{
    int Value
    {
{|GM0077:get|};
        set;
    }
}";
            var fixedCode = @"class Foo
{
    int Value
    {
        get;
        set;
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LambdaStatement_UnderIndented_Fix()
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
            var fixedCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CollectionInitializer_ItemUnderIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CollectionInitializer_ItemOverIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ArrayInitializer_AsArgument_ItemUnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
        {|GM0077:1|},
            2
        });
    }

    void Use(int[] values) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            1,
            2
        });
    }

    void Use(int[] values) { }
}";
            var test = new CSharpCodeFixTest<GM0077Analyzer, GM0077CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
