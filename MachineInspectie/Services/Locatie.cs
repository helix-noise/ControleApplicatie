using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using Windows.Web.Http;
using MachineInspectionLibrary;

namespace MachineInspectie.Dal
{
    public class Locatie
    {
        private readonly Uri _apiLocatie = new Uri("http://vangansewinkel.vanlaer-it.be/api/location");
        //private HttpClient _client;
        //private string _response { get; set; }
        //private HttpResponseMessage _httpResponse;

        //public async Task<List<MachineInspectionLibrary.Location>> GetListLocation()
        //{
        //    try
        //    {
        //        using (_client = new HttpClient())
        //        {
        //            _response = await _client.GetStringAsync(_apiLocatie);

        //            //teststuk
        //            HttpResponseMessage testResponse = await _client.GetAsync(_apiLocatie);
        //            var responseMessage = testResponse.Content.ReadAsStringAsync().Result;
        //            List<Location> testLocationList =
        //                JsonConvert.DeserializeObject<LocationWrapper>(responseMessage).data;
        //        }

        //        List<Location> locatieLijst = JsonConvert.DeserializeObject<LocationWrapper>(_response).data;

        //        return locatieLijst;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public async Task<string> GetLocation()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync(_apiLocatie);
            client.Dispose();
            var message = httpResponse.Content.ReadAsStringAsync().Result;
            return message;
        }

        //public List<MachineInspectionLibrary.Location> GetListLocation()
        //{
        //    try
        //    {
        //        _webRequest = (HttpWebRequest)WebRequest.Create(_apiLocatie);
        //        _webRequest.Accept = "application/json";
        //        _webRequest.Method = HttpMethod.Get.ToString();
        //        var response = _webRequest.GetResponseAsync().Result;
        //        using (var sr = new StreamReader(response.GetResponseStream()))
        //        {
        //            _response = sr.ReadToEnd();
        //        }
        //        List<MachineInspectionLibrary.Location> LocatieLijst = JsonConvert.DeserializeObject<List<MachineInspectionLibrary.Location>>(_response);

        //        return LocatieLijst;
        //    }
        //    catch (Exception)
        //    {

        //        return new List<MachineInspectionLibrary.Location>();
        //    }
        //}


    }
}
