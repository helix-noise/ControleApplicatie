using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MachineInspectie.Services
{
    public class ErrorWarningMessage
    {
        public async Task<string> ReturnPageWarning(string language)
        {
            string title;
            string message;
            string btnMessageOk;
            string btnMessageCancel;
            if (language == "nl")
            {
                title = "Waarschuwing";
                message = "U keert terug naar het vorige scherm." + Environment.NewLine + "Het ingevoerde antwoord zal worden verwijderd.";
                btnMessageOk = "Doorgaan";
                btnMessageCancel = "Annuleren";
            }
            else
            {
                title = "Attention";
                message = "Vous revenez à l'écran précédent, la réponse rempli sera enlevé.";
                btnMessageOk = "Continuez";
                btnMessageCancel = "Annulez";
            }
            var msg = new MessageDialog(message, title);
            var okBtn = new UICommand(btnMessageOk);
            var cancelBtn = new UICommand(btnMessageCancel);
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
            if (result.Label == "Doorgaan" || result.Label == "Continuez")
            {
                result.Label = "Ok";
            }
            return result.Label;
        }
    }
}
