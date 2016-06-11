using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MachineInspectie.Services
{
    public class CallApi
    {
        //Private Uri's
        private readonly Uri _apiLocatie = new Uri("http://vangansewinkel.vanlaer-it.be/api/location");
        private readonly Uri _apiMatis = new Uri("http://vangansewinkel.vanlaer-it.be/api/matis?location=");
        private readonly Uri _apiControlQuestion = new Uri("http://vangansewinkel.vanlaer-it.be/api/controlquestion?category=");

        /// <summary>
        /// Sends a call to the location API
        /// </summary>
        /// <returns>Response message as string</returns>
        public async Task<string> Location()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync(_apiLocatie);
            client.Dispose();
            var message = httpResponse.Content.ReadAsStringAsync().Result;
            return message;
        }

        /// <summary>
        /// Sends a call to the matis API
        /// </summary>
        /// <param name="location">User location</param>
        /// <returns>Response message as string</returns>
        public async Task<string> Matis(string location)
        {
            Uri apiMatis = new Uri(_apiMatis + location);
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync(apiMatis);
            client.Dispose();
            var message = httpResponse.Content.ReadAsStringAsync().Result;
            return message;
        }

        /// <summary>
        /// Sends a call to the controlquestion API
        /// </summary>
        /// <param name="category">Selected matis category</param>
        /// <param name="language">Selected application lanuage</param>
        /// <returns></returns>
        public async Task<string> ControlQuestion(string category, string language)
        {
            Uri apiControlQuestion = new Uri(_apiControlQuestion.ToString() + category + "&locale=" + language);
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync(apiControlQuestion);
            client.Dispose();
            var message = httpResponse.Content.ReadAsStringAsync().Result;
            return message;
        }
    }
}
