using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using MachineInspectionLibrary;
using Newtonsoft.Json;

namespace MachineInspectie.Dal
{
    public class PostQuestionList
    {
        private readonly Uri _apiControlImage = new Uri("http://vangansewinkel.vanlaer-it.be/api/controlimage");
        private readonly Uri _apiControlReport = new Uri("http://vangansewinkel.vanlaer-it.be/api/controlreport");
        private HttpClient _client;
        private string _response;
        private List<ControlAnswer> _controlAnswers;

        public async Task<string> SendControlRapport(List<ControlAnswer> imageSendList, ControlReport report)
        {
            try
            {
                List<ControlAnswer> sendList = new List<ControlAnswer>();
                foreach (var controlAnswerImage in imageSendList)
                {
                    ControlAnswer sendAnswer = new ControlAnswer();
                    sendAnswer.controlQuestionId = controlAnswerImage.controlQuestionId;
                    sendAnswer.startTime = controlAnswerImage.startTime;
                    sendAnswer.endTime = controlAnswerImage.endTime;
                    sendAnswer.testOk = controlAnswerImage.testOk;
                    sendAnswer.comment = controlAnswerImage.comment;
                    if (controlAnswerImage.images != null)
                    {
                        sendAnswer.images = new List<ControlImage>();
                        foreach (ControlImage imgPath in controlAnswerImage.images)
                        {
                            var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(imgPath.fileName);
                            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(storageFile);
                            var bufferArray = buffer.ToArray();
                            _client = new HttpClient();
                            MultipartFormDataContent form = new MultipartFormDataContent();
                            form.Add(new ByteArrayContent(bufferArray), "image", imgPath.fileName);
                            HttpResponseMessage response = await _client.PostAsync(_apiControlImage, form);
                            response.EnsureSuccessStatusCode();
                            _client.Dispose();
                            var responsemessage = response.Content.ReadAsStringAsync().Result;
                            ControlImage img = JsonConvert.DeserializeObject<ControlImageWrapper>(responsemessage).data;
                            sendAnswer.images.Add(img);
                        }
                    }
                    sendList.Add(sendAnswer);
                }
                report.controlAnswers = sendList;
                var sendReport = JsonConvert.SerializeObject(report, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                using (_client = new HttpClient())
                {
                    HttpResponseMessage response = await _client.PostAsync(_apiControlReport, new StringContent(sendReport, Encoding.UTF8, "application/json"));
                    response.EnsureSuccessStatusCode();
                }
                return "Ok";
            }
            catch (Exception)
            {
                return "Nok";
            }
        }

        //public async Task<List<ControlAnswer>> SendImage(List<ControlAnswer> listWithImages)
        //{
        //    List<ControlAnswer> sendList = new List<ControlAnswer>();
        //    foreach (var controlAnswerImage in listWithImages)
        //    {
        //        ControlAnswer sendAnswer = new ControlAnswer();
        //        sendAnswer.controlQuestionId = controlAnswerImage.controlQuestionId;
        //        sendAnswer.startTime = controlAnswerImage.startTime;
        //        sendAnswer.endTime = controlAnswerImage.endTime;
        //        sendAnswer.testOk = controlAnswerImage.testOk;
        //        sendAnswer.comment = controlAnswerImage.comment;
        //        if (controlAnswerImage.images.Count != 0)
        //        {
        //            sendAnswer.images = new List<ControlImage>();
        //            foreach (ControlImage imgPath in controlAnswerImage.images)
        //            {
        //                var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(imgPath.fileName);
        //                var buffer = await Windows.Storage.FileIO.ReadBufferAsync(storageFile);
        //                var bufferArray = buffer.ToArray();
        //                _client = new HttpClient();
        //                MultipartFormDataContent form = new MultipartFormDataContent();
        //                form.Add(new ByteArrayContent(bufferArray), "image", imgPath.fileName);
        //                HttpResponseMessage response = await _client.PostAsync(_apiControlImage, form);
        //                response.EnsureSuccessStatusCode();
        //                _client.Dispose();
        //                var responsemessage = response.Content.ReadAsStringAsync().Result;
        //                ControlImage img = JsonConvert.DeserializeObject<ControlImageWrapper>(responsemessage).data;
        //                sendAnswer.images.Add(img);
        //            }
        //        }
        //        sendList.Add(sendAnswer);
        //    }
        //    return sendList;
        //}

        //public async Task<string> SendControlReport(List<ControlAnswer> sendList, ControlReport report)
        //{
        //    report.controlAnswers = sendList;
        //    var sendReport = JsonConvert.SerializeObject(report, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //    using (_client = new HttpClient())
        //    {
        //        //StringContent content = new StringContent(sendReport,Encoding.UTF8,"application/json");
        //        HttpResponseMessage response = await _client.PostAsync(_apiControlReport, new StringContent(sendReport, Encoding.UTF8, "application/json"));

        //        response.EnsureSuccessStatusCode();
        //        return response.StatusCode.ToString();
        //    }
        //}
    }
}
