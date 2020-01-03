using CommandLine;

namespace bulk_attachment_delete
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Path to CSV input file containing attachment file names.")]
        public string FilePath { get; set; }

        [Option('h', "header", Default = true, HelpText = "CSV input file has a header row.")]
        public bool HasHeaderRecord { get; set; }

        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
}
