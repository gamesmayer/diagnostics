// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GamesMayer.Analyzers.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM1212Analyzer>;

    public class GM1212AnalyzerTests
    {
        [Fact]
        public async Task GetBeforeSet_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    private int i;
    public int Prop
    {
        get { return i; }
        set { i = value; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task GetterOnly_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    private int i;
    public int Prop
    {
        get { return i; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SetterOnly_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    private int i;
    public int Prop
    {
        set { i = value; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoPropertyGetBeforeSet_NoDiagnostic()
        {
            var testCode = @"public class Foo { public int Prop { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionProperty_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    private int i;
    public int Prop => i;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SetBeforeGet_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    private int i;
    public int Prop
    {
        {|GM1212:set|} { i = value; }
        get { return i; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoPropertySetBeforeGet_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    public int Prop { {|GM1212:set|}; get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IndexerSetBeforeGet_Diagnostic()
        {
            var testCode = @"
public class Foo
{
    private int field;
    public int this[int index]
    {
        {|GM1212:set|} { field = value; }
        get { return field; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IndexerGetBeforeSet_NoDiagnostic()
        {
            var testCode = @"
public class Foo
{
    private int field;
    public int this[int index]
    {
        get { return field; }
        set { field = value; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
