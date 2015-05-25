using System.Diagnostics;

namespace DetectIps
{
    public class ExecuteAsAdministrator
    {
        public ExecuteAsAdministrator(string command)
        {
            Command = command;
        }

        public string Command { get; set; }

        public void Run()
        {
            string subCommandFinal = @"cmd /K \""" + Command.Replace(@"\", @"\\") + @"\""";

            //Run the runas command directly
            var procStartInfo = new ProcessStartInfo("runas.exe") {UseShellExecute = true, CreateNoWindow = true};

            //Create our arguments
            string finalArgs = @"/env /user:Administrator """ + subCommandFinal + @"""";
            procStartInfo.Arguments = finalArgs;

            //command contains the command to be executed in cmd
            using (var proc = new Process())
            {
                proc.StartInfo = procStartInfo;
                proc.Start();
            }
        }
    }
}