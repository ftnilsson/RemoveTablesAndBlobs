using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SenorDeveloper.Console;

namespace RemoveTableAndBlobs
{
    /// <summary>
    /// requires storage emulator >= 5.3
    /// </summary>
    public class Program
    {
        private static Stopwatch _sw;
        private static (int minMajor, int minMinor) minVersion = (5, 3);
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var isValid = true;
            Console.WindowWidth = 120;
            StorageConnectionString = config["StorageConnectionString"];

            SenorDeveloperAsciiArt.GetAsciiArtRainbow();

            var version = AzureStorageEmulatorManager.GetRunningVersion();
            if (!IsValidVersion(version.major, version.minor, version.build))
            {
                "***********************************************************************************************************".WriteLine(ConsoleColor.Yellow);
                $"*********************** you need to run azure storage emulator with version >= {minVersion.minMajor}.{minVersion.minMinor} ************************".WriteLine(ConsoleColor.Yellow);
                "***********************      as it requires the version 2017-04-17 of the API      ************************".WriteLine(ConsoleColor.Yellow);
                "*********************** https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409 ************************".WriteLine(ConsoleColor.Yellow); 
                "***********************************************************************************************************".WriteLine(ConsoleColor.Yellow);

                isValid = false;
            }

            if (isValid && !AzureStorageEmulatorManager.IsProcessRunning())
            {
                "*** Starting Azure Storage Emulator ***".WriteLine(ConsoleColor.Yellow);
                try
                {
                    AzureStorageEmulatorManager.StartStorageEmulator();
                }
                catch
                {
                    "Could not start emulator - aborting".WriteLine(ConsoleColor.Red);
                    "try starting the Storage Emulator manually".WriteLine(ConsoleColor.Yellow);

                    isValid = false;
                }
                
            }

            if (isValid)
            {
                CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts).Wait())
                    .WithNotParsed<Options>((errs) => HandleParseError(errs));

                "**** Finished!".WriteLine(ConsoleColor.Yellow);
                "*******************************".WriteLine(ConsoleColor.Yellow);
            }
            else
            {
                "***** requirements not met *****".WriteLine(ConsoleColor.Red);
            }
            "please press enter to exit.".WriteLine(ConsoleColor.Yellow);
            Console.ReadLine();
        }

        public static bool IsValidVersion(int major, int minor, int build)
        {
            return major >= minVersion.minMajor && minor >= minVersion.minMinor;
        }
        
        public static string StorageConnectionString { get; set; }

        private static async Task RunOptionsAndReturnExitCode(Options opts)
        {
            if (opts.Tables.Any())
            {
                var tables = opts.Tables.Contains(",") ? opts.Tables.Split(",") : opts.Tables.Split(" ");
                await DeleteTables(tables, opts.Verbose);
            }
            if (opts.Blobs.Any())
            {
                var blobs = opts.Tables.Contains(",") ? opts.Blobs.Split(",") : opts.Blobs.Split(" ");
                await DeleteBlobs(blobs, opts.Verbose);
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
            {
                $"error : {error}".WriteLine(ConsoleColor.Red);
            }
        }

        public static async Task DeleteTables(IEnumerable<string> tables, bool verbose)
        {
            _sw = new Stopwatch();
            _sw.Start();
            if (verbose)
                $"******** Deleteing Tables ********".WriteLine(ConsoleColor.Yellow);

            if (tables != null)
            {
                var enumerable = tables as IList<string> ?? tables.ToList();
                foreach (var table in enumerable.ToList())
                {
                    try
                    {
                        await DeleteTable(table, verbose);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        continue;
                    }
                }
                _sw.Stop();
                if (verbose)
                {
                    $"Deleted tables: {string.Join(",", enumerable)} in {_sw.ElapsedMilliseconds} ms".WriteLine(ConsoleColor.DarkYellow); ;
                    $"*********************************".WriteLine(ConsoleColor.Yellow); ;
                }
            }
        }

        public static async Task DeleteBlobs(IEnumerable<string> blobs, bool verbose)
        {
            _sw = new Stopwatch();
            _sw.Start();
            if (verbose)
                $"******** Deleteing Blobs ********".WriteLine(ConsoleColor.Yellow); ;

            foreach (var blob in blobs)
            {
                await DeleteBlob(blob, verbose);
            }

            _sw.Stop();
            if (verbose)
            {
                $"Deleted blobs: {string.Join(",", blobs)} in {_sw.ElapsedMilliseconds} ms".WriteLine(ConsoleColor.DarkYellow);
                $"*******************************".WriteLine(ConsoleColor.Yellow); 
            }
        }

        private static async Task DeleteBlob(string blobName, bool verbose)
        {
            if (verbose)
                $".. deleting {blobName}".WriteLine(ConsoleColor.DarkYellow);

            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(blobName);

            var deleted = await container.DeleteIfExistsAsync();

            if (verbose)
                if(deleted)
                    $".. {blobName} deleted".WriteLine(ConsoleColor.DarkYellow);
            else
                    $".. {blobName} not deleted".WriteLine(ConsoleColor.DarkRed);
        }

        private static async Task DeleteTable(string tableName, bool verbose)
        {
            if (verbose)
                $".. deleting {tableName}".WriteLine(ConsoleColor.DarkYellow);

            try
            {
                var client = CreateClient();
                var table = client.GetTableReference(tableName);

                var deleted = await table.DeleteIfExistsAsync();

                if (verbose)
                    if (deleted)
                        $".. {tableName} deleted ".WriteLine(ConsoleColor.Blue);
                    else                            
                        $".. {tableName} not deleted".WriteLine(ConsoleColor.DarkRed);
            }
            catch (Exception e)
            {
                $".. {tableName} not deleted - {e.Message}".WriteLine(ConsoleColor.Red);
            }
        }

        private static CloudTableClient CreateClient()
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            return storageAccount.CreateCloudTableClient();
        }
    }
}
