using System;
using System.Linq;

namespace ProcFsCore.Experiments
{
    internal static class Program
    {
        private static void Main()
        {
            foreach (var proc in ProcFs.Default.Processes())
                Console.WriteLine($"{proc.Pid} {proc.Name} {proc.CommandLine}");
            
            Console.WriteLine();

            foreach (var file in ProcFs.Default.Process(1).OpenFiles)
                Console.WriteLine(file);
            
            Console.WriteLine();
            
            foreach (var svc in ProcFs.Default.Net.Services.Unix().Where(svc => svc.State == NetServiceState.Established))
                Console.WriteLine(svc);
        }
    }
}