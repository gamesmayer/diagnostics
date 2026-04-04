namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0130Analyzer>;

    public class GM0130AnalyzerTests
    {
        [Fact]
        public async Task NoRulesConfigured_NoDiagnostic()
        {
            var testCode = @"
using MyApp.Application;

namespace MyApp.Domain
{
    class Foo { }
}

namespace MyApp.Application { }
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ProtectedNamespace_DisallowedUsing_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Application|};

namespace MyApp.Domain
{
    class Foo { }
}

namespace MyApp.Application { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ProtectedNamespace_AllowedUsing_NoDiagnostic()
        {
            var testCode = @"
using System;

namespace MyApp.Domain
{
    class Foo { }
}
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NonProtectedNamespace_DisallowedUsing_NoDiagnostic()
        {
            var testCode = @"
using MyApp.Application;

namespace MyApp.Infrastructure
{
    class Foo { }
}

namespace MyApp.Application { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application"));

            await test.RunAsync();
        }

        [Fact]
        public async Task FileScopedNamespace_DisallowedUsing_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Application|};

namespace MyApp.Domain;

class Foo { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                TestState =
                {
                    Sources =
                    {
                        "namespace MyApp.Application { }",
                    },
                },
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleDisallowedPatterns_SecondMatches_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Infrastructure|};

namespace MyApp.Domain
{
    class Foo { }
}

namespace MyApp.Infrastructure { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application, *.Infrastructure"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleRules_SecondRuleMatches_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Presentation|};

namespace MyApp.Application
{
    class Foo { }
}

namespace MyApp.Presentation { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application, *.Infrastructure | *.Application -> *.Presentation"));

            await test.RunAsync();
        }

        [Fact]
        public async Task UsingInsideNamespaceBlock_DisallowedUsing_Diagnostic()
        {
            var testCode = @"
namespace MyApp.Domain
{
    using {|GM0130:MyApp.Application|};

    class Foo { }
}

namespace MyApp.Application { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Application"));

            await test.RunAsync();
        }

        [Fact]
        public async Task DefaultCleanArchitectureConfig_DomainUsesApplication_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Application|};

namespace MyApp.Domain
{
    class Foo { }
}

namespace MyApp.Application { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Presentation, *.Application, *.Infrastructure"));

            await test.RunAsync();
        }

        [Fact]
        public async Task DefaultCleanArchitectureConfig_DomainUsesPresentation_Diagnostic()
        {
            var testCode = @"
using {|GM0130:MyApp.Presentation|};

namespace MyApp.Domain
{
    class Foo { }
}

namespace MyApp.Presentation { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Presentation, *.Application, *.Infrastructure"));

            await test.RunAsync();
        }

        [Fact]
        public async Task DefaultCleanArchitectureConfig_DomainUsesSystem_NoDiagnostic()
        {
            var testCode = @"
using System.Collections.Generic;

namespace MyApp.Domain
{
    class Foo { }
}
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Presentation, *.Application, *.Infrastructure"));

            await test.RunAsync();
        }
        [Fact]
        public async Task SurfMasters_DomainUsesApplication_Diagnostic()
        {
            var testCode = @"
using {|GM0130:SurfMasters.SavedGame.Application|};

namespace SurfMasters.SavedGame.Domain
{
    public class WorldLevelCompletedTaskDefinitionEntity { }
}

namespace SurfMasters.SavedGame.Application { }
";
            var test = new CSharpAnalyzerTest<GM0130Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0130.rules = *.Domain -> *.Presentation, *.Application, *.Infrastructure"));

            await test.RunAsync();
        }
    }
}
