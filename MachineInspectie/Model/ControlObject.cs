using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineInspectionLibrary;

namespace MachineInspectie.Model
{
    public class ControlObject
    {
        public ControlQuestion ControlQuestion { get; set; }
        public ControlAnswer ControlAnswer { get; set; }

        public ControlObject()
        {
            
        }

        public ControlObject(ControlQuestion controlQuestion, ControlAnswer controlAnswer)
        {
            this.ControlQuestion = controlQuestion;
            this.ControlAnswer = controlAnswer;
        }
    }
}
