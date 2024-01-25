using System;
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

            RetryOnAssert(() =>
            {
                process.Refresh();
                p.Refresh();
                if (process.ProcessName != p.Name)
                    Assert.IsTrue(p.CommandLine.Contains(process.ProcessName), $"Process name mismatch: {p.Name} ({p.CommandLine}) - {process.ProcessName}");
            });

            Assert.IsNotNull(p.CommandLine);
            var expectedStartTime = process.StartTime.ToUniversalTime();
            var actualStartTime = p.StartTimeUtc;
            var expectedDelta = TimeSpan.FromSeconds(0.01);
            Assert.AreEqual(expectedStartTime.Ticks, actualStartTime.Ticks, expectedDelta.Ticks, $"Expected a difference no greater than <{expectedDelta.TotalMilliseconds}> ms between expected value <{expectedStartTime:O}> and actual value {actualStartTime:O}");

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
            var pi = ProcFs.Default.CurrentProcess;
            var process = DiagnosticsProcess.GetCurrentProcess();
            VerifyProcess(pi, process);
        }

        [TestMethod]
        public void Process_ByPid_Test()
        {
            var process = DiagnosticsProcess.Start("sleep", "1000");
            Assert.IsNotNull(process);
            try
            {
                var pi = ProcFs.Default.Process(process.Id);
                VerifyProcess(pi, process);
                Assert.AreEqual("sleep\01000", pi.CommandLine);
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
            Dictionary<int, Process>? pis = null;
            Dictionary<int, DiagnosticsProcess>? processes = null;
            RetryOnAssert(() =>
            {
                pis = ProcFs.Default.Processes().ToDictionary(pi => pi.Pid);
                processes = DiagnosticsProcess.GetProcesses().ToDictionary(p => p.Id);
                Assert.AreEqual(processes.Count, pis.Count);
                CollectionAssert.AreEquivalent(pis.Keys, processes.Keys);
            });
            
            foreach (var pi in pis!.Values)
            {
                var process = processes![pi.Pid];
                VerifyProcess(pi, process);
            }
        }
        
        [TestMethod]
        public void Process_Current_OpenFiles_Test()
        {
            var pi = ProcFs.Default.CurrentProcess;
            var fileName = $"/proc/{pi.Pid}/stat";
            using (File.OpenRead(fileName))
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 12345));
                var links = pi.OpenFiles.ToList();
                Assert.IsTrue(links.Any(l => l.Type == LinkType.File && l.Path == fileName));
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Anon));
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Socket));
                var expectedINode = ProcFs.Default.Net.Services.Udp(NetAddressVersion.IPv4)
                                                                  .Single(s => s.LocalEndPoint.Address.IsEmpty && s.LocalEndPoint.Port == 12345 && s.State == NetServiceState.Closed)
                                                                  .INode;
                Assert.IsTrue(links.Any(l => l.Type == LinkType.Socket && l.INode == expectedINode));
            }
        }

        [TestMethod]
        public void Process_Current_IO_Test()
        {
            const int mb = 1048576;
            const int fileSizeMb = 128;
            const int ioErrorDelta = mb / 16;

            var process = ProcFs.Default.CurrentProcess;
            var initialIoStats = process.IO;

            var tmpFile = Path.GetTempFileName();
            try
            {
                var rnd = new Random();
                Span<byte> buffer = stackalloc byte[mb];
                using (var file = File.OpenWrite(tmpFile))
                {
                    for (var i = 0; i < fileSizeMb; ++i)
                    {
                        rnd.NextBytes(buffer);
                        file.Write(buffer);
                    }
                }

                using (var file = File.OpenRead(tmpFile))
                {
                    while (file.Read(buffer) > 0)
                    {
                    }
                }
            }
            finally
            {
                File.Delete(tmpFile);                
            }
            
            Console.WriteLine("Test");
            
            var ioStats = process.IO;
            // Assert.IsTrue(ioStats.Read.Bytes > 0, "Read.Bytes > 0"); // Not sure how to initiate a read directly from disk - it seems always comes from cache
            Assert.IsTrue(ioStats.Read.Characters > 0, "Read.Characters > 0");
            Assert.IsTrue(ioStats.Read.Characters >= ioStats.Read.Bytes, "Read.Characters >= Read.Bytes");
            Assert.IsTrue(ioStats.Read.SysCalls > 0, "Read.SysCalls > 0");
            Assert.IsTrue(ioStats.Read.Characters > initialIoStats.Read.Characters, "Read.Characters > initial.Read.Characters");
            Assert.IsTrue(ioStats.Read.SysCalls > initialIoStats.Read.SysCalls, "Read.SysCalls > initial.Read.SysCalls");
            Assert.AreEqual(mb * fileSizeMb, ioStats.Read.Characters - initialIoStats.Read.Characters, ioErrorDelta);
            
            
            Assert.IsTrue(ioStats.Write.Bytes > 0, "Write.Bytes > 0");
            Assert.IsTrue(ioStats.Write.Characters > 0, "Write.Characters > 0");
            Assert.IsTrue(ioStats.Write.Characters > ioStats.Write.Bytes, "Write.Characters > Write.Bytes");
            Assert.IsTrue(ioStats.Write.SysCalls > 0, "Write.SysCalls > 0");
            Assert.IsTrue(ioStats.Write.Characters > initialIoStats.Write.Characters, "Write.Characters > initial.Write.Characters");
            Assert.IsTrue(ioStats.Write.Bytes > initialIoStats.Write.Bytes, "Write.Bytes > initial.Write.Bytes");
            Assert.IsTrue(ioStats.Write.SysCalls > initialIoStats.Write.SysCalls, "Write.SysCalls > initial.Write.SysCalls");
            Assert.AreEqual(mb * fileSizeMb, ioStats.Write.Characters - initialIoStats.Write.Characters, ioErrorDelta);
            Assert.AreEqual(mb * fileSizeMb, ioStats.Write.Bytes - initialIoStats.Write.Bytes, ioErrorDelta);
        }
    }
}