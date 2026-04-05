namespace GamesMayer.Diagnostics.Tests
{
    using System.Linq;
    using GamesMayer.Diagnostics.Utils;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;

    public class FluentChainUtilsTests
    {
        [Fact]
        public void GetChainRoot_WrappedInParens_ReturnsInnerExpression()
        {
            var code = @"class C { void M() { var x = (items.Where(i => i > 0)); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var paren = root.DescendantNodes().OfType<ParenthesizedExpressionSyntax>().Single();
            var result = FluentChainUtils.GetChainRoot(paren);

            Assert.IsType<InvocationExpressionSyntax>(result);
        }

        [Fact]
        public void GetChainRoot_AwaitExpression_ReturnsInnerExpression()
        {
            var code = @"class C { async System.Threading.Tasks.Task M() { var x = await items.ToListAsync(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var awaitExpr = root.DescendantNodes().OfType<AwaitExpressionSyntax>().Single();
            var result = FluentChainUtils.GetChainRoot(awaitExpr);

            Assert.IsType<InvocationExpressionSyntax>(result);
        }

        [Fact]
        public void IsFluentChainExpression_MemberAccess_ReturnsTrue()
        {
            var code = @"class C { void M() { var x = obj.Property; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single();
            Assert.True(FluentChainUtils.IsFluentChainExpression(memberAccess));
        }

        [Fact]
        public void IsFluentChainExpression_InvocationOnMemberAccess_ReturnsTrue()
        {
            var code = @"class C { void M() { obj.Method(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
            Assert.True(FluentChainUtils.IsFluentChainExpression(invocation));
        }

        [Fact]
        public void IsFluentChainExpression_PlainInvocation_ReturnsFalse()
        {
            var code = @"class C { void M() { Method(); } void Method() { } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is IdentifierNameSyntax);
            Assert.False(FluentChainUtils.IsFluentChainExpression(invocation));
        }

        [Fact]
        public void IsFluentChainStart_TopOfChain_ReturnsTrue()
        {
            var code = @"class C { void M() { items.Where(x => x > 0).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // The outermost invocation (.ToList()) is the chain start
            var outerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "ToList");

            Assert.True(FluentChainUtils.IsFluentChainStart(outerInvocation));
        }

        [Fact]
        public void IsFluentChainStart_NestedInChain_ReturnsFalse()
        {
            var code = @"class C { void M() { items.Where(x => x > 0).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // The inner invocation (.Where(...)) is NOT the chain start
            var innerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "Where");

            Assert.False(FluentChainUtils.IsFluentChainStart(innerInvocation));
        }

        [Fact]
        public void CollectFluentChainBoundaries_TwoInvocations_ReturnsTwoBoundaries()
        {
            var code = @"class C { void M() { items.Where(x => x > 0).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var outerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "ToList");

            var boundaries = new System.Collections.Generic.List<(
                Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax,
                Microsoft.CodeAnalysis.SyntaxToken,
                Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)>();

            FluentChainUtils.CollectFluentChainBoundaries(outerInvocation, boundaries);

            Assert.Equal(2, boundaries.Count);
            Assert.IsType<InvocationExpressionSyntax>(boundaries[0].Item3); // .Where(...)
            Assert.IsType<InvocationExpressionSyntax>(boundaries[1].Item3); // .ToList()
        }

        [Fact]
        public void CountChainInvocations_TwoInvocations_ReturnsTwo()
        {
            var code = @"class C { void M() { items.Where(x => x > 0).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var outerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "ToList");

            Assert.Equal(2, FluentChainUtils.CountChainInvocations(outerInvocation));
        }

        [Fact]
        public void CountChainInvocations_SingleMemberAccessNoDot_ReturnsZero()
        {
            var code = @"class C { void M() { var x = obj.Property; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single();
            Assert.Equal(0, FluentChainUtils.CountChainInvocations(memberAccess));
        }

        [Fact]
        public void CountChainInvocations_ThreeInvocations_ReturnsThree()
        {
            var code = @"class C { void M() { items.Where(x => x > 0).OrderBy(x => x).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var outerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "ToList");

            Assert.Equal(3, FluentChainUtils.CountChainInvocations(outerInvocation));
        }
    }
}
