using CommandLine;

namespace RemoveTableAndBlobs
{
    class Options
    {
        [Option('t', "tables", Required = false, HelpText = "Tables to be deleted.")]
        public string Tables { get; set; }

        [Option('b', "blobs", Required = false, HelpText = "Blob containers to be deleted.")]
        public string Blobs { get; set; }

        [Option('v', "verbose", Default  = true, Required = false, HelpText = "Verbose logging on.")]
        public bool Verbose { get; set; }

    }
}
