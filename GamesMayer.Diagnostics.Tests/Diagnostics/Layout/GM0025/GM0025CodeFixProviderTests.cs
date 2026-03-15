namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0025CodeFixProviderTests
    {
        [Fact]
        public async Task MiddleArgNotOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1, {|GM0025:2,|}
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0025Analyzer, GM0025CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LastArgNotOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2, {|GM0025:3|});
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0025Analyzer, GM0025CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ParameterNotOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void M(
        int a, {|GM0025:int b,|}
        int c)
    {
    }
}";
            var fixedCode = @"class C
{
    void M(
        int a,
        int b,
        int c)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0025Analyzer, GM0025CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LambdaArgumentNotOnOwnLine_Fix()
        {
            var testCode = @"using System;

class C
{
    void M(LevelSystem levelSystem, State state)
    {
        Array.ForEach(levelSystem.Collectables, {|GM0025:collectable =>
        {
            var collectableEntity = new GameplayCollectableEntity(Guid.NewGuid().ToString(), collectable.type, collectable.position, collectable.amount);
            state.collectables.Add(collectableEntity);
        }|});
    }
}

class LevelSystem
{
    public Collectable[] Collectables { get; set; } = Array.Empty<Collectable>();
}

class State
{
    public System.Collections.Generic.List<GameplayCollectableEntity> collectables { get; } = new System.Collections.Generic.List<GameplayCollectableEntity>();
}

class Collectable
{
    public string type = string.Empty;
    public Position position = new Position();
    public int amount;
}

class Position
{
}

class GameplayCollectableEntity
{
    public GameplayCollectableEntity(string id, string type, Position position, int amount)
    {
    }
}";
            var fixedCode = @"using System;

class C
{
    void M(LevelSystem levelSystem, State state)
    {
        Array.ForEach(levelSystem.Collectables,
            collectable =>
        {
            var collectableEntity = new GameplayCollectableEntity(Guid.NewGuid().ToString(), collectable.type, collectable.position, collectable.amount);
            state.collectables.Add(collectableEntity);
        });
    }
}

class LevelSystem
{
    public Collectable[] Collectables { get; set; } = Array.Empty<Collectable>();
}

class State
{
    public System.Collections.Generic.List<GameplayCollectableEntity> collectables { get; } = new System.Collections.Generic.List<GameplayCollectableEntity>();
}

class Collectable
{
    public string type = string.Empty;
    public Position position = new Position();
    public int amount;
}

class Position
{
}

class GameplayCollectableEntity
{
    public GameplayCollectableEntity(string id, string type, Position position, int amount)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0025Analyzer, GM0025CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
