using System.Text;

namespace ReceiptPrinter
{
    public class FormattedText
    {
        private class Token
        {
            public bool IsLineBreak { get; set; }
            public string? Text { get; set; }

            public static Token CreateText(string text) => new Token(false, text);
            public static Token CreateLineBreak() => new Token(true, null);

            private Token(bool isLineBreak, string? text)
            {
                IsLineBreak = isLineBreak;

                if (text != null)
                    Text = text.Trim();
            }

            public override string ToString()
            {
                if (IsLineBreak)
                    return "\n";
                else
                    return Text ?? "";
            }
        }

        private string rawText;

        public FormattedText(string text)
        {
            rawText = text;
        }

        public string ApplyMaxWidth(int maxWidth)
        {
            StringBuilder result = new StringBuilder();
            List<Token> tokens = GetTokens(rawText);

            int currentLineWidth = 0;

            foreach (Token token in tokens)
            {
                if (token.IsLineBreak)
                {
                    result.AppendLine();
                    currentLineWidth = 0;
                }
                else
                {
                    if (currentLineWidth + token.Text!.Length > maxWidth)
                    {
                        result.AppendLine();
                        currentLineWidth = 0;

                        result.Append($"{token.Text!} ");
                        currentLineWidth += token.Text.Length + 1;
                    }
                    else if (currentLineWidth + token.Text!.Length + 1 > maxWidth) // if the last space would exceed the line width, don't add it, we will line break anyway
                    {
                        result.Append(token.Text);
                        currentLineWidth += token.Text.Length;
                    }
                    else
                    {
                        result.Append($"{token.Text!} ");
                        currentLineWidth += token.Text.Length + 1;
                    }
                }
            }

            result.Replace(" \r\n", "\r\n");
            result.Replace(" \n", "\n");
            result.Replace("\r\n ", "\r\n");
            result.Replace("\n ", "\n");

            return result.ToString();
        }

        private List<Token> GetTokens(string text)
        {
            List<Token> result = new List<Token>();

            string[] parts = text.Split(' ');

            foreach (string part in parts)
            {
                if (part.Contains("\n"))
                {
                    string[] subParts = part.Split('\n');

                    for (int i = 0; i < subParts.Length; i++)
                    {
                        result.Add(Token.CreateText(subParts[i]));

                        if (i < subParts.Length - 1)
                            result.Add(Token.CreateLineBreak());
                    }
                }
                else
                {
                    result.Add(Token.CreateText(part));
                }
            }

            return result;
        }
    }
}
