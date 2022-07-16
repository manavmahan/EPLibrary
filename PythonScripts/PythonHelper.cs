using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects.PythonScripts
{
    public static class PythonHelper
    {
        public static string ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            //Console.WriteLine(command);

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            //Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            //Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
            return output;
        }

        public static void Plot(params XYZList[] loops)
        {
            var loopStr = string.Join(":", loops.Select(p => p.ToString(true)));
            string python = "python3";
            string arg = $"{nameof(PythonScripts)}/{nameof(Plot).ToLower()}.py" + " " + loopStr;
            ExecuteCommand(string.Join(" ", new[] { python, arg }));
        }
        
    }
}
