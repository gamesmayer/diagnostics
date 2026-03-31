namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0076Analyzer>;

    public class GM0076AnalyzerTests
    {
        [Fact]
        public async Task UnityDebugLog_Diagnostic()
        {
            var testCode = @"namespace UnityEngine
{
    public static class Debug
    {
        public static void Log(object message) { }
    }
}

class Foo
{
    void M()
    {
        {|GM0076:UnityEngine.Debug.Log(""Hello"")|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UnityDebugLogWarningAndLogError_Diagnostics()
        {
            var testCode = @"namespace UnityEngine
{
    public static class Debug
    {
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }
}

class Foo
{
    void M()
    {
        {|GM0076:UnityEngine.Debug.LogWarning(""Warn"")|};
        {|GM0076:UnityEngine.Debug.LogError(""Error"")|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UnityDebugOtherMethod_NoDiagnostic()
        {
            var testCode = @"namespace UnityEngine
{
    public static class Debug
    {
        public static void LogException(System.Exception ex) { }
    }
}

class Foo
{
    void M(System.Exception ex)
    {
        UnityEngine.Debug.LogException(ex);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonUnityDebugType_NoDiagnostic()
        {
            var testCode = @"namespace MyNamespace
{
    public static class Debug
    {
        public static void Log(object message) { }
    }
}

class Foo
{
    void M()
    {
        MyNamespace.Debug.Log(""Hello"");
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
