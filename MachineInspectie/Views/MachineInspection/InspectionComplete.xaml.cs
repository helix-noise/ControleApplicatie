using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Popups;
using MachineInspectie.Dal;
using MachineInspectionLibrary;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie.Views.MachineInspection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InspectionComplete : Page
    {
        private string _language;
        private ControlReport _controlReport;
        private List<ControlAnswer> _answers;
        private int _sendCount = 0;


        public InspectionComplete()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += BackButtonPress;
        }

        private void BackButtonPress(Object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                _answers = (List<ControlAnswer>)e.Parameter;
                var localSave = ApplicationData.Current.LocalSettings;
                _language = localSave.Values["Language"].ToString();
                _controlReport = JsonConvert.DeserializeObject<ControlReport>(localSave.Values["TempControlReport"].ToString());
                if (_language == "nl")
                {
                    lblComplete.Text = "Controle is voltooid";
                    btnSend.Content = "Controle verzenden";
                }
                else
                {
                    lblComplete.Text = "Contrôle terminé";
                    btnSend.Content = "Envoyez la contrôle";
                }
            }
            else
            {
                var previousControl = ApplicationData.Current.LocalSettings;
                _language = previousControl.Values["Language"].ToString();
                _answers = JsonConvert.DeserializeObject<List<ControlAnswer>>(previousControl.Values["PreviousAnswers"].ToString());
                _controlReport = JsonConvert.DeserializeObject<ControlReport>(previousControl.Values["PreviousReport"].ToString());
                if (_language == "nl")
                {
                    lblComplete.Text = "Vorige Controle verzenden";
                    btnSend.Content = "Controle verzenden";
                }
                else
                {
                    lblComplete.Text = "Envoyer la dernière contrôle";
                    btnSend.Content = "Envoyez la contrôle";
                }
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= BackButtonPress;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            imgOk.Visibility = Visibility.Collapsed;
            imgNok.Visibility = Visibility.Collapsed;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                btnSend.IsEnabled = false;
                if (_language == "nl")
                {
                    lblComplete.Text = "Uw controle wordt verzonden" + Environment.NewLine + "Even geduld aub ...";
                }
                else
                {
                    lblComplete.Text = "Votre contrôle est en train d'être envoyé" + Environment.NewLine + "Veuillez patienter un instant svp ...";
                }
                prSendData.Visibility = Visibility.Visible;
                prSendData.IsActive = true;
                PostQuestionList pQL = new PostQuestionList();
                string responce = await pQL.SendControlRapport(_answers, _controlReport);
                if (responce == "Ok")
                {
                    lblComplete.Text = _language == "nl" ? "Uw controle werd met succes verzonden" : "L'envoi de votre contrôle a réussi";
                    imgOk.Visibility = Visibility.Visible;
                    btnHome.Content = _language == "nl" ? "Naar beginscherm" : "À l'écran d'accueil";
                    btnSend.Visibility = Visibility.Collapsed;
                    btnHome.Visibility = Visibility.Visible;
                    DeletePictures();
                    ClearPreviousControl();
                }
                else
                {
                    lblComplete.Text = _language == "nl"
                        ? "Er is een fout opgetreden" + Environment.NewLine + "Probeer opnieuw..."
                        : "Une erreur est survenue" + Environment.NewLine + "Réessayez...";
                    imgNok.Visibility = Visibility.Visible;
                    _sendCount += 1;
                    if (_sendCount > 3)
                    {
                        lblComplete.Text = _language == "nl"
                            ? "Controle kon niet verzonden worden !" + Environment.NewLine +
                              "De controle wordt lokaal opgeslagen en zal bij de volgende opstart verzonden worden."
                            : "Le contrôle n'a pas été envoyé !" + Environment.NewLine +
                              "Le contrôle sera enregistré localement et sera envoyé avec la prochaine démarrage.";
                        var localStorage = ApplicationData.Current.LocalSettings;
                        localStorage.Values["PreviousAnswers"] = JsonConvert.SerializeObject(_answers);
                        localStorage.Values["PreviousReport"] = JsonConvert.SerializeObject(_controlReport);
                        btnHome.Content = _language == "nl" ? "Naar beginscherm" : "À l'écran d'accueil";
                        btnSend.Visibility = Visibility.Collapsed;
                        btnHome.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnSend.Content = _language == "nl" ? "Probeer opnieuw" : "Réessayez";
                        btnSend.IsEnabled = true;
                    }
                }
                prSendData.IsActive = false;
                prSendData.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoConnectionError();
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            var localSave = ApplicationData.Current.LocalSettings;
            localSave.Values["TempControlReport"] = null;
            localSave.Values["Templocation"] = null;
            localSave.Values["TempMatis"] = null;
            this.Frame.Navigate(typeof(SelectionPage));
        }

        public async void DeletePictures()
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
        }

        private void ClearPreviousControl()
        {
            var localSave = ApplicationData.Current.LocalSettings;
            localSave.Values["PreviousAnswers"] = null;
            localSave.Values["PreviousReport"] = null;
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
    }
}
