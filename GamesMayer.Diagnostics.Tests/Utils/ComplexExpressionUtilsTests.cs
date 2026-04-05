namespace GamesMayer.Diagnostics.Tests
{
    using System.Linq;
    using GamesMayer.Diagnostics.Utils;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;

    public class ComplexExpressionUtilsTests
    {
        private static ExpressionSyntax ParseExpression(string expressionCode)
        {
            var code = $"class C {{ void M() {{ var x = {expressionCode}; }} }}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var variable = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single();
            return variable.Initializer!.Value;
        }

        [Fact]
        public void Lambda_ReturnsTrue()
        {
            var expr = ParseExpression("(System.Func<int>)(() => 42)");
            var lambda = expr.DescendantNodesAndSelf().OfType<LambdaExpressionSyntax>().First();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(lambda));
        }

        [Fact]
        public void AnonymousMethod_ReturnsTrue()
        {
            var expr = ParseExpression("(System.Func<int>)(delegate() { return 1; })");
            var anonMethod = expr.DescendantNodesAndSelf().OfType<AnonymousMethodExpressionSyntax>().First();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(anonMethod));
        }

        [Fact]
        public void AnonymousType_ReturnsTrue()
        {
            var expr = ParseExpression("new { Name = \"x\" }");
            Assert.True(ComplexExpressionUtils.IsComplexExpression(expr));
        }

        [Fact]
        public void ObjectCreationWithInitializer_ReturnsTrue()
        {
            var code = "class C { void M() { var x = new System.Collections.Generic.List<int> { 1, 2 }; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var objCreation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Single();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(objCreation));
        }

        [Fact]
        public void ImplicitArrayCreation_ReturnsTrue()
        {
            var code = "class C { void M() { var x = new[] { 1, 2, 3 }; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var arrayCreation = root.DescendantNodes().OfType<ImplicitArrayCreationExpressionSyntax>().Single();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(arrayCreation));
        }

        [Fact]
        public void ArrayCreationWithInitializer_ReturnsTrue()
        {
            var code = "class C { void M() { var x = new int[] { 1, 2, 3 }; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var arrayCreation = root.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Single();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(arrayCreation));
        }

        [Fact]
        public void ArrayCreationWithoutInitializer_ReturnsFalse()
        {
            var code = "class C { void M() { var x = new int[3]; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var arrayCreation = root.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Single();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(arrayCreation));
        }

        [Fact]
        public void ObjectCreationWithoutInitializer_ReturnsFalse()
        {
            var code = "class C { void M() { var x = new System.Text.StringBuilder(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var objCreation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Single();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(objCreation));
        }

        [Fact]
        public void ObjectCreationWithComplexArgument_ReturnsTrue()
        {
            // Guid.NewGuid().ToString() is a fluent chain of 2 invocations — meets default threshold
            var code = "class C { void M() { var x = new Foo(System.Guid.NewGuid().ToString(), 1, 2); } } class Foo { public Foo(string a, int b, int c) {} }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var objCreation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(objCreation));
        }

        [Fact]
        public void ObjectCreationWithSimpleArguments_ReturnsFalse()
        {
            var code = "class C { void M() { var x = new Foo(1, 2, 3); } } class Foo { public Foo(int a, int b, int c) {} }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var objCreation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Single();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(objCreation));
        }

        [Fact]
        public void SimpleInvocation_ReturnsFalse()
        {
            var code = "class C { void M() { var x = Foo(); } static int Foo() => 0; }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(invocation));
        }

        [Fact]
        public void FluentChain_BelowThreshold_ReturnsFalse()
        {
            // Single invocation — below default threshold of 2
            var code = "class C { void M() { var x = items.ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma && ma.Name.Identifier.Text == "ToList");
            Assert.False(ComplexExpressionUtils.IsComplexExpression(invocation));
        }

        [Fact]
        public void FluentChain_AtThreshold_ReturnsTrue()
        {
            // Two invocations — at default threshold of 2
            var code = "class C { void M() { var x = items.Where(i => i > 0).ToList(); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma && ma.Name.Identifier.Text == "ToList");
            Assert.True(ComplexExpressionUtils.IsComplexExpression(invocation));
        }

        [Fact]
        public void InvocationWithLambdaArgument_ReturnsTrue()
        {
            var code = "class C { void M() { var x = items.Where(i => i > 0); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma && ma.Name.Identifier.Text == "Where");
            Assert.True(ComplexExpressionUtils.IsComplexExpression(invocation));
        }

        [Fact]
        public void InvocationWithSimpleArguments_ReturnsFalse()
        {
            var code = "class C { void M() { var x = Foo(1, 2); } static int Foo(int a, int b) => a + b; }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(invocation));
        }

        [Fact]
        public void InvocationWithNestedComplexArgument_ReturnsTrue()
        {
            // Invocation whose argument is itself a fluent chain at threshold
            var code = "class C { void M() { Foo(items.Where(i => i > 0).ToList()); } static void Foo(object o) { } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var outerInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is IdentifierNameSyntax id && id.Identifier.Text == "Foo");
            Assert.True(ComplexExpressionUtils.IsComplexExpression(outerInvocation));
        }

        [Fact]
        public void BinaryExpression_WithComplexOperand_ReturnsTrue()
        {
            // String concatenation where one operand is an invocation with a lambda argument
            var code = @"class C { void M() { var x = ""prefix"" + string.Join("","", items.Select(p => p.ToString())); } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(binary));
        }

        [Fact]
        public void BinaryExpression_WithOnlySimpleOperands_ReturnsFalse()
        {
            var code = "class C { void M() { var x = 1 + 2; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().Single();
            Assert.False(ComplexExpressionUtils.IsComplexExpression(binary));
        }

        [Fact]
        public void BinaryExpression_ChainedConcatenation_WithOneComplexOperand_ReturnsTrue()
        {
            // a + complex + b — the nested binary (a + complex) has a complex right operand
            var code = @"class C { void M() { var x = ""a"" + string.Join("","", items.Select(p => p.ToString())) + ""b""; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            // Outermost binary: (... + complex + ...) + "b"
            var outerBinary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
            Assert.True(ComplexExpressionUtils.IsComplexExpression(outerBinary));
        }
    }
}
