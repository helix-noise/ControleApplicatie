using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MachineInspectie.Views.MachineInspection;
using MachineInspectionLibrary;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuestionPage : Page
    {
        #region Private members
        private List<ControlQuestion> _questionList = new List<ControlQuestion>();
        private DateTime _startTimeQuestion;
        private static int _stepCounter;
        private string _language;
        private bool _testResult;
        private List<ControlAnswer> _answers = new List<ControlAnswer>();
        private ControlQuestion _controlQuestion;
        private Translation _translation;
        private MediaCapture _captureManager;
        private BitmapImage _photo;
        private bool _inspectionStarted;
        private bool _undoQuestion;
        private bool _captureActive;
        private int _photoCount;
        private List<ControlImage> _controlImages;
        private string _comment;
        #endregion

        public QuestionPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += BackButtonPress;
        }

        private async void BackButtonPress(Object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (_inspectionStarted)
            {
                string title;
                string message;
                string btnMessageOk;
                string btnMessageCancel;
                if (_language == "nl")
                {
                    title = "Waarschuwing";
                    message = "U keert terug naar de vorige vraag."+ Environment.NewLine +"Het ingevoerde antwoord zal worden verwijderd.";
                    btnMessageOk = "Doorgaan";
                    btnMessageCancel = "Annuleren";
                }
                else
                {
                    title = "Attention";
                    message = "Vous revenez à la question précédente, la réponse rempli sera enlevé.";
                    btnMessageOk = "Continuez";
                    btnMessageCancel = "Annulez";
                }
                var msg = new MessageDialog(message, title);
                var okBtn = new UICommand(btnMessageOk);
                var cancelBtn = new UICommand(btnMessageCancel);
                msg.Commands.Add(okBtn);
                msg.Commands.Add(cancelBtn);
                IUICommand result = await msg.ShowAsync();

                if (result != null && result.Label == btnMessageOk)
                {
                    _stepCounter -= 2;
                    if (_stepCounter == 0)
                    {
                        _inspectionStarted = false;
                        _answers = new List<ControlAnswer>();
                    }
                    else
                    {
                        _answers.RemoveAt(_stepCounter);
                    }
                    if (PhotoFrame.Visibility == Visibility.Visible)
                    {
                        PhotoFrame.Visibility = Visibility.Collapsed;
                        imgPhoto.Source = null;
                        cePreview.Source = null;
                        btnCaptureOk.IsEnabled = false;
                        btnCaptureReset.IsEnabled = false;
                        btnCapture.IsEnabled = true;
                    }
                    if (CommentFrame.Visibility == Visibility.Visible)
                    {
                        CommentFrame.Visibility = Visibility.Collapsed;
                    }
                    _undoQuestion = true;
                    DoInspection();
                }
            }
            else
            {
                if (Frame.CanGoBack)
                {
                    _stepCounter = 0;
                    Frame.GoBack();
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var locallanguage = Windows.Storage.ApplicationData.Current.LocalSettings;
            _language = locallanguage.Values["Language"].ToString();
            _questionList = (List<ControlQuestion>)e.Parameter;
            btnCapture.Content = _language == "nl" ? "Neem foto" : "Prenez une photo";
            btnNextCapture.Content = _language == "nl" ? "Volgende foto" : "Prochaine photo";
            btnCaptureOk.Content = _language == "nl" ? "Volgende vraag" : "Prochaine question";
            btnComment.Content = _language == "nl" ? "Volgende vraag" : "Prochaine question";
            DoInspection();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= BackButtonPress;
            _stepCounter = 0;
        }

        #region QuestionListFrame

        private void btnOk_Nok_Click(object sender, RoutedEventArgs e)
        {
            _inspectionStarted = true;
            _testResult = sender == btnOk;
            //Test1
            //if (_controlQuestion.imageRequired)
            //{
            //    PhotoFrame.Visibility = Visibility.Visible;
            //    StartCamera();
            //}
            //else
            //{
            //    DoInspection();
            //}

            //Test2
            PhotoFrame.Visibility = Visibility.Visible;
            StartCamera();
            if (_controlQuestion.imageRequired)
            {
                btnCaptureOk.IsEnabled = false;
            }
            else
            {
                btnCaptureOk.IsEnabled = true;
            }
        }

        public void DoInspection()
        {
            if (_stepCounter != 0 && _undoQuestion == false)
            {
                ControlAnswer answerWithImage = new ControlAnswer
                {
                    controlQuestionId = _controlQuestion.id,
                    startTime = _startTimeQuestion.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    endTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    testOk = _testResult,
                    comment = _comment 
                };
                if (_controlImages != null)
                {
                    //foreach (var controlImage in _controlImages)
                    //{
                    //    ControlImage img = new ControlImage();
                    //    img.fileName = photo;
                    //    answerWithImage.images = new List<ControlImage> { img };
                    //    _photoPath = null;
                    //}
                    answerWithImage.images = _controlImages;
                    _controlImages = null;
                }
                _answers.Add(answerWithImage);
            }
            _comment = null;
            _undoQuestion = false;
            _startTimeQuestion = DateTime.Now;
            if (_stepCounter < _questionList.Count)
            {
                _controlQuestion = _questionList[_stepCounter];
                _translation = _controlQuestion.translations[0];
                lblQuestion.Text = _translation.question;
                _stepCounter += 1;
            }
            else
            {
                var localSave = ApplicationData.Current.LocalSettings;
                ControlReport report = JsonConvert.DeserializeObject<ControlReport>(localSave.Values["TempControlReport"].ToString());
                report.endTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                localSave.Values["TempControlReport"] = JsonConvert.SerializeObject(report);
                this.Frame.Navigate(typeof(InspectionComplete), _answers);
            }

        }

        #endregion

        #region PhotoFrame

        private async void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            //
            InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();
            //

            _photoCount += 1;
            btnCapture.IsEnabled = false;
            btnCapture.Visibility = Visibility.Collapsed;
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("InspectionPhoto.jpg", CreationCollisionOption.GenerateUniqueName);
            //await _captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);
            await _captureManager.CapturePhotoToStreamAsync(imgFormat, imageStream);
            BitmapDecoder dec = await BitmapDecoder.CreateAsync(imageStream);
            BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(imageStream, dec);
            enc.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
            await enc.FlushAsync();
            StorageFile file =
                await
                    ApplicationData.Current.LocalFolder.CreateFileAsync("InspectionPhoto.jpg",
                        CreationCollisionOption.GenerateUniqueName);
            var filestream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAsync(imageStream, filestream);
            _photo = new BitmapImage(new Uri(file.Path));
            //_photoPath = new[] { file.Name };
            imgPhoto.Source = _photo;
            await _captureManager.StopPreviewAsync();
            btnCaptureReset.IsEnabled = true;
            btnCaptureOk.IsEnabled = true;
            _captureActive = false;
            btnNextCapture.Visibility = Visibility.Visible;
            if (_controlImages == null)
            {
                _controlImages = new List<ControlImage>();
            }
            _controlImages.Add(new ControlImage(file.Name));
        }

        private void btnNextCapture_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
            btnNextCapture.Visibility = Visibility.Collapsed;
            btnCapture.Visibility = Visibility.Visible;
            btnCapture.IsEnabled = true;
        }

        private void btnCaptureReset_Click(object sender, RoutedEventArgs e)
        {
            _photoCount -= 1;
            imgPhoto.Source = null;
            _photo = null;
            btnCapture.IsEnabled = true;
            btnCaptureReset.IsEnabled = false;

            btnNextCapture.Visibility = Visibility.Collapsed;
            btnCapture.Visibility = Visibility.Visible;
            if (_controlImages.Count == 1)
            {
                _controlImages = new List<ControlImage>();
            }
            else
            {
                _controlImages.RemoveAt(_photoCount);
            }
            if (_controlQuestion.imageRequired && _photoCount == 0)
            {
                btnCaptureOk.IsEnabled = false;
            }
            StartCamera();
        }

        private async void btnCaptureOk_Click(object sender, RoutedEventArgs e)
        {
            btnNextCapture.Visibility = Visibility.Collapsed;
            btnCapture.Visibility = Visibility.Visible;
            PhotoFrame.Visibility = Visibility.Collapsed;
            imgPhoto.Source = null;
            cePreview.Source = null;
            btnCaptureOk.IsEnabled = false;
            btnCaptureReset.IsEnabled = false;
            btnCapture.IsEnabled = true;
            if (_captureActive)
            {
                await _captureManager.StopPreviewAsync();
                _captureActive = false;
            }
            _photoCount = 0;
            //DoInspection();
            lblComment.Text = _language == "nl"
                ? "Wenst u een opmerking te geven over:" + Environment.NewLine + _controlQuestion.translations[0].question
                : "Voudriez-vous donner une remarque sur:" + Environment.NewLine + _controlQuestion.translations[0].question;
            CommentFrame.Visibility = Visibility.Visible;
        }

        public async void StartCamera()
        {
            var cameraId = await GetCameraId(Windows.Devices.Enumeration.Panel.Back);
            _captureManager = new MediaCapture();
            await _captureManager.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                PhotoCaptureSource = PhotoCaptureSource.Photo,
                AudioDeviceId = string.Empty,
                VideoDeviceId = cameraId.Id
            });
            cePreview.Source = _captureManager;
            _captureManager.SetPreviewRotation(VideoRotation.Clockwise180Degrees);
            _captureActive = true;
            await _captureManager.StartPreviewAsync();
        }
        private static async Task<DeviceInformation> GetCameraId(Windows.Devices.Enumeration.Panel desired)
        {
            DeviceInformation deviceId =
                (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).FirstOrDefault(
                    x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desired);
            if (deviceId != null)
            {
                return deviceId;
            }
            else throw new Exception(string.Format("Camera of type {0} doesn't exist.", desired));
        }



        #endregion

        #region CommentFrame

        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            _comment = txtComment.Text;
            txtComment.Text = string.Empty;
            CommentFrame.Visibility = Visibility.Collapsed;
            DoInspection();
        }

        #endregion

        //public static Task WheneClicked(Button button)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    RoutedEventHandler handler = null;
        //    handler = (sender, e) =>
        //    {
        //        tcs.TrySetResult(true);
        //        button.Click -= handler;
        //    };
        //    button.Click += handler;
        //    return tcs.Task;
        //}


    }
}
