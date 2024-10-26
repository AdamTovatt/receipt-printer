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
    }
}
