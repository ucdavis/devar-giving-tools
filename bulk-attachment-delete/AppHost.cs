using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bulk_attachment_delete
{
    public interface IAppHost
    {
        void Run(Options opts);
    }

    public class AppHost : IAppHost
    {
        private readonly IGivingApiService _givingApiService;
        private Options _opts;

        public AppHost(IGivingApiService givingApiService)
        {
            _givingApiService = givingApiService;
        }

        public void Run(Options opts)
        {
            _opts = opts;
            
            WriteStartMessage();

            if (String.IsNullOrWhiteSpace(opts.FilePath))
            {
                throw new ArgumentException("Please provide a path to the input file.", "input");
            }

            if (!File.Exists(opts.FilePath))
            {
                throw new ArgumentException("Input file does not exist.", "input");
            }
            
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)) 
            {
                try
                {
                    var attachments = GetAttachments(opts.FilePath, opts.HasHeaderRecord);

                    foreach (Attachment attachment in attachments)
                    {
                        if(opts.Verbose)
                        {
                            Console.WriteLine($"Deleting attachment with id '{attachment.Id}'");
                        }
                        
                        var result = _givingApiService.DeleteAttachmentById(attachment.Id);

                        if (!result)
                        {
                            Console.WriteLine("FAILED.");
                            //WriteEndMessage();
                            //Environment.Exit(1);
                        }
                        else if (opts.Verbose)
                        {
                            Console.WriteLine("SUCCESS!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException);
                    Environment.Exit(1);
                }
            }

            WriteEndMessage();

            Environment.Exit(0);
        }

        private void WriteStartMessage()
        {
            Console.Write("Press <Esc> to exit... ");

            if (_opts.Verbose)
            {
                DateTime dat = DateTime.Now;
                Console.WriteLine("Start time: {0:d} at {0:t}", dat);                
            }
            return;
        }

        private void WriteEndMessage()
        {
            if (_opts.Verbose)
            {
                DateTime dat = DateTime.Now;
                Console.WriteLine("End time: {0:d} at {0:t}", dat);
            }
            return;
        }

        private List<Attachment> GetAttachments(string filePath, bool hasHeaderRecord)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = hasHeaderRecord;

            var attachments = csv.GetRecords<Attachment>().ToList();

            return attachments;
        }
    }
}
