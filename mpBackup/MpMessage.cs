using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup.MpGUI
{
    /// <summary>
    /// Used for sending message across the application, allows to specify how they should be displayed.
    /// </summary>
    public class MpMessage
    {
        public enum DisplayAs
        {
            BALOON,
            POPUP
        }

        public string text { get; set; }
        public DisplayAs displayAs { get; set; }
    }
}
