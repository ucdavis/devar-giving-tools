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

        public AppHost(IGivingApiService givingApiService)
        {
            _givingApiService = givingApiService;
        }

        public void Run(Options opts)
        {
            WriteStartMessage(opts);

            if (String.IsNullOrWhiteSpace(opts.FilePath))
            {
                throw new ArgumentException("Please provide a path to the input file.", "input");
            }

            if (!File.Exists(opts.FilePath))
            {
                throw new ArgumentException("Input file does not exist.", "input");
            }

            while (Console.ReadKey().Key != ConsoleKey.Enter) 
            {
                try
                {
                    var attachments = GetAttachments(opts);

                    foreach (Attachment attachment in attachments)
                    {
                        if(opts.Verbose)
                        {
                            Console.WriteLine($"Deleting attachment with id '{attachment.Id}'");
                        }
                        
                        var result = _givingApiService.DeleteAttachmentById(attachment.Id);

                        if (!result)
                        {
                            Console.WriteLine("FAILED. Exiting program...");
                            WriteEndMessage(opts);
                            Environment.Exit(1);
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

            WriteEndMessage(opts);

            Environment.Exit(0);
        }

        private void WriteStartMessage(Options opts)
        {
            Console.Write("Press <Enter> to exit... ");

            if (opts.Verbose)
            {
                DateTime dat = DateTime.Now;
                Console.WriteLine("Start time: {0:d} at {0:t}", dat);                
            }
            return;
        }

        private void WriteEndMessage(Options opts)
        {
            if (opts.Verbose)
            {
                DateTime dat = DateTime.Now;
                Console.WriteLine("End time: {0:d} at {0:t}", dat);
            }
            return;
        }

        private List<Attachment> GetAttachments(Options opts)
        {
            using var reader = new StreamReader(opts.FilePath);
            using var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = opts.HasHeaderRecord;

            return csv.GetRecords<Attachment>().ToList();
        }
    }
}
