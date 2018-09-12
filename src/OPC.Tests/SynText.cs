using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using Xunit;

using static OPC.Core.LexicalModule;
using static OPC.Core.SyntaxModule;
using static OPC.Core.Types;

namespace OPC.Tests
{
    public class SynText
    {
        [Fact]
        public void WhenVariableDeclaration_ShouldReturnTrue()
        {
            var span = "entero e1; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsVariableDeclaration(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenValidSimpleOperation_ShouldReturnTrue()
        {
            var span = "a = 8 + 2; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenValidComplexOperation_ShouldReturnTrue()
        {
            var span = "a = ( 8 + 2) - 5; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.True(isValid);
        }


        [Fact]
        public void WhenMinimalLogicOperation_ShouldReturnTrue()
        {
            var span = "a = !b; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenInvalidComplexOperation_ShouldReturnFalse()
        {
            var span = "a = 8 + 2) - 5; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.False(isValid);
        }

        [Fact]
        public void WhenInvalidComplexOperation2_ShouldReturnFalse()
        {
            var span = "a = (8 + 2) - 5); ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.False(isValid);
        }


        [Fact]
        public void WhenInvalidComplexOperation3_ShouldReturnFalse()
        {
            var span = "a = (8 + 2 - 5; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsAssigment(fList);
            Assert.False(isValid);
        }

        [Fact]
        public void WhenReturn_ShouldReturnTrue()
        {
            var span = "regresa e1; ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsReturn(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenValidParams_ShouldReturnTrue()
        {
            var span = "logico l1, real r1) ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsParams(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenNoParams_ShouldReturnTrue()
        {
            var span = ") ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsParams(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenInvalidParams_ShouldReturnFalse()
        {
            var span = "logico l1 real r1) ";
            var tokens = getTokens(span);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsParams(fList);
            Assert.False(isValid);
        }
        
        [Fact]
        public void WhenFullBlock_ShouldReturnTrue()
        {
            var block = "{ entero e1; } ";
            var tokens = getTokens(block);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsBlock(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenFullBlock2_ShouldReturnTrue()
        {
            var block = @"{ real e1; logico e2; a = ((e1 + e2) - e3) / func1() ^ 3; b = 4 > 4; b = b == c; b = b | c; regresa a; } ";
            var tokens = getTokens(block);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsBlock(fList);
            Assert.True(isValid);
        }

        [Fact]
        public void WhenValidFunc_ShouldReturnTrue()
        {
            var function = "entero f() { } ";
            var tokens = getTokens(function);

            var fList = ListModule.OfSeq(tokens.Item1);

            var isValid = IsFunction(fList);
            Assert.True(isValid);
        }
    }
}
