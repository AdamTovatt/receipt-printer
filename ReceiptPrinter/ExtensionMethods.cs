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

        // Helper method to split a long word into chunks
        private static IEnumerable<string> SplitWord(string word, int maxLength)
        {
            for (int i = 0; i < word.Length; i += maxLength)
            {
                yield return word.Substring(i, Math.Min(maxLength, word.Length - i));
            }
        }

        /// <summary>
        /// Repeats the given string until it reaches the specified length.
        /// </summary>
        /// <param name="loopingText">The string to loop.</param>
        /// <param name="length">The target length.</param>
        /// <returns>A new string of the specified length, filled by repeating the loopingText.</returns>
        public static string LoopToLength(this string loopingText, int length) // overly complicated function for looping text
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than zero.", nameof(length));
            }

            if (string.IsNullOrEmpty(loopingText))
            {
                return new string(' ', length); // Return a string of spaces if the input is empty
            }

            char[] resultArray = new char[length];
            int currentIndex = 0;

            // Fill the result array by looping through the input string
            while (currentIndex < length)
            {
                foreach (char c in loopingText)
                {
                    if (currentIndex < length)
                    {
                        resultArray[currentIndex] = c;
                        currentIndex++;
                    }
                    else
                    {
                        break; // Break if we've reached the desired length
                    }
                }
            }

            return new string(resultArray);
        }

        /// <summary>
        /// Centers the textToCenter inside the textCanvas by replacing characters in the middle with textToCenter,
        /// adding a white space before and after textToCenter.
        /// </summary>
        /// <param name="textToCenter">The text to be centered.</param>
        /// <param name="textCanvas">The canvas string where the text will be centered.</param>
        /// <returns>The textCanvas with textToCenter centered within it.</returns>
        public static string CenterIn(this string textToCenter, string textCanvas)
        {
            if (textCanvas == null) throw new ArgumentNullException(nameof(textCanvas));
            if (textToCenter == null) throw new ArgumentNullException(nameof(textToCenter));

            // Calculate total length needed for textToCenter with spaces
            string centeredText = $" {textToCenter} ";
            int canvasLength = textCanvas.Length;
            int textLength = centeredText.Length;

            if (textLength > canvasLength)
            {
                // If the centered text is longer than the canvas, return the centered text as it is
                return centeredText.Substring(0, canvasLength);
            }

            // Calculate starting index to replace in textCanvas
            int startIndex = (canvasLength - textLength) / 2;

            // Create a char array from the textCanvas
            char[] resultArray = textCanvas.ToCharArray();

            // Replace the middle part of the textCanvas with centeredText
            for (int i = 0; i < textLength; i++)
            {
                resultArray[startIndex + i] = centeredText[i];
            }

            // Return the modified string
            return new string(resultArray);
        }
    }
}
