using System;
using Xunit;
using Microsoft.FSharp.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using OPC.Cminus.Core;

namespace OPC.Cminus.Tests
{
    public class LexTests
    {
        [Fact]
        public void WhenVarDec_TokenCount3()
        {
            var fList = ListModule.OfSeq(new List<string>{ "int", "x", ";" });
            var tokens = LexicalModule.processString(fList);
            Assert.Equal(3, tokens.Length);
        }

        [Fact]
        public void ProcessFullProgram()
        {
            var str = @"int fact( int x )
                        { if (x == 1)
                            return x * fact(x-1);
                          else
                            return 1;
                            }
                        void main(void) {
                            int x;
                            x = read();
                            if (x > 0) write(fact(x));
                        }";

            var words = GetWords(str);
            var fList = ListModule.OfSeq(words);
            var tokens = LexicalModule.processString(fList);
            Assert.Equal(fList.Length, tokens.Length);
        }


        private readonly char[] deletableSeparator = { ' ', '\t', '\n', '\r' };
        private readonly char[] keepChar = { '(', ')', '{', '}', '-', '+', '<', '>' };
        StringBuilder builder = new StringBuilder();
        private IEnumerable<string> GetWords(string str)
        {
            builder.Clear();
            var words = new List<string>();
            foreach (var ch in str)
            {
                if (deletableSeparator.Contains(ch))
                {
                    if (builder.Length == 0)
                        continue;
                    words.Add(builder.ToString());
                    builder.Clear();
                }
                else if (keepChar.Contains(ch))
                {
                    if (builder.Length != 0)
                    {
                        words.Add(builder.ToString());
                        builder.Clear();
                    }
                    words.Add(ch.ToString());
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return words;
        }
    }
}
