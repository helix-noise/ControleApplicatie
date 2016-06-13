using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MachineInspectie.Model;
using MachineInspectie.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie.Views.MachineInspection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommentPage : Page
    {
        private ControlObject _controlObject;
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public CommentPage()
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
            _controlObject = e.Parameter as ControlObject;
            lblComment.Text = _localSettings.Values["Language"].ToString() == "nl"
                ? "Wenst u een opmerking te geven over:" + Environment.NewLine +
                  _controlObject.ControlQuestion.translations[0].question
                : "Voudriez-vous donner une remarque sur:" + Environment.NewLine +
                  _controlObject.ControlQuestion.translations[0].question;
            btnNext.Content = _localSettings.Values["Language"].ToString() == "nl"
                ? "Volgende vraag"
                : "Prochaine question";
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
                _controlObject.ControlAnswer.images.Clear();
                this.Frame.Navigate(typeof (PhotoPage), _controlObject);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            _controlObject.ControlAnswer.comment = txtComment.Text;
            _controlObject.ControlAnswer.endTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            this.Frame.Navigate(typeof (QuestionPage), _controlObject.ControlAnswer);
        }
    }
}
