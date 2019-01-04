using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class LightFileStreamTests
    {
        private readonly List<string> _tempFiles = new List<string>();
        private readonly Random _rnd = new Random();

        private string GetTempFile()
        {
            var tempFile = Path.GetTempFileName();
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        [TestCleanup]
        public void CleanUp()
        {
            foreach (var tempFile in _tempFiles)
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
        }
        
        [TestMethod]
        public void LightFileStream_Read_Test()
        {
            var tempFile = GetTempFile();
            var expectedData = new byte[65536];
            _rnd.NextBytes(expectedData);
            File.WriteAllBytes(tempFile, expectedData);
            using (var file = LightFileStream.OpenRead(tempFile))
            {
                var actualData = new byte[expectedData.Length * 2];
                var totalReadBytes = 0;
                int readBytes;
                while ((readBytes = file.Read(actualData.AsSpan(totalReadBytes, 1024))) != 0)
                    totalReadBytes += readBytes;
                Assert.IsTrue(actualData.AsSpan(0, totalReadBytes).SequenceEqual(expectedData));                
            }
        }
        
        [TestMethod]
        public void LightFileStream_Write_Test()
        {
            var tempFile = GetTempFile();
            var expectedData = new byte[65536];
            _rnd.NextBytes(expectedData);
            using (var file = LightFileStream.OpenWrite(tempFile))
            {
                var writeBuffer = expectedData.AsSpan();
                while (!writeBuffer.IsEmpty)
                {
                    file.Write(writeBuffer.Slice(0, 1024));
                    writeBuffer = writeBuffer.Slice(1024);
                }
            }

            var actualData = File.ReadAllBytes(tempFile);
            Assert.IsTrue(actualData.AsSpan().SequenceEqual(expectedData)); 
        }
    }
}