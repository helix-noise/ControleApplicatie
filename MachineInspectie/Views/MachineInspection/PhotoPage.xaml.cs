using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MachineInspectie.Model;
using MachineInspectionLibrary;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie.Views.MachineInspection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoPage : Page
    {
        private MediaCapture _captureManager;
        private bool _captureActive;
        private int _photoCount;
        private BitmapImage _photo;
        private List<ControlImage> _controlImages;
        //private ControlQuestion _controlQuestion;
        //private ControlAnswer _controlAnswer;
        private Helper _helper;
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public PhotoPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += BackButtonPress;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _helper = e.Parameter as Helper;
            btnCapture.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Neem foto" : "Prenez une photo";
            btnNextCapture.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Volgende foto" : "Prochaine photo";
            btnCaptureOk.Content = _localSettings.Values["Language"].ToString() == "nl" ? "Volgende vraag" : "Prochaine question";
            StartCamera();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= BackButtonPress;
        }

        private void BackButtonPress(Object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
        }

        #region UiButtons
        private async void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            //
            InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();
            //

            _photoCount += 1;
            btnCapture.IsEnabled = false;
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
            _photo = new BitmapImage(new Uri(file.Path));
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
            if (_helper.ControlQuestion.imageRequired && _photoCount == 0)
            {
                btnCaptureOk.IsEnabled = false;
            }
            StartCamera();
        }

        private async void btnCaptureOk_Click(object sender, RoutedEventArgs e)
        {
            btnNextCapture.Visibility = Visibility.Collapsed;
            btnCapture.Visibility = Visibility.Visible;
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

            //TODO: aanpassen
            _helper.ControlAnswer.images = _controlImages;
            this.Frame.Navigate(typeof (CommentPage), _helper);
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
