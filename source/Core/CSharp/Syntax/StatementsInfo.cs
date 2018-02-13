﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Syntax
{
    public struct StatementsInfo : IEquatable<StatementsInfo>, IReadOnlyList<StatementSyntax>
    {
        internal StatementsInfo(BlockSyntax block)
        {
            Debug.Assert(block != null);

            Node = block;
            IsBlock = true;
            Statements = block.Statements;
        }

        internal StatementsInfo(SwitchSectionSyntax switchSection)
        {
            Debug.Assert(switchSection != null);

            Node = switchSection;
            IsBlock = false;
            Statements = switchSection.Statements;
        }

        private static StatementsInfo Default { get; } = new StatementsInfo();

        public CSharpSyntaxNode Node { get; }

        public SyntaxList<StatementSyntax> Statements { get; }

        public bool IsBlock { get; }

        public bool IsSwitchSection
        {
            get { return Success && !IsBlock; }
        }

        public BlockSyntax Block
        {
            get { return (IsBlock) ? (BlockSyntax)Node : null; }
        }

        public SwitchSectionSyntax SwitchSection
        {
            get { return (IsSwitchSection) ? (SwitchSectionSyntax)Node : null; }
        }

        public bool Success
        {
            get { return Node != null; }
        }

        public int Count
        {
            get { return Statements.Count; }
        }

        public StatementSyntax this[int index]
        {
            get { return Statements[index]; }
        }

        IEnumerator<StatementSyntax> IEnumerable<StatementSyntax>.GetEnumerator()
        {
            return ((IReadOnlyList<StatementSyntax>)Statements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyList<StatementSyntax>)Statements).GetEnumerator();
        }

        public SyntaxList<StatementSyntax>.Enumerator GetEnumerator()
        {
            return Statements.GetEnumerator();
        }

        internal static StatementsInfo Create(BlockSyntax block)
        {
            if (block == null)
                return Default;

            return new StatementsInfo(block);
        }

        internal static StatementsInfo Create(SwitchSectionSyntax switchSection)
        {
            if (switchSection == null)
                return Default;

            return new StatementsInfo(switchSection);
        }

        internal static StatementsInfo Create(StatementSyntax statement)
        {
            if (statement == null)
                return Default;

            SyntaxNode parent = statement.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.Block:
                    return new StatementsInfo((BlockSyntax)parent);
                case SyntaxKind.SwitchSection:
                    return new StatementsInfo((SwitchSectionSyntax)parent);
                default:
                    return Default;
            }
        }

        internal static StatementsInfo Create(StatementsSelection selectedStatements)
        {
            return Create(selectedStatements?.UnderlyingList.FirstOrDefault());
        }

        public StatementsInfo WithStatements(IEnumerable<StatementSyntax> statements)
        {
            return WithStatements(List(statements));
        }

        public StatementsInfo WithStatements(SyntaxList<StatementSyntax> statements)
        {
            ThrowInvalidOperationIfNotInitialized();

            if (IsBlock)
                return new StatementsInfo(Block.WithStatements(statements));

            if (IsSwitchSection)
                return new StatementsInfo(SwitchSection.WithStatements(statements));

            throw new InvalidOperationException();
        }

        public StatementsInfo RemoveNode(SyntaxNode node, SyntaxRemoveOptions options)
        {
            ThrowInvalidOperationIfNotInitialized();

            if (IsBlock)
                return new StatementsInfo(Block.RemoveNode(node, options));

            if (IsSwitchSection)
                return new StatementsInfo(SwitchSection.RemoveNode(node, options));

            throw new InvalidOperationException();
        }

        public StatementsInfo ReplaceNode(SyntaxNode oldNode, SyntaxNode newNode)
        {
            ThrowInvalidOperationIfNotInitialized();

            if (IsBlock)
                return new StatementsInfo(Block.ReplaceNode(oldNode, newNode));

            if (IsSwitchSection)
                return new StatementsInfo(SwitchSection.ReplaceNode(oldNode, newNode));

            throw new InvalidOperationException();
        }

        public StatementsInfo Add(StatementSyntax statement)
        {
            return WithStatements(Statements.Add(statement));
        }

        public StatementsInfo AddRange(IEnumerable<StatementSyntax> statements)
        {
            return WithStatements(Statements.AddRange(statements));
        }

        public bool Any()
        {
            return Statements.Any();
        }

        public StatementSyntax First()
        {
            return Statements.First();
        }

        public StatementSyntax FirstOrDefault()
        {
            return Statements.FirstOrDefault();
        }

        public int IndexOf(Func<StatementSyntax, bool> predicate)
        {
            return Statements.IndexOf(predicate);
        }

        public int IndexOf(StatementSyntax statement)
        {
            return Statements.IndexOf(statement);
        }

        public StatementsInfo Insert(int index, StatementSyntax statement)
        {
            return WithStatements(Statements.Insert(index, statement));
        }

        public StatementsInfo InsertRange(int index, IEnumerable<StatementSyntax> statements)
        {
            return WithStatements(Statements.InsertRange(index, statements));
        }

        public StatementSyntax Last()
        {
            return Statements.Last();
        }

        public int LastIndexOf(Func<StatementSyntax, bool> predicate)
        {
            return Statements.LastIndexOf(predicate);
        }

        public int LastIndexOf(StatementSyntax statement)
        {
            return Statements.LastIndexOf(statement);
        }

        public StatementSyntax LastOrDefault()
        {
            return Statements.LastOrDefault();
        }

        public StatementsInfo Remove(StatementSyntax statement)
        {
            return WithStatements(Statements.Remove(statement));
        }

        public StatementsInfo RemoveAt(int index)
        {
            return WithStatements(Statements.RemoveAt(index));
        }

        public StatementsInfo Replace(StatementSyntax nodeInList, StatementSyntax newNode)
        {
            return WithStatements(Statements.Replace(nodeInList, newNode));
        }

        public StatementsInfo ReplaceAt(int index, StatementSyntax newNode)
        {
            return WithStatements(Statements.ReplaceAt(index, newNode));
        }

        public StatementsInfo ReplaceRange(StatementSyntax nodeInList, IEnumerable<StatementSyntax> newNodes)
        {
            return WithStatements(Statements.ReplaceRange(nodeInList, newNodes));
        }

        private void ThrowInvalidOperationIfNotInitialized()
        {
            if (Node == null)
                throw new InvalidOperationException($"{nameof(StatementsInfo)} is not initalized.");
        }

        public override string ToString()
        {
            return Node?.ToString() ?? base.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is StatementsInfo other && Equals(other);
        }

        public bool Equals(StatementsInfo other)
        {
            return EqualityComparer<CSharpSyntaxNode>.Default.Equals(Node, other.Node);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<CSharpSyntaxNode>.Default.GetHashCode(Node);
        }

        public static bool operator ==(StatementsInfo info1, StatementsInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(StatementsInfo info1, StatementsInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
