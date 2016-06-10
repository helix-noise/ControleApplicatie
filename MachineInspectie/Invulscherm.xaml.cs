using System.Collections.Generic;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MachineInspectionLibrary;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MachineInspectie
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Invulscherm : Page
    {
        private string _taal;
        public string HeaderTaal { get; set; }

        public Invulscherm()
        {
            this.InitializeComponent();
        }

        #region TestData
        ///TestData
        //private List<MachineInspectionLibrary.Location> TestList()
        //{
        //    List<MachineInspectionLibrary.Location> temp = new List<MachineInspectionLibrary.Location>();
        //    temp.Add(new MachineInspectionLibrary.Location(1, 1111, "Locatie1"));
        //    temp.Add(new MachineInspectionLibrary.Location(2, 1222, "Locatie2"));
        //    temp.Add(new MachineInspectionLibrary.Location(3, 1333, "Locatie3"));
        //    temp.Add(new MachineInspectionLibrary.Location(4, 1444, "Locatie4"));
        //    temp.Add(new MachineInspectionLibrary.Location(5, 1555, "Locatie5"));
        //    temp.Add(new MachineInspectionLibrary.Location(6, 1666, "Locatie6"));
        //    temp.Add(new MachineInspectionLibrary.Location(7, 1777, "Locatie7"));
        //    temp.Add(new MachineInspectionLibrary.Location(8, 1888, "Locatie8"));
        //    return temp;
        //}

        //private List<Matis> TestListMatis()
        //{
        //    List<Matis> temp = new List<Matis>();
        //    temp.Add(new Matis(1, 111, "Matis1", new MatisCategory(1, "MatisCath1"), new MachineInspectionLibrary.Location(1, 3366, "Locatie1")));
        //    temp.Add(new Matis(2, 222, "Matis2", new MatisCategory(2, "MatisCath2"), new MachineInspectionLibrary.Location(2, 8899, "Locatie2")));
        //    temp.Add(new Matis(3, 333, "Matis3", new MatisCategory(3, "MatisCath3"), new MachineInspectionLibrary.Location(3, 4561, "Locatie3")));
        //    temp.Add(new Matis(4, 444, "Matis4", new MatisCategory(4, "MatisCath4"), new MachineInspectionLibrary.Location(4, 7851, "Locatie4")));
        //    temp.Add(new Matis(5, 555, "Matis5", new MatisCategory(5, "MatisCath5"), new MachineInspectionLibrary.Location(5, 2564, "Locatie5")));
        //    return temp;
        //}
#endregion


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _taal = e.Parameter.ToString();
            if (_taal == "Nl")
            {
                lblNaam.Text = "Naam";
                lblLocatie.Text = "Locatie";
                lblMatis.Text = "Matis";
                lblUur.Text = "Uur";
                btnStart.Content = "Start controle";
                btnReset.Content = "Reset";
                HeaderTaal = "Maak u keuze";
                btnLocatie.Content = "Selecteer een locatie";
            }
            else
            {
                lblNaam.Text = "Nom";
                lblLocatie.Text = "Emplacement";
                lblMatis.Text = "Matis";
                lblUur.Text = "Heure";
                btnStart.Content = "Lancer le contrôle";
                btnReset.Content = "Réinitialiser";
                HeaderTaal = "Faites votre choix";
                btnLocatie.Content = "Choisissez votre lieu";
            }
        }

        private async void ListPickerLocatie_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            MachineInspectionLibrary.Location temp = (MachineInspectionLibrary.Location)ListPickerLocatie.SelectedItem;
            btnLocatie.Content = temp.name;
            Controller.Matis matisController = new Controller.Matis();
            ListPickerMatis.ItemsSource = await matisController.GetMatisByLocation(temp.name);
            ListPickerMatis.SelectedValuePath = "id";
            ListPickerMatis.DisplayMemberPath = "DisplayName";
            if (_taal == "Nl")
            {
                btnMatis.Content = "Selecteer matis";
            }
            else
            {
                btnMatis.Content = "Choisissez un matis";
            }
            btnMatis.IsEnabled = true;
        }

        private void ListPickerMatis_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            Matis temp = (Matis)ListPickerMatis.SelectedItem;
            btnMatis.Content = temp.name;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtUur.Text = "";
            btnMatis.Content = "";
            ListPickerMatis.SelectedIndex = -1;
            if (_taal == "Nl")
            {
                btnLocatie.Content = "Selecteer een locatie";
            }
            else
            {
                btnLocatie.Content = "Choisissez votre lieu";
            }
            ListPickerLocatie.SelectedIndex = -1;
            txtNaam.Text = "";
        }

        private async void btnLocatie_Click(object sender, RoutedEventArgs e)
        {
            Controller.Locatie loc = new Controller.Locatie();
            ListPickerLocatie.ItemsSource = await loc.LocationList();
            ListPickerLocatie.SelectedValuePath = "name";
            ListPickerLocatie.DisplayMemberPath = "DisplayName";
        }

        private void btnMatis_Click(object sender, RoutedEventArgs e)
        {

        }

        //private async void txtNaam_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Controller.Locatie loc = new Controller.Locatie();
        //    ListPickerLocatie.ItemsSource = await loc.GetListLocation();
        //    ListPickerLocatie.SelectedValuePath = "name";
        //    ListPickerLocatie.DisplayMemberPath = "DisplayNaam";
        //    if (_taal == "Nl")
        //    {
        //        btnLocatie.Content = "Selecteer een locatie";
        //    }
        //    else
        //    {
        //        btnLocatie.Content = "Choisissez votre lieu";
        //    }
        //    btnLocatie.IsEnabled = true;
        //}
    }
}
