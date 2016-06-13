using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MachineInspectie.Model;
using MachineInspectie.Services;
using MachineInspectionLibrary;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie.Views.MachineInspection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoPage : Page
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        private MediaCapture _captureManager;
        private bool _captureActive;
        private int _photoCount;
        private List<ControlImage> _controlImages;
        //private ControlQuestion _controlQuestion;
        //private ControlAnswer _controlAnswer;
        private ControlObject _controlObject;

        public PhotoPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += BackButtonPress;
            _photoCount = 0;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _controlObject = e.Parameter as ControlObject;
            btnCapture.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Neem foto" : "Prenez une photo";
            btnNextCapture.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Volgende foto" : "Prochaine photo";
            btnCaptureOk.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Volgende vraag" : "Prochaine question";
            StartCamera();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= BackButtonPress;
        }

        private async void BackButtonPress(Object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            ErrorWarningMessage message = new ErrorWarningMessage();
            if (await message.ReturnPageWarning(_localSettings.Values["Language"].ToString()) == "Ok")
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                IReadOnlyList<StorageFile> filesInFolder = await folder.GetFilesAsync();
                if (filesInFolder.Count != 0)
                {
                    foreach (var storageFile in filesInFolder)
                    {
                        var deleteFile = await folder.GetFileAsync(storageFile.Name);
                        await deleteFile.DeleteAsync();
                    }
                }
                if (_captureActive)
                {
                    await _captureManager.StopPreviewAsync();
                    _captureActive = false;
                }
                this.Frame.Navigate(typeof(QuestionPage));
            }
        }

        #region UiButtons
        private async void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();
            _photoCount += 1;
            btnCapture.Visibility = Visibility.Collapsed;
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
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
            imgPhoto.Source = new BitmapImage(new Uri(file.Path));
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
            btnCaptureReset.IsEnabled = false;
            btnCaptureOk.IsEnabled = false;
        }

        private void btnCaptureReset_Click(object sender, RoutedEventArgs e)
        {
            _photoCount -= 1;
            imgPhoto.Source = null;
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
            if (_controlObject.ControlQuestion.imageRequired && _photoCount == 0)
            {
                btnCaptureOk.IsEnabled = false;
            }
            StartCamera();
        }

        private void btnCaptureOk_Click(object sender, RoutedEventArgs e)
        {
            //btnNextCapture.Visibility = Visibility.Collapsed;
            //btnCapture.Visibility = Visibility.Visible;
            //imgPhoto.Source = null;
            //cePreview.Source = null;
            //btnCaptureOk.IsEnabled = false;
            //btnCaptureReset.IsEnabled = false;
            //if (_captureActive)
            //{
            //    await _captureManager.StopPreviewAsync();
            //    _captureActive = false;
            //}

            //TODO: aanpassen
            _controlObject.ControlAnswer.images = _controlImages;
            this.Frame.Navigate(typeof(CommentPage), _controlObject);
        }
        #endregion

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
            else throw new Exception($"Camera of type {desired} doesn't exist.");
        }

    }
}
