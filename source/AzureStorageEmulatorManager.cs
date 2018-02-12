using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SenorDeveloper.Console;

namespace RemoveTableAndBlobs
{
    public static class AzureStorageEmulatorManager
    {

        private const string EmulatorPath = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";
        /// <summary>
        /// found on stackover flow https://stackoverflow.com/questions/7547567/how-to-start-azure-storage-emulator-from-within-a-program 
        /// by https://stackoverflow.com/users/607701/david-peden
        /// </summary>
        /// <returns></returns>
        public static bool IsProcessRunning()
        {
            bool status;

            using (Process process = Process.Start(StorageEmulatorProcessFactory.Create(ProcessCommand.Status)))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Unable to start process.");
                }

                status = GetStatus(process);
                process.WaitForExit();
            }

            return status;
        }

        public static void StartStorageEmulator()
        {
            if (!IsProcessRunning())
            {
                ExecuteProcess(ProcessCommand.Start);
            }
        }

        public static void StopStorageEmulator()
        {
            if (IsProcessRunning())
            {
                ExecuteProcess(ProcessCommand.Stop);
            }
        }

        private static void ExecuteProcess(ProcessCommand command)
        {
            string error;

            using (Process process = Process.Start(StorageEmulatorProcessFactory.Create(command)))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (process == null)
                {
                    throw new InvalidOperationException("Unable to start process.");
                }

                error = GetError(process);

                while (!GetStatus(process) || sw.ElapsedMilliseconds > 30000 )
                {
                    ".".Write();  
                    Thread.Sleep(500);
                }
                
                    
            }

            if (!String.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private static class StorageEmulatorProcessFactory
        {
            public static ProcessStartInfo Create(ProcessCommand command)
            {
                return new ProcessStartInfo
                {
                    FileName = EmulatorPath,
                    Arguments = command.ToString().ToLower(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
        }

        private enum ProcessCommand
        {
            Start,
            Stop,
            Status
        }

        private static bool GetStatus(Process process)
        {
            string output = process.StandardOutput.ReadToEnd();
            string isRunningLine = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).SingleOrDefault(line => line.StartsWith("IsRunning"));

            if (isRunningLine == null)
            {
                return false;
            }

            return Boolean.Parse(isRunningLine.Split(':').Select(part => part.Trim()).Last());
        }

        private static string GetError(Process process)
        {
            string output = process.StandardError.ReadToEnd();
            return output.Split(':').Select(part => part.Trim()).Last();
        }

        public static (int major, int minor, int build) GetRunningVersion()
        {
            try
            {
                FileVersionInfo myFileVersionInfo =
                    FileVersionInfo.GetVersionInfo(EmulatorPath);

                return (myFileVersionInfo.FileMajorPart , myFileVersionInfo.FileMinorPart , myFileVersionInfo.FileBuildPart);
            }
            catch (Exception e)
            {
                $"Could not get {EmulatorPath} FileVersionInfo".WriteLine(ConsoleColor.Red);
            }

            return (0, 0, 0);

        }
    }
}
