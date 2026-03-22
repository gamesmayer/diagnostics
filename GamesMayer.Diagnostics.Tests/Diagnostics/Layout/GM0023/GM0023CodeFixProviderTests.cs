namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0023CodeFixProviderTests
    {
        [Fact]
        public async Task WronglyIndentedArgument_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
        {|GM0023:2|},
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
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task WronglyIndentedParameter_Fix()
        {
            var testCode = @"class C
{
    void M(
        int a,
    {|GM0023:int b|},
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
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task WronglyIndentedNamedArgument_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
        {|GM0023:b: 2|},
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
            b: 2,
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task WronglyIndentedLambdaArgument_Fix()
        {
            var testCode = @"using System.Linq;

class C
{
    void M(Definition definition, WorldSystem worldSystem)
    {
        var levels = definition.Levels
            .Select
            (
            {|GM0023:levelDefinition => new WorldMapLevelState
            (
                definition: levelDefinition,
                key: WorldLevelKeyService.GetLevelKey(definition.Key, levelDefinition.Level),
                isRevealed: worldSystem.RevealLevelService.IsRevealed(definition.Key, levelDefinition.Level),
                isUnlocked: worldSystem.UnlockLevelService.IsUnlocked(definition.Key, levelDefinition.Level),
                isCompleted: worldSystem.CompleteLevelService.IsCompleted(definition.Key, levelDefinition.Level)
            )|}
            )
            .ToList();
    }

    class Definition
    {
        public string Key { get; set; }
        public LevelDefinition[] Levels { get; set; }
    }

    class LevelDefinition
    {
        public int Level { get; set; }
    }

    class WorldMapLevelState
    {
        public WorldMapLevelState(LevelDefinition definition, string key, bool isRevealed, bool isUnlocked, bool isCompleted) { }
    }

    static class WorldLevelKeyService
    {
        public static string GetLevelKey(string key, int level) => key + level;
    }

    class WorldSystem
    {
        public RevealLevelServiceType RevealLevelService { get; } = new RevealLevelServiceType();
        public UnlockLevelServiceType UnlockLevelService { get; } = new UnlockLevelServiceType();
        public CompleteLevelServiceType CompleteLevelService { get; } = new CompleteLevelServiceType();
    }

    class RevealLevelServiceType
    {
        public bool IsRevealed(string key, int level) => true;
    }

    class UnlockLevelServiceType
    {
        public bool IsUnlocked(string key, int level) => true;
    }

    class CompleteLevelServiceType
    {
        public bool IsCompleted(string key, int level) => true;
    }
}";
            var fixedCode = @"using System.Linq;

class C
{
    void M(Definition definition, WorldSystem worldSystem)
    {
        var levels = definition.Levels
            .Select
            (
                levelDefinition => new WorldMapLevelState
            (
                definition: levelDefinition,
                key: WorldLevelKeyService.GetLevelKey(definition.Key, levelDefinition.Level),
                isRevealed: worldSystem.RevealLevelService.IsRevealed(definition.Key, levelDefinition.Level),
                isUnlocked: worldSystem.UnlockLevelService.IsUnlocked(definition.Key, levelDefinition.Level),
                isCompleted: worldSystem.CompleteLevelService.IsCompleted(definition.Key, levelDefinition.Level)
            )
            )
            .ToList();
    }

    class Definition
    {
        public string Key { get; set; }
        public LevelDefinition[] Levels { get; set; }
    }

    class LevelDefinition
    {
        public int Level { get; set; }
    }

    class WorldMapLevelState
    {
        public WorldMapLevelState(LevelDefinition definition, string key, bool isRevealed, bool isUnlocked, bool isCompleted) { }
    }

    static class WorldLevelKeyService
    {
        public static string GetLevelKey(string key, int level) => key + level;
    }

    class WorldSystem
    {
        public RevealLevelServiceType RevealLevelService { get; } = new RevealLevelServiceType();
        public UnlockLevelServiceType UnlockLevelService { get; } = new UnlockLevelServiceType();
        public CompleteLevelServiceType CompleteLevelService { get; } = new CompleteLevelServiceType();
    }

    class RevealLevelServiceType
    {
        public bool IsRevealed(string key, int level) => true;
    }

    class UnlockLevelServiceType
    {
        public bool IsUnlocked(string key, int level) => true;
    }

    class CompleteLevelServiceType
    {
        public bool IsCompleted(string key, int level) => true;
    }
}";
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
