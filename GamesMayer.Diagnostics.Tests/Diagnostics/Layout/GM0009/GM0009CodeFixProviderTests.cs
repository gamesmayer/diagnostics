namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0009CodeFixProviderTests
    {
        [Fact]
        public async Task ClassDeclarationWithLineBreak_Fix()
        {
            var testCode = @"{|GM0009:public
class Foo|}
{
}";
            var fixedCode = @"public class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassDeclarationWithMultipleLineBreaks_Fix()
        {
            var testCode = @"{|GM0009:public
sealed
class Foo|}
{
}";
            var fixedCode = @"public sealed class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclarationWithLineBreak_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0009:private
    int field|};
}";
            var fixedCode = @"class Foo
{
    private int field;
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclarationArrayTypeWithLineBreak_Fix()
        {
            var testCode = @"class BasePriceWidget { }
class Foo
{
    {|GM0009:protected
    BasePriceWidget[] prefabs|};
}";
            var fixedCode = @"class BasePriceWidget { }
class Foo
{
    protected BasePriceWidget[] prefabs;
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclarationGenericTypeWithLineBreak_Fix()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    {|GM0009:protected readonly
    List<int> list|};
}";
            var fixedCode = @"using System.Collections.Generic;
class Foo
{
    protected readonly List<int> list;
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclarationGenericTypeSpacingPreserved_Fix()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    {|GM0009:protected
    Dictionary
    <int, string>
    dictionary|};
}";
            var fixedCode = @"using System.Collections.Generic;
class Foo
{
    protected Dictionary<int, string> dictionary;
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task PropertyDeclarationWithLineBreak_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    int Property|} { get; set; }
}";
            var fixedCode = @"class Foo
{
    public int Property { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodDeclarationWithLineBreak_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    void Method|}() { }
}";
            var fixedCode = @"class Foo
{
    public void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodDeclarationWithLineBreakBetweenTypeAndName_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0009:public void
    Method|}() { }
}";
            var fixedCode = @"class Foo
{
    public void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ConstructorDeclarationWithLineBreak_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0009:public
    Foo|}() { }
}";
            var fixedCode = @"class Foo
{
    public Foo() { }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task StructDeclarationWithLineBreak_Fix()
        {
            var testCode = @"{|GM0009:public
struct Bar|}
{
}";
            var fixedCode = @"public struct Bar
{
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task InterfaceDeclarationWithLineBreak_Fix()
        {
            var testCode = @"{|GM0009:public
interface IFoo|}
{
}";
            var fixedCode = @"public interface IFoo
{
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EventDeclarationWithLineBreak_Fix()
        {
            var testCode = @"using System;
class Foo
{
    {|GM0009:public event
    EventHandler MyEvent|} { add { } remove { } }
}";
            var fixedCode = @"using System;
class Foo
{
    public event EventHandler MyEvent { add { } remove { } }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MemberWithAttributesAndLineBreak_Fix()
        {
            var testCode = @"using System;
class Foo
{
    [Obsolete]
    {|GM0009:public
    int Property|} { get; set; }
}";
            var fixedCode = @"using System;
class Foo
{
    [Obsolete]
    public int Property { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0009Analyzer, GM0009CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
