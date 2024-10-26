namespace ReceiptPrinter
{
    public static class ExtensionMethods
    {
        public static string? SafeGetElementAtIndex(this string[] args, int index) =>
            args.Length > index ? args[index] : null;

        public static void EnsureSuccessStatusCodeWithInfo(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string? content = null;

                try
                {
                    content = response.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    content = "(Missing error details because the response content could not be read)";
                }

                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).\n{content}");
            }
        }

        public static string SplitToLines(this string stringToSplit, int maximumLineLength)
        {
            if (string.IsNullOrEmpty(stringToSplit) || maximumLineLength <= 0)
            {
                return stringToSplit; // Return the original string if it's null or empty
            }

            List<string> lines = new List<string>();
            string[] words = stringToSplit.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string line = "";

            foreach (string word in words)
            {
                // Check if the current word is longer than the maximum line length
                if (word.Length > maximumLineLength)
                {
                    // If so, break the word into smaller chunks
                    foreach (string chunk in SplitWord(word, maximumLineLength))
                    {
                        if (line.Length + chunk.Length + 1 > maximumLineLength)
                        {
                            if (!string.IsNullOrEmpty(line)) // Avoid yielding empty lines
                            {
                                lines.Add(line); // Add the current line to the list
                            }
                            line = chunk; // Start a new line with the chunk
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                line += " "; // Add a space if line is not empty
                            }
                            line += chunk; // Add the chunk to the current line
                        }
                    }
                }
                else
                {
                    if (line.Length + word.Length + 1 > maximumLineLength)
                    {
                        lines.Add(line); // Add the current line to the list
                        line = word; // Start a new line with the current word
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            line += " "; // Add a space if line is not empty
                        }
                        line += word; // Add the word to the current line
                    }
                }
            }

            // Add the last line if it's not empty
            if (!string.IsNullOrEmpty(line))
            {
                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines); // Join the lines into a single string
        }

        // Helper method to split a long word into chunks
        private static IEnumerable<string> SplitWord(string word, int maxLength)
        {
            for (int i = 0; i < word.Length; i += maxLength)
            {
                yield return word.Substring(i, Math.Min(maxLength, word.Length - i));
            }
        }
    }
}
