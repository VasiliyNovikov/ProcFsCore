using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiagnosticsProcess = System.Diagnostics.Process;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class ProcessTests : ProcFsTestsBase
    {
        private const double CpuError = 0.01;

        private static void VerifyProcess(Process p, DiagnosticsProcess process)
        {
            Assert.AreEqual(process.Id, p.Pid);
            Assert.AreEqual(process.ProcessName, p.Name);
            Assert.IsNotNull(p.CommandLine);
            Assert.AreEqual(process.StartTime.ToUniversalTime(), p.StartTimeUtc);

            RetryOnAssert(() =>
            {
                process.Refresh();
                p.Refresh();
                Assert.AreEqual(process.VirtualMemorySize64, p.VirtualMemorySize);
            });

            RetryOnAssert(() =>
            {
                process.Refresh();
                p.Refresh();
                Assert.AreEqual(process.WorkingSet64, p.ResidentSetSize);
            });

            RetryOnAssert(() =>
            {
                process.Refresh();
                p.Refresh();
                Assert.AreEqual(process.UserProcessorTime.TotalSeconds, p.UserProcessorTime, CpuError);
            });

            RetryOnAssert(() =>
            {
                process.Refresh();
                p.Refresh();
                Assert.AreEqual(process.PrivilegedProcessorTime.TotalSeconds, p.KernelProcessorTime, CpuError);
            });
        }

        [TestMethod]
        public void Process_Current_Test()
        {
            var pi = Process.Current;
            var process = DiagnosticsProcess.GetCurrentProcess();
            VerifyProcess(pi, process);
        }

        [TestMethod]
        public void Process_ByPid_Test()
        {
            var process = DiagnosticsProcess.Start("sleep", "1000");
            try
            {
                var pi = new Process(process.Id);
                VerifyProcess(pi, process);
                Assert.AreEqual("sleep 1000", pi.CommandLine);
            }
            finally
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        [TestMethod]
        public void Process_All_Test()
        {
            Dictionary<int, Process> pis = null;
            Dictionary<int, DiagnosticsProcess> processes = null;
            RetryOnAssert(() =>
            {
                pis = ProcFs.Processes().ToDictionary(pi => pi.Pid);
                processes = DiagnosticsProcess.GetProcesses().ToDictionary(p => p.Id);
                Assert.AreEqual(processes.Count, pis.Count);
                CollectionAssert.AreEquivalent(pis.Keys, processes.Keys);
            });
            
            foreach (var pi in pis.Values)
            {
                var process = processes[pi.Pid];
                VerifyProcess(pi, process);
            }
        }
        
        [TestMethod]
        public void Process_Current_OpenFiles_Test()
        {
            var pi = Process.Current;
            var fileName = $"/proc/{pi.Pid}/stat";
            using (File.OpenRead(fileName))
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 12345));
                var links = pi.OpenFiles.ToList();
                Assert.IsTrue(links.Any(l => l.Type == LinkType.File && l.Path == fileName));
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Anon));
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Socket));
                var expectedINode = ProcFs.Net.Services.Udp(NetAddressVersion.IPv4)
                                                       .Single(s => s.LocalEndPoint.Address.IsEmpty && s.LocalEndPoint.Port == 12345 && s.State == NetServiceState.Closed)
                                                       .INode;
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Socket && l.INode == expectedINode));
            }
        }
    }
}