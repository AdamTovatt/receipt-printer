using System.Diagnostics;
using System.Text;

namespace ReceiptPrinter.Printers
{
    public class Command
    {
        public string WorkingDirectory { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }

        public Command(string workingDirectory, string fileName, string arguments)
        {
            WorkingDirectory = workingDirectory;
            FileName = fileName;
            Arguments = arguments;
        }

        public Command(string command, string workingDirectory)
        {
            StringBuilder argument = new StringBuilder();
            string[] parts = command.Split();

            if (parts.Length > 1)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    argument.Append(string.Format("{0} ", parts[i]));
                }
            }

            WorkingDirectory = workingDirectory;
            FileName = parts[0];
            Arguments = argument.ToString().Trim();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", FileName, Arguments);
        }

        public async Task<string> RunAsync()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = WorkingDirectory;
            startInfo.FileName = FileName;
            startInfo.Arguments = Arguments;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;

            Process process = Process.Start(startInfo)!;
            await process.WaitForExitAsync();

            return await process.StandardOutput.ReadToEndAsync();
        }
    }
}
