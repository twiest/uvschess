using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using Gdk;

namespace UvsChess
{
    class LogWindow : Gtk.Window
    {
        public LogWindow(ChessColor playerColor)
            : base("")
        {
            this.Title = playerColor.ToString();
            Gtk.TextView view;
            Gtk.TextBuffer buffer;
            this.SetDefaultSize(400, 300);

            view = new Gtk.TextView();
            this.Add(view);
            buffer = view.Buffer;

            buffer.Text = playerColor.ToString() + " log";

            this.ShowAll();

            this.DeleteEvent += new DeleteEventHandler(LogWindow_DeleteEvent);//Close window event
            this.Visible = false;


        }

        void LogWindow_DeleteEvent(object o, DeleteEventArgs args)
        {
            //Just hide the window. Don't destroy it.
            //this.Visible = false;
        }

        public void Log(string message)
        {
        }
           
    }
}
