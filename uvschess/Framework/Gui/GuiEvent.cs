using System;
using System.Collections.Generic;
using System.Text;

namespace UvsChess.Gui
{
    public class GuiEvent
    {
        public UpdateWinGuiOnTimer.ObjectParameterCallback EventCallback { get; set; }
        public object[] EventArgs { get; set; }

        public GuiEvent(UpdateWinGuiOnTimer.ObjectParameterCallback eventCallback, params object[] eventArgs)
        {
            EventCallback = eventCallback;
            EventArgs = eventArgs;
        }
    }
}
