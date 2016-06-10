using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MachineInspectionLibrary;

namespace MachineInspectie.Dal
{
    public class ControlQuestions
    {
        private readonly Uri _apiControlQuestion = new Uri("http://vangansewinkel.vanlaer-it.be/api/controlquestion?category=");
        //private HttpClient _client;
        //private string _response; 

        //public async Task<string> GetMatisByLocation(string category, string language)
        //{
        //    try
        //    {
        //        Uri apiControlQuestions = new Uri(_api.ToString() + category + "&locale=" + language);
        //        using (_client = new HttpClient())
        //        {
        //            _response = await _client.GetStringAsync(apiControlQuestions);
        //        }
        //        return _response;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public async Task<List<ControlQuestion>> ControlQuestionList(string category, string language)
        //{
        //    List<ControlQuestion> controlQuestions =
        //        JsonConvert.DeserializeObject<ControlQuestionWrapper>(await GetMatisByLocation(category, language)).data;
        //    return controlQuestions;
        //}

        public async Task<string> GetControlQuestion(string category, string language)
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
