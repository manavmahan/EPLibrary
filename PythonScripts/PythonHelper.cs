//using IronPython.Hosting;
//using IronPython.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IDFObjects.PythonScripts
{
    public static class PythonHelper
    {
        public const string ScriptFolder = "PythonScripts";

        public static string Program = string.Empty;
        public static void SetProgram(string program = "")
        {
            if (!string.IsNullOrEmpty(Program))
                return;

            Program = program;
            if (string.IsNullOrEmpty(Program))
                Program = "/usr/bin/python3";

            CheckPython();
        }

        private static void CheckPython()
        {
            if(ExecuteCommand("CheckLibraries", string.Empty).Item3 != 0){
                throw new Exception("Cannot locate python with shapely library!");
            }
            return;
        }

        public static (string, string, int) ExecuteCommand(string program, string args)
        {
            SetProgram();
            //int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            //Console.WriteLine($"/c {Python3} " + $"{ScriptFolder}/{program}.py {args}");

            processInfo = new ProcessStartInfo(Program, $"{ScriptFolder}/{program}.py {args}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            process = Process.Start(processInfo);


            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            var output = string.Empty;
            var error = string.Empty;
            process.BeginOutputReadLine();
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            { output += e.Data; });

            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            { error += e.Data; });

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Python error.\n{error}");

            var exitCode = process.ExitCode;

            //Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            //Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            
            process.Close();
            return (output, error, exitCode); 
        }

        //public static string ExecuteCommandIPython(string program, string args)
        //{
        //    var engine = Python.CreateEngine();
        //    var scope = engine.GetSysModule(); 
        //    scope.SetVariable("args", args);

        //    var output = engine.ExecuteFile($"{ScriptFolder}/{program}.py");
        //    //engine.Runtime.IO.RedirectToConsole();
        //    var o2 = ExecuteCommand(program, args);

        //    return string.Empty;
        //}

    }
}
