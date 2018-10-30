﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Metrics;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class LogicalLinesOfCodeCommandExecutor : AbstractLinesOfCodeCommandExecutor
    {
        public LogicalLinesOfCodeCommandExecutor(LogicalLinesOfCodeCommandLineOptions options, string language) : base(language)
        {
            Options = options;
        }

        public LogicalLinesOfCodeCommandLineOptions Options { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            var codeMetricsOptions = new CodeMetricsOptions(includeGenerated: Options.IncludeGeneratedCode);

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                CodeMetricsCounter counter = CodeMetricsCounter.GetLogicalLinesCounter(project.Language);

                if (counter != null)
                {
                    WriteLine($"Count logical lines for '{project.FilePath}'", ConsoleColor.Cyan, Verbosity.Minimal);

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    CodeMetrics metrics = await counter.CountLinesAsync(project, codeMetricsOptions, cancellationToken);

                    stopwatch.Stop();

                    WriteLine(Verbosity.Minimal);

                    WriteMetrics(
                        metrics.CodeLineCount,
                        metrics.WhitespaceLineCount,
                        metrics.CommentLineCount,
                        metrics.PreprocessorDirectiveLineCount,
                        metrics.TotalLineCount);

                    WriteLine(Verbosity.Minimal);
                    WriteLine($"Done counting logical lines for '{project.FilePath}' {stopwatch.Elapsed:mm\\:ss\\.ff}", ConsoleColor.Green, Verbosity.Normal);
                }
                else
                {
                    WriteLine($"Cannot count logical lines for language '{project.Language}'", ConsoleColor.Yellow, Verbosity.Minimal);
                }
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                WriteLine($"Count logical lines for solution '{solution.FilePath}'", ConsoleColor.Cyan, Verbosity.Minimal);

                IEnumerable<Project> projects = FilterProjects(solution, Options);

                Stopwatch stopwatch = Stopwatch.StartNew();

                ImmutableDictionary<ProjectId, CodeMetrics> projectsMetrics = CodeMetricsCounter.CountLogicalLinesInParallel(projects, codeMetricsOptions, cancellationToken);

                stopwatch.Stop();

                if (projectsMetrics.Count > 0)
                {
                    WriteLine(Verbosity.Normal);
                    WriteLine("Logical lines of code by project:", Verbosity.Normal);

                    WriteLinesOfCode(solution, projectsMetrics);
                }

                WriteLine(Verbosity.Minimal);
                WriteLine("Summary:", Verbosity.Minimal);

                WriteMetrics(
                    projectsMetrics.Sum(f => f.Value.CodeLineCount),
                    projectsMetrics.Sum(f => f.Value.WhitespaceLineCount),
                    projectsMetrics.Sum(f => f.Value.CommentLineCount),
                    projectsMetrics.Sum(f => f.Value.PreprocessorDirectiveLineCount),
                    projectsMetrics.Sum(f => f.Value.TotalLineCount));

                WriteLine(Verbosity.Minimal);
                WriteLine($"Done counting logical lines for solution '{solution.FilePath}' {stopwatch.Elapsed:mm\\:ss\\.ff}", ConsoleColor.Green, Verbosity.Normal);
            }

            return new CommandResult(true);
        }

        private static void WriteMetrics(
            int totalCodeLineCount,
            int totalWhitespaceLineCount,
            int totalCommentLineCount,
            int totalPreprocessorDirectiveLineCount,
            int totalLineCount)
        {
            string totalCodeLines = totalCodeLineCount.ToString("n0");
            string totalWhitespaceLines = totalWhitespaceLineCount.ToString("n0");
            string totalCommentLines = totalCommentLineCount.ToString("n0");
            string totalPreprocessorDirectiveLines = totalPreprocessorDirectiveLineCount.ToString("n0");
            string totalLines = totalLineCount.ToString("n0");

            int maxDigits = Math.Max(totalCodeLines.Length,
                Math.Max(totalWhitespaceLines.Length,
                    Math.Max(totalCommentLines.Length,
                        Math.Max(totalPreprocessorDirectiveLines.Length, totalLines.Length))));

            WriteLine($"{totalCodeLines.PadLeft(maxDigits)} {totalCodeLineCount / (double)totalLineCount,4:P0} logical lines of code", Verbosity.Minimal);
            WriteLine($"{totalWhitespaceLines.PadLeft(maxDigits)} {totalWhitespaceLineCount / (double)totalLineCount,4:P0} white-space lines", Verbosity.Minimal);
            WriteLine($"{totalCommentLines.PadLeft(maxDigits)} {totalCommentLineCount / (double)totalLineCount,4:P0} comment lines", Verbosity.Minimal);
            WriteLine($"{totalPreprocessorDirectiveLines.PadLeft(maxDigits)} {totalPreprocessorDirectiveLineCount / (double)totalLineCount,4:P0} preprocessor directive lines", Verbosity.Minimal);
            WriteLine($"{totalLines.PadLeft(maxDigits)} {totalLineCount / (double)totalLineCount,4:P0} total lines", Verbosity.Minimal);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Logical lines counting was canceled.", Verbosity.Quiet);
        }
    }
}