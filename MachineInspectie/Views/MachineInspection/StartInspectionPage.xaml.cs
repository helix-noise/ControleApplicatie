using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MachineInspectionLibrary;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MachineInspectie.Dal;
using MachineInspectie.Views.MachineInspection;
using Newtonsoft.Json;
using Matis = MachineInspectionLibrary.Matis;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartInspectionPage : Page
    {
        private string _language;
        private Location _selectedLocation;
        private Matis _selectedMatis;
        public string ListHeaderLanguage { get; set; }
        private ControlReport _controlReport;

        public StartInspectionPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += BackButtonPress;
        }

        private void BackButtonPress(Object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= BackButtonPress;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var localSaved = Windows.Storage.ApplicationData.Current.LocalSettings;
            _language = localSaved.Values["Language"].ToString();
            if (_language == "nl")
            {
                UiDutch();
            }
            else
            {
                UiFrench();
            }
            if (localSaved.Values["TempControlReport"] == null) return;
            _controlReport = JsonConvert.DeserializeObject<ControlReport>(localSaved.Values["TempControlReport"].ToString());
            _selectedLocation = JsonConvert.DeserializeObject<Location>(localSaved.Values["Templocation"].ToString());
            _selectedMatis = JsonConvert.DeserializeObject<Matis>(localSaved.Values["TempMatis"].ToString());
            txtName.Text = _controlReport.user;
            txtHour.Text = _controlReport.matisServiceTime.ToString();
            btnLocation.Content = _selectedLocation.name;
            btnMatis.Content = _selectedMatis.name;
            btnMatis.IsEnabled = true;
            btnStart.IsEnabled = true;
        }

        private void ListPickerLocatie_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            _selectedLocation = (Location)ListPickerLocatie.SelectedItem;
            btnLocation.Content = _selectedLocation.name;
            btnMatis.Content = _language == "nl" ? "Selecteer matis" : "Choisissez un matis";
            btnMatis.IsEnabled = true;
        }

        private void ListPickerMatis_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            _selectedMatis = (Matis)ListPickerMatis.SelectedItem;
            btnMatis.Content = _selectedMatis.name;
            btnStart.IsEnabled = true;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtHour.Text = "";
            btnMatis.Content = "";
            btnMatis.IsEnabled = false;
            ListPickerMatis.SelectedIndex = -1;
            btnLocation.Content = _language == "nl" ? "Selecteer een locatie" : "Choisissez votre lieu";
            ListPickerLocatie.SelectedIndex = -1;
            txtName.Text = "";
        }

        private async void btnMatis_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                ListPickerMatis.ItemsSource = null;
                Dal.Matis matisController = new Dal.Matis();
                string message = await matisController.GetMatis(_selectedLocation.name);
                if (message.Contains("data"))
                {
                    ListPickerMatis.ItemsSource = JsonConvert.DeserializeObject<MatisWrapper>(message).data;
                    ListPickerMatis.SelectedValuePath = "id";
                    ListPickerMatis.DisplayMemberPath = "name";
                }
                else
                {
                    ErrorMessage();
                }
            }
            else
            {
                NoConnectionError();
            }

        }

        private async void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                ListPickerLocatie.ItemsSource = null;
                Locatie location = new Locatie();
                string message = await location.GetLocation();
                if (message.Contains("data"))
                {
                    ListPickerLocatie.ItemsSource = JsonConvert.DeserializeObject<LocationWrapper>(message).data;
                    ListPickerLocatie.SelectedValuePath = "name";
                    ListPickerLocatie.DisplayMemberPath = "name";
                }
                else
                {
                    ErrorMessage();
                }
            }
            else
            {
                NoConnectionError();
            }

        }
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text))
            {
                if (!string.IsNullOrEmpty(txtHour.Text))
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        _controlReport = new ControlReport
                        {
                            languageId = _language == "nl" ? 1 : 2,
                            locationId = _selectedLocation.id,
                            matisId = _selectedMatis.id,
                            matisServiceTime = int.Parse(txtHour.Text),
                            user = txtName.Text,
                            controlAnswers = new List<ControlAnswer>(),
                            startTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                        };
                        var tempSave = ApplicationData.Current.LocalSettings;
                        tempSave.Values["TempMatis"] = JsonConvert.SerializeObject(_selectedMatis);
                        tempSave.Values["TempLocation"] = JsonConvert.SerializeObject(_selectedLocation);
                        tempSave.Values["TempControlReport"] = JsonConvert.SerializeObject(_controlReport);
                        ControlQuestions questions = new ControlQuestions();
                        string response = await questions.GetControlQuestion(_selectedMatis.Category.name, _language);
                        if (response.Contains("data"))
                        {
                            List<ControlQuestion> controlQuestions =
                                JsonConvert.DeserializeObject<ControlQuestionWrapper>(response).data;
                            this.Frame.Navigate(typeof (QuestionPage), controlQuestions);
                        }
                        else
                        {
                            ErrorMessage();
                        }
                        //this.Frame.Navigate(typeof(QuestionPage), await questions.ControlQuestionList(_selectedMatis.Category.name, _language));
                    }
                    else
                    {
                        NoConnectionError();
                    }
                }
                else
                {
                    txtHour.BorderBrush = new SolidColorBrush(Colors.Red);
                    CheckStart(_language, lblHour.Text);
                }
                txtName.ClearValue(BorderBrushProperty);
            }
            else
            {
                txtName.BorderBrush = new SolidColorBrush(Colors.Red);
                CheckStart(_language, lblName.Text);
            }
        }

        private void txtHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] invalidChars = { ",", "-", "." };
            foreach (string t in invalidChars)
            {
                if (txtHour.Text != null) txtHour.Text = txtHour.Text.Replace(t, "");
            }
            if (txtHour.Text != null) txtHour.SelectionStart = txtHour.Text.Length;
        }

        #region Constructors

        private async void CheckStart(string language, string field)
        {
            var msg = language == "nl"
                ? new MessageDialog("Veld vergeten " + field + " !")
                : new MessageDialog("Champ " + field + " n'est pas rempli!");
            var okBtn = new UICommand("Ok");
            msg.Commands.Add(okBtn);
            IUICommand result = await msg.ShowAsync();
            if (result != null && result.Label == "Ok")
            {
                return;
            }
        }

        private async void NoConnectionError()
        {
            string title;
            string message;
            if (_language == "nl")
            {
                title = "Geen internet verbinding";
                message = "Er is geen internet verbinding gelieven eerst verbinding te maken.";
            }
            else
            {
                title = "Connexion internet pas retrouvé";
                message = "Aucune connexion internet a été trouvé, veuillez d'abord se connecter à l'internet.";
            }
            var msg = new MessageDialog(message, title);
            var okBtn = new UICommand("Ok");
            msg.Commands.Add(okBtn);
            IUICommand result = await msg.ShowAsync();

            if (result != null && result.Label == "Ok")
            {
                return;
            }
        }

        private async void ErrorMessage()
        {
            string title;
            string message;
            if (_language == "nl")
            {
                title = "Error";
                message = "Er heeft zich een probleem voorgedaan."
                    + Environment.NewLine + "Probeer het later opnieuw.";
            }
            else
            {
                title = "Erreur";
                message = "Il y a eu une erreur." + Environment.NewLine + "Réessayez plus tard.";
            }
            var msg = new MessageDialog(message, title);
            var okBtn = new UICommand("Ok");
            msg.Commands.Add(okBtn);
            IUICommand result = await msg.ShowAsync();

            if (result != null && result.Label == "Ok")
            {
                return;
            }
        }

        private void UiDutch()
        {
            lblName.Text = "Naam";
            lblLocation.Text = "Locatie";
            lblMatis.Text = "Matis";
            lblHour.Text = "Uur";
            btnStart.Content = "Start";
            btnReset.Content = "Reset";
            ListHeaderLanguage = "Maak uw keuze";
            btnLocation.Content = "Selecteer een locatie";
        }

        private void UiFrench()
        {
            lblName.Text = "Nom";
            lblLocation.Text = "Centre de tri";
            lblMatis.Text = "Matis";
            lblHour.Text = "Heures";
            btnStart.Content = "Start";
            btnReset.Content = "Reset";
            ListHeaderLanguage = "Faites votre choix";
            btnLocation.Content = "Choisissez votre lieu";
        }


        #endregion


    }
}
