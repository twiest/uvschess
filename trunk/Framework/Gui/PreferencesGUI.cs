#if USE_GTK
using System;
using System.Collections.Generic;
using Gtk;
using Gdk;

namespace UvsChess.Gui
{
    class PreferencesGUI : Gtk.Dialog
    {
        private TextBuffer grace_buffer = null;
        private TextBuffer time_buffer = null;

        public PreferencesGUI()
        {
            this.Title = "UvsChess Preferences";
            this.Modal = true;

            Table table = new Table(2, 2, true);
            //VBox vertbox = new VBox();
            //this.Add(vertbox);

            //HBox hbox_time = new HBox();
            Label lblTime = new Label("Length of turn (seconds)");
            //hbox_time.PackStart(lblTime);

            //TextBuffer time_buffer = new TextBuffer(new TextTagTable());
            time_buffer = new TextBuffer(new TextTagTable());
            TextView time_view = new TextView(time_buffer);
            time_view.AcceptsTab = false;
            
            //hbox_time.PackStart(time_view);

            table.Attach(lblTime, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 5, 5);
            table.Attach(time_view, 1, 2, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 5, 5);            

            //HBox hbox_grace = new HBox();
            Label lblGrace = new Label("Length of grace period (seconds)");
            //hbox_grace.PackStart(lblGrace);

            //TextBuffer grace_buffer = new TextBuffer(new TextTagTable());
            grace_buffer = new TextBuffer(new TextTagTable());
            TextView grace_view = new TextView(grace_buffer);
            grace_view.AcceptsTab = false;
            //hbox_grace.PackStart(grace_view);
            
            table.Attach(lblGrace, 0, 1, 1, 2, AttachOptions.Fill, AttachOptions.Fill, 5, 5);
            table.Attach(grace_view, 1, 2, 1, 2, AttachOptions.Fill, AttachOptions.Fill, 5, 5);

            //vertbox.PackStart(hbox_time);
            //vertbox.PackStart(table);
            //this.Add(table);
            this.VBox.PackStart(table);
            //this.Add(table);
            this.AddButton("OK", ResponseType.Ok);
            this.AddButton("Cancel", ResponseType.Cancel);
            this.Response += new ResponseHandler(ChessPreferences_Response);

            //READ settings from file. 
            //ReadPrefsFromFile();
            time_buffer.Text = (UserPrefs.Time / 1000.0).ToString().ToString();
            grace_buffer.Text = (UserPrefs.GracePeriod / 1000.0).ToString();
            this.ShowAll();
            
        }

        void ChessPreferences_Response(object o, ResponseArgs args)
        {
            ResponseType response = args.ResponseId;
            
            if (response == ResponseType.Ok)
            {
                Program.Log("OK clicked");
                string str_time = time_buffer.Text;
                string str_grace = grace_buffer.Text;
                try
                {
                    //throw new Exception();
                    double time = Convert.ToDouble(str_time);
                    UserPrefs.Time = (int)(time * 1000);

                    double grace = Convert.ToDouble(str_grace);
                    UserPrefs.GracePeriod = (int)(grace * 1000);

                    Program.Log(String.Format("Turn length set to {0}", UserPrefs.Time));
                    Program.Log(String.Format("Grace period set to {0}", UserPrefs.GracePeriod));
                    UserPrefs.Save();
                }
                catch
                {
                    Console.WriteLine("Invalid values for preferences");
                    response = ResponseType.Reject;                    
                }
            }
            else
            {
                Program.Log("Cancel clicked");
            }            
        }
    }
}

#endif