using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics.Utils
{
    [Flags]
    internal enum BraceCategory
    {
        None = 0,
        Types = 1 << 0,
        Methods = 1 << 1,
        Properties = 1 << 2,
        Indexers = 1 << 3,
        Events = 1 << 4,
        Accessors = 1 << 5,
        AnonymousMethods = 1 << 6,
        ControlBlocks = 1 << 7,
        AnonymousTypes = 1 << 8,
        ObjectCollectionArrayInitializers = 1 << 9,
        Lambdas = 1 << 10,
        LocalFunctions = 1 << 11,
        All = Types | Methods | Properties | Indexers | Events | Accessors | AnonymousMethods | ControlBlocks | AnonymousTypes | ObjectCollectionArrayInitializers | Lambdas | LocalFunctions,
    }

    internal static class AnalyzerConfigCategoryParser
    {
        private static readonly IReadOnlyDictionary<string, BraceCategory> CategoryFlags = new Dictionary<string, BraceCategory>
        {
            ["types"] = BraceCategory.Types,
            ["methods"] = BraceCategory.Methods,
            ["properties"] = BraceCategory.Properties,
            ["indexers"] = BraceCategory.Indexers,
            ["events"] = BraceCategory.Events,
            ["accessors"] = BraceCategory.Accessors,
            ["anonymous_methods"] = BraceCategory.AnonymousMethods,
            ["control_blocks"] = BraceCategory.ControlBlocks,
            ["anonymous_types"] = BraceCategory.AnonymousTypes,
            ["object_collection_array_initializers"] = BraceCategory.ObjectCollectionArrayInitializers,
            ["lambdas"] = BraceCategory.Lambdas,
            ["local_functions"] = BraceCategory.LocalFunctions,
        };

        public static BraceCategory GetConfiguredBraceCategories(
            SyntaxNodeAnalysisContext context,
            string optionKey)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(optionKey, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return BraceCategory.All;
            }

            return ParseBraceCategories(value);
        }

        private static BraceCategory ParseBraceCategories(string value)
        {
            var flags = BraceCategory.None;
            var tokens = value.Split(',');

            foreach (var rawToken in tokens)
            {
                var token = rawToken.Trim().ToLowerInvariant();
                if (token.Length == 0)
                {
                    continue;
                }

                if (token == "all")
                {
                    return BraceCategory.All;
                }

                if (token == "none")
                {
                    return BraceCategory.None;
                }

                if (CategoryFlags.TryGetValue(token, out var tokenFlag))
                {
                    flags |= tokenFlag;
                }
            }

            return flags;
        }
    }
}
