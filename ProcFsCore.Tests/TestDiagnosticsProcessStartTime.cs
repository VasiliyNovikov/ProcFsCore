using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiagnosticsProcess = System.Diagnostics.Process;

namespace ProcFsCore.Tests;

[TestClass]
public class TestDiagnosticsProcessStartTime
{
    private const string GetStartTimeByPidProgram = "using System; Console.WriteLine(System.Diagnostics.Process.GetProcessById(int.Parse(args[0])).StartTime.Ticks);"; 

    [TestMethod]
    public void ProcessStartTime_Deterministic_Across_Instances()
    {
        using var process = DiagnosticsProcess.Start("sleep", "1000");
        
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        try
        {
            Cmd("dotnet", "new console", tempDir);
            
            File.WriteAllText(Path.Combine(tempDir, "Program.cs"), GetStartTimeByPidProgram);
            
            Cmd("dotnet", "build", tempDir);
            
            for (var i = 0; i < 1000; ++i)
            {
                var startTimeTicks = long.Parse(Cmd("dotnet", $"run --no-build -- {process.Id}", tempDir));
                Assert.AreEqual(process.StartTime.Ticks, DiagnosticsProcess.GetProcessById(process.Id).StartTime.Ticks);
                Assert.AreEqual(process.StartTime.Ticks, startTimeTicks);
            }
        }
        finally
        {
            process.Kill();
            Directory.Delete(tempDir, true);
        }
    }

    private static string Cmd(string cmd, string args, string? workingDir = null)
    {
        var process = new DiagnosticsProcess
        {
            StartInfo =
            {
                FileName = cmd,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDir
            }
        };

        var output = new StringBuilder();
        process.OutputDataReceived += (_, e) => HandleDataReceived(output, e);

        var error = new StringBuilder();
        process.ErrorDataReceived += (_, e) => HandleDataReceived(error, e);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        process.WaitForExit();

        return process.ExitCode == 0
            ? output.ToString()
            : throw new Exception($"Process exited with code {process.ExitCode}.\n{error}");

        static void HandleDataReceived(StringBuilder target, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            if (target.Length > 0)
                target.Append('\n');
            target.Append(e.Data);
        }
    }
}