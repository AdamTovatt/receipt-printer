namespace ReceiptPrinter
{
    public class RuntimeArguments
    {
        private Dictionary<string, string> arguments;

        public RuntimeArguments(string[] arguments)
        {
            this.arguments = CreateDictionary(arguments);
        }

        public void Require(params string[] requiredArguments)
        {
            List<string> missingArguments = new List<string>();

            foreach (string requiredArgument in requiredArguments)
            {
                if (!arguments.ContainsKey(requiredArgument))
                    missingArguments.Add(requiredArgument);
            }

            if (missingArguments.Count > 0)
                throw new ArgumentException($"Missing required command line arguments: {string.Join(", ", missingArguments)}.\nArguments should be specified with two dashes in front followed by a space and then the value. Like this: --{missingArguments.First()} value-for-{missingArguments.First()}-here");
        }

        /// <summary>
        /// Creates a dictionary from the given arguments array. The format for the run time arguments string should be "--key-1 value-1 --key-2 value-2"
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Dictionary<string, string> CreateDictionary(string[] args)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Iterate through the arguments array
            for (int i = 0; i < args.Length; i++)
            {
                // Check if the argument starts with "--"
                if (args[i].StartsWith("--"))
                {
                    // Extract the key by removing the "--" prefix
                    string key = args[i].Substring(2);

                    // Ensure that the next argument is the value
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        // Add the key-value pair to the dictionary
                        dictionary[key] = args[i + 1];
                        i++; // Move to the next argument (skip the value part)
                    }
                    else
                    {
                        // If the value is missing, throw an exception or handle accordingly
                        throw new ArgumentException($"Expected a value for parameter '{args[i]}'");
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Get an argument from the run-time arguments, convert it to the specified type, or return null if it does not exist or cannot be converted.
        /// </summary>
        /// <typeparam name="T">The type to which the argument value should be converted.</typeparam>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The converted value of the argument or null if it doesn't exist or conversion fails.</returns>
        public T? Get<T>(string argumentName)
        {
            if (arguments.TryGetValue(argumentName, out string? value) && value is string stringValue)
            {
                try
                {
                    return (T)Convert.ChangeType(stringValue, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"The argument '{argumentName}' was found, but could not be converted to type '{typeof(T).Name}'. Expected format: {typeof(T).Name}.", ex);
                }
            }
            return default;
        }
    }
}
