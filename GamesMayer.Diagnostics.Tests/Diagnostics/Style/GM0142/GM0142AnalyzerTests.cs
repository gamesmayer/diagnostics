namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0142Analyzer>;

    public class GM0142AnalyzerTests
    {
        [Fact]
        public async Task ArrayEmpty_Diagnostic()
        {
            var testCode = @"using System;
class C
{
    string[] M()
    {
        return {|GM0142:Array.Empty<string>()|};
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewArrayCreation_NoDiagnostic()
        {
            var testCode = @"class C
{
    string[] M()
    {
        return new string[0];
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayEmptyWithCustomType_Diagnostic()
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

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleArrayEmpty_MultipleDiagnostics()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        var a = {|GM0142:Array.Empty<string>()|};
        var b = {|GM0142:Array.Empty<int>()|};
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
