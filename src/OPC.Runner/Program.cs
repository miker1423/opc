using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.FSharp.Collections;

using static OPC.Core.LexicalModule;
using static OPC.Core.SyntaxModule;
using static OPC.Core.Types;

namespace OPC.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var file = await File.ReadAllTextAsync(@".\Archivo_Fuente.txt");
            var newText = RemoveRChar(file);

            var tokens = getTokens(newText);

            var list = ListModule.OfSeq(tokens.Item1);

            var syntaxResult = SyntaxAnalyze(list);

            Console.WriteLine(syntaxResult);

            Console.WriteLine("TOKENS");
            foreach (var token in tokens.Item1)
            {
                Console.WriteLine(token);
            }

            Console.ReadLine();
        }

        static string RemoveRChar(ReadOnlySpan<char> chars)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (ch.Equals('\r'))
                {
                    i++;
                    ch = chars[i];
                    var isNewLine = ch.Equals('\n');
                    if (isNewLine)
                        builder.Append(ch);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }
    }
}
