using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Net;

namespace bulk_attachment_delete
{
    /// <summary>
    /// Interface describing methods for sending requests to the Giving Api (GREAT)
    /// </summary>
    public interface IGivingApiService
    {
        /// <summary>
        /// Delete a file using the Giving Api
        /// </summary>
        /// <param name="id">123123-123123-123123-file.pdf</param>
        bool DeleteAttachmentById(string id);
    }

    public class GivingApiService : IGivingApiService
    {
        private readonly GivingSecrets _givingSecrets;

        public GivingApiService(IOptions<GivingSecrets> givingSecrets)
        {
            _givingSecrets = givingSecrets.Value;
        }

        public bool DeleteAttachmentById(string id)
        {            
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "Attachment id cannot be empty.");
            }

            var returnValue = false;

            var client = new RestClient(_givingSecrets.ApiUrl);

            var request = new RestRequest($"api/FileStorage/{id}", Method.DELETE);
            request.AddHeader("X-Auth-Token", _givingSecrets.ApiKey);

            client.ExecuteAsync(request, response => {
                returnValue = response.StatusCode == HttpStatusCode.OK;
            });

            return returnValue;
        }       
    }
}
