using System;
using System.Linq;
using Xunit;

using OPC.Core;

namespace OPC.Tests
{
    public class LexTests
    {
        [Fact]
        public void WhenKeyword_ShouldReturnToken()
        {
            var result = LexicalModule.isKeyword("principal");

            Assert.True(result.IsKeyword);
        }

        [Fact]
        public void WhenKeywords_ShouldReturnToken()
        {
            var keywords = new string[] { "si", "principal", "entero", "real", "logico", "mientras", "regresa", "verdadero", "falso" };
            var failed = keywords.Select(word => LexicalModule.isKeyword(word))
                    .Select(result => result.IsKeyword)
                    .Any(result => result != true);

            Assert.False(failed);
        }

        [Fact]
        private void WhenNoKeyword_ShouldReturnNone()
        {
            var result = LexicalModule.isKeyword("aaaaa");

            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenSeparator_ShouldReturnToken()
        {
            var result = LexicalModule.isPunctuation("(");
            Assert.True(result.IsPunctuation);
        }
        
        [Fact]
        public void WhenNotSeparator_ShouldReturnNone()
        {
            var result = LexicalModule.isPunctuation("a");
            
            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenOperator_ShouldReturnOperator()
        {
            var result = LexicalModule.isOperator("+", false);

            Assert.True(result.IsOperator);
        }

        [Fact]
        public void WhenEqualsAndFalse_ShouldReturnNone()
        {
            var result = LexicalModule.isOperator("=", false);

            Assert.True(result.IsNone);
        }

        [Fact]
        public void WhenEqualsAndTrue_ShouldReturnOperator()
        {
            var result = LexicalModule.isOperator("==", true);

            Assert.True(result.IsOperator);
        }
    }
}
