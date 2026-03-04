// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1213Analyzer>;

    public class GM1213AnalyzerTests
    {
        [Fact]
        public async Task AddBeforeRemove_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar
    {
        add { }
        remove { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AddOnly_NoDiagnostic()
        {
            // Single accessor — not two, so rule doesn't apply
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar
    {
        add { }
        remove { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task RemoveBeforeAdd_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar
    {
        {|GM1213:remove|} { }
        add { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task RemoveBeforeAdd_SameLine_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar { {|GM1213:remove|} { } add { } }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AddBeforeRemove_SameLine_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar { add { } remove { } }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task RemoveBeforeAddWithLineComment_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    public event System.EventHandler Bar
    {
        // remove handler
        {|GM1213:remove|} { }
        // add handler
        add { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
