using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using static OPC.Core.LexicalModule;

namespace OPC.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var file = await File.ReadAllTextAsync(@".\Archivo_Fuente.txt");
            var newText = RemoveRChar(file);

            var (tokens, symbols) = getTokens(newText);

            Console.WriteLine("TOKENS");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            Console.WriteLine("\nTABLE");
            foreach (var symbol in symbols)
            {
                Console.WriteLine($"{symbol.Type} - {symbol.Id}");
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
