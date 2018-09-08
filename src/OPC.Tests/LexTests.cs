using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Xunit;

using static OPC.Core.LexicalModule;
using static OPC.Core.Types;

namespace OPC.Tests
{
    public class LexTests
    {
        [Fact]
        public void WhenKeyword_ShouldReturnToken()
        {
            var result = isKeyword("principal");

            Assert.True(result.IsKeyword);
        }

        [Fact]
        public void WhenKeywords_ShouldReturnToken()
        {
            var keywords = new string[] { "si", "principal", "entero", "real", "logico", "mientras", "regresa", "verdadero", "falso" };
            var failed = keywords.Select(word => isKeyword(word))
                    .Select(result => result.IsKeyword)
                    .Any(result => result != true);

            Assert.False(failed);
        }

        [Fact]
        private void WhenNoKeyword_ShouldReturnNone()
        {
            var result = isKeyword("aaaaa");

            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenSeparator_ShouldReturnToken()
        {
            var result = isPunctuation("(");
            Assert.True(result.IsPunctuation);
        }
        
        [Fact]
        public void WhenNotSeparator_ShouldReturnNone()
        {
            var result = isPunctuation("a");
            
            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenOperator_ShouldReturnOperator()
        {
            var result = isOperator("+", false);

            Assert.True(result.IsOperator);
        }

        [Fact]
        public void WhenEqualsAndFalse_ShouldReturnNone()
        {
            var result = isOperator("=", false);

            Assert.True(result.IsOperator);
        }

        [Fact]
        public void WhenEqualsAndTrue_ShouldReturnOperator()
        {
            var result = isOperator("==", true);

            Assert.True(result.IsOperator);
        }

        [Fact]
        public void WhenRealNumber_ShouldReturnConstant()
        {
            var result = isNumber("486.48");

            Assert.True(result.IsConstant);
        }

        [Fact]
        public void WhenIntegerNumber_ShouldReturnConstant()
        {
            var result = isNumber("4686");

            Assert.True(result.IsConstant);
        }

        [Fact]
        public void WhenInvalidIntegerNumber_ShouldReturnError()
        {
            var result = isNumber("4845a");
            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenInvalidRealNumber_ShouldReturnError()
        {
            var result = isNumber("48.45a");
            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenInvalidReal2Number_ShouldReturnError()
        {
            var result = isNumber("48.45.48");
            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenIdentifier_ShouldReturnIdentifier()
        {
            var result = isIdentifier("as2");
            Assert.True(result.IsIdentifier);
        }

        [Fact]
        public void WhenInvalidIdentifierWithNumber_ShouldReturnError()
        {
            var result = isIdentifier("1as");
            Assert.True(result.IsError);
        }

        [Fact]
        public void WhenInvalidIdentifierWithSymbol_ShouldReturnError()
        {
            var result = isIdentifier("$as");
            Assert.True(result.IsError);
        }

        [Fact]
        public void TestFullString()
        {
            var buffer = new StringBuilder();
            var tokens = new List<Tokens>();
            var str = "principal ".AsSpan();
            processSpan(str, buffer, tokens);

            Assert.True(tokens.Count != 0);
        }

        [Fact]
        public void TestFullLine()
        {
            var str = "entero a;".AsSpan();
            var (tokens, symbols) = getTokens(str);

            Assert.True(tokens.Count == 3);
        }

        [Fact]
        public void TestTripleEquals()
        {
            var str = "=== ".AsSpan();
            var (tokens, symbols) = getTokens(str);

            Assert.True(tokens.Count == 2);
        }

        [Fact]
        public void TestFiveEquals()
        {
            var str = "===== ".AsSpan();
            var (tokens, symbols) = getTokens(str);

            Assert.True(tokens.Count == 3);
        }
    }
}
