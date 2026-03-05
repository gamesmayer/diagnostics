// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Diagnostics
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal enum AccessLevel
    {
        Public = 1,
        Internal = 2,
        ProtectedInternal = 3,
        Protected = 4,
        PrivateProtected = 5,
        Private = 6,
        NotSpecified = 7,
    }

    internal static class OrderingHelpers
    {
        internal static AccessLevel GetAccessLevel(SyntaxTokenList modifiers)
        {
            bool isPublic = modifiers.Any(SyntaxKind.PublicKeyword);
            bool isInternal = modifiers.Any(SyntaxKind.InternalKeyword);
            bool isProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
            bool isPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);

            if (isPublic) return AccessLevel.Public;
            if (isInternal && isProtected) return AccessLevel.ProtectedInternal;
            if (isPrivate && isProtected) return AccessLevel.PrivateProtected;
            if (isInternal) return AccessLevel.Internal;
            if (isProtected) return AccessLevel.Protected;
            if (isPrivate) return AccessLevel.Private;
            return AccessLevel.NotSpecified;
        }

        internal static string GetAccessLevelName(AccessLevel level)
        {
            return level switch
            {
                AccessLevel.Public => "public",
                AccessLevel.Internal => "internal",
                AccessLevel.ProtectedInternal => "protected internal",
                AccessLevel.Protected => "protected",
                AccessLevel.PrivateProtected => "private protected",
                AccessLevel.Private => "private",
                _ => "private",
            };
        }

        internal static Location GetNameOrIdentifierLocation(SyntaxNode member)
        {
            switch (member)
            {
                case ClassDeclarationSyntax c: return c.Identifier.GetLocation();
                case StructDeclarationSyntax s: return s.Identifier.GetLocation();
                case InterfaceDeclarationSyntax i: return i.Identifier.GetLocation();
                case EnumDeclarationSyntax e: return e.Identifier.GetLocation();
                case DelegateDeclarationSyntax d: return d.Identifier.GetLocation();
                case MethodDeclarationSyntax m: return m.Identifier.GetLocation();
                case PropertyDeclarationSyntax p: return p.Identifier.GetLocation();
                case FieldDeclarationSyntax f: return f.Declaration.GetLocation();
                case EventDeclarationSyntax ev: return ev.Identifier.GetLocation();
                case EventFieldDeclarationSyntax evf: return evf.Declaration.GetLocation();
                case ConstructorDeclarationSyntax ctor: return ctor.Identifier.GetLocation();
                case DestructorDeclarationSyntax dtor: return dtor.Identifier.GetLocation();
                case IndexerDeclarationSyntax idx: return idx.ThisKeyword.GetLocation();
                case OperatorDeclarationSyntax op: return op.OperatorToken.GetLocation();
                case ConversionOperatorDeclarationSyntax conv: return conv.Type.GetLocation();
                default: return member.GetLocation();
            }
        }

        internal static bool IsSystemUsingDirective(UsingDirectiveSyntax usingDirective)
        {
            var name = usingDirective.Name?.ToString();
            return name != null && (name == "System" || name.StartsWith("System.", StringComparison.Ordinal));
        }

        internal static bool HasNamespaceAliasQualifier(UsingDirectiveSyntax usingDirective)
        {
            return usingDirective.Name?.ToString().Contains("::") == true;
        }

        internal static bool IsPrecededByPreprocessorDirective(UsingDirectiveSyntax usingDirective)
        {
            foreach (var trivia in usingDirective.GetLeadingTrivia())
            {
                switch (trivia.Kind())
                {
                    case SyntaxKind.IfDirectiveTrivia:
                    case SyntaxKind.ElseDirectiveTrivia:
                    case SyntaxKind.ElifDirectiveTrivia:
                    case SyntaxKind.EndIfDirectiveTrivia:
                    case SyntaxKind.RegionDirectiveTrivia:
                    case SyntaxKind.EndRegionDirectiveTrivia:
                    case SyntaxKind.DefineDirectiveTrivia:
                    case SyntaxKind.UndefDirectiveTrivia:
                    case SyntaxKind.ErrorDirectiveTrivia:
                    case SyntaxKind.WarningDirectiveTrivia:
                    case SyntaxKind.LineDirectiveTrivia:
                    case SyntaxKind.PragmaWarningDirectiveTrivia:
                    case SyntaxKind.PragmaChecksumDirectiveTrivia:
                        return true;
                }
            }

            return false;
        }

        internal static int CompareNamespaces(NameSyntax firstName, NameSyntax secondName)
        {
            return string.Compare(
                GetNormalizedNamespace(firstName),
                GetNormalizedNamespace(secondName),
                StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetNormalizedNamespace(NameSyntax name)
        {
            return name?.ToString().Replace(" ", string.Empty) ?? string.Empty;
        }
    }
}
