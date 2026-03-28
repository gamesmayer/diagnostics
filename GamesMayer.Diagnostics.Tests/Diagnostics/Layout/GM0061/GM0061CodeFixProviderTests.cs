namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0061CodeFixProviderTests
    {
        [Fact]
        public async Task IsNull_Fix()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if ({|GM0061:x is null|}) { }
    }
}";
            var fixedCode = @"class Foo
{
    void M(object x)
    {
        if (x == null) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IsNotNull_Fix()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if ({|GM0061:x is not null|}) { }
    }
}";
            var fixedCode = @"class Foo
{
    void M(object x)
    {
        if (x != null) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IsEnumMember_Fix()
        {
            var testCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Began|}) { }
    }
}";
            var fixedCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if (phase == Phase.Began) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IsNotEnumMember_Fix()
        {
            var testCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is not Phase.Began|}) { }
    }
}";
            var fixedCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if (phase != Phase.Began) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IsOrPattern_TwoValues_Fix()
        {
            var testCode = @"enum Phase { Began, Moved, Stationary }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Moved or Phase.Stationary|}) { }
    }
}";
            var fixedCode = @"enum Phase { Began, Moved, Stationary }

class Foo
{
    void M(Phase phase)
    {
        if (phase == Phase.Moved || phase == Phase.Stationary) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IsOrPattern_ThreeValues_Fix()
        {
            var testCode = @"enum Phase { Began, Moved, Stationary, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Moved or Phase.Stationary or Phase.Ended|}) { }
    }
}";
            var fixedCode = @"enum Phase { Began, Moved, Stationary, Ended }

class Foo
{
    void M(Phase phase)
    {
        if (phase == Phase.Moved || phase == Phase.Stationary || phase == Phase.Ended) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0061Analyzer, GM0061CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
