﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pihrtsoft.CodeAnalysis.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LogicalNotExpressionDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticDescriptors.SimplifyLogicalNotExpression);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeLogicalNotExpression(f), SyntaxKind.LogicalNotExpression);
        }

        private void AnalyzeLogicalNotExpression(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;

            if (logicalNot.Operand?.IsAnyKind(SyntaxKind.TrueLiteralExpression, SyntaxKind.FalseLiteralExpression) == true
                && logicalNot.OperatorToken.TrailingTrivia.IsWhitespaceOrEndOfLine()
                && logicalNot.Operand.GetLeadingTrivia().IsWhitespaceOrEndOfLine())
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.SimplifyLogicalNotExpression,
                    logicalNot.GetLocation());
            }
        }
    }
}
