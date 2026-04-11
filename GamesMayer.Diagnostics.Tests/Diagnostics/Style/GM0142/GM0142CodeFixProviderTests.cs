namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0142CodeFixProviderTests
    {
        [Fact]
        public async Task ArrayEmptyString_Fix()
        {
            var testCode = @"using System;
class C
{
    string[] M()
    {
        return {|GM0142:Array.Empty<string>()|};
    }
}";

            var fixedCode = @"using System;
class C
{
    string[] M()
    {
        return new string[0];
    }
}";

            var test = new CSharpCodeFixTest<GM0142Analyzer, GM0142CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ArrayEmptyCustomType_Fix()
        {
            var testCode = @"using System;
class TaskSettings { }
class C
{
    TaskSettings[] M()
    {
        return {|GM0142:Array.Empty<TaskSettings>()|};
    }
}";

            var fixedCode = @"using System;
class TaskSettings { }
class C
{
    TaskSettings[] M()
    {
        return new TaskSettings[0];
    }
}";

            var test = new CSharpCodeFixTest<GM0142Analyzer, GM0142CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
