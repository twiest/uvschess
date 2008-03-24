
/******************************************************************************
* The MIT License
* Copyright (c) 2006 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

#if USE_GTK
using System;
using System.IO;
using System.Threading;

using System.Runtime.Remoting.Messaging;
using Gtk;
using Gdk;
using System.Collections.Generic;
using UvsChess;

namespace UvsChess.Gui
{
    public class GtkGui : Gtk.Window
    {
        #region Members
        ChessState mainChessState = null;
        GtkChessBoard chessBoardControl = null;
        TreeView _history = null;
        ListStore _store = null;
        RadioButton radWhite = null;
        RadioButton radBlack = null;
        ChessPlayer WhitePlayer = null;
        ChessPlayer BlackPlayer = null;
        bool IsRunning = false;
        ChessMove humanMove = null;
        ManualResetEvent pieceMovedEvent = new ManualResetEvent(true);
        AccelGroup mainGroup = new AccelGroup();
        Label statusLabel = null;
        EventBox eventBox = null;
        ScrolledWindow swindow = null;

        LogWindow whiteLogWindow = null;
        LogWindow blackLogWindow = null;

        //These are for the threading involved with telling the AI to EndTurn
        //int TurnWaitTime = 5000;// time in milliseconds for each turn. 
        ChessPlayer thread_player = null;
        ChessMove thread_move = null;
        ChessBoard thread_board = null;
        TimeSpan thread_time = TimeSpan.MinValue;

        public delegate void PlayCompletedHandler();
        public event PlayCompletedHandler PlayCompleted;
        protected delegate void PlayDelegate();

        List<AI> AvailableAIs = new List<AI>();

        #endregion

        #region Constructors

        public GtkGui()
            : base("UvsChess AI Framework --- http://code.google.com/p/uvschess")
        {
            // Application.Init();

            //this.Resize(1000, 700);
            this.DeleteEvent += OnUserQuit;
            this.SetPosition(WindowPosition.Center);
            //this.Maximize();
            this.Events |= EventMask.KeyPressMask;

            SearhForAIs();
            VBox vertBox = new VBox();
            this.Add(vertBox);

            this.SetAccelPath("<UvsChess>", null);
            MenuBar bar = CreateMenus();
            vertBox.PackStart(bar, false, false, 0);
            HBox horzBox = new HBox();

            mainChessState = new ChessState();
            chessBoardControl = new GtkChessBoard(mainChessState.CurrentBoard);
            //chessBoardControl.IsLocked = true;

            //chessBoardControl.OnChessPieceMoved += UpdateChessBoard;
            chessBoardControl.OnChessPieceMoved += HumanMovedPieceEvent;
            vertBox.Add(horzBox);

            //vertBox.PackStart(chessBoardControl, true, true, 0);
            VBox leftPanel = new VBox();
            horzBox.PackStart(leftPanel, true, true, 10);
            horzBox.PackEnd(chessBoardControl, false, false, 0);

            VBox statusBox = new VBox();
            statusLabel = new Label("The game is stopped.");
            Table selectors = CreateAISelectors();

            statusBox.PackStart(selectors, false, false, 5);


            //-------------------------------------------------------
            // All AI Selector controls change color when game is started

            statusBox.PackStart(statusLabel, true, true, 0);

            eventBox = new EventBox();
            eventBox.Add(statusBox);
            leftPanel.PackStart(eventBox, false, false, 0);

            //-------------------------------------------------------
            // Just the label changes color when game is started

            //eventBox.Add(statusLabel);
            //statusBox.PackStart(eventBox, true, true, 5);
            //leftPanel.PackStart(statusBox, false, false, 0);

            //-------------------------------------------------------
            CreateTreeView();
            CreateLogs();

            //ScrolledWindow swindow = new ScrolledWindow();
            swindow = new ScrolledWindow();
            swindow.Add(_history);
            leftPanel.PackStart(swindow, true, true, 0);

            WhitePlayer = new ChessPlayer(ChessColor.White);
            BlackPlayer = new ChessPlayer(ChessColor.Black);
            UserPrefs.Load();
            this.ShowAll();



        }

        #endregion

        #region Log windows
        private void CreateLogs()
        {
            this.blackLogWindow = new LogWindow(ChessColor.Black);
            this.whiteLogWindow = new LogWindow(ChessColor.White);

        }

        void toggleLogWindow(ChessColor color)
        {
            //TODO
            LogWindow window = this.whiteLogWindow;
            if (color == ChessColor.Black)
            {
                window = this.blackLogWindow;
            }

            window.Visible = !window.Visible;
            Log("Toggling " + color.ToString() + " log window: " + window.Visible.ToString());

        }
        #endregion

        #region Gui Control Creators
        //Creates the Radio buttons, drop down menus, and 'show log' buttons.
        Table CreateAISelectors()
        {
            Table main = new Table(2, 3, false);

            List<string> list = new List<string>();
            foreach (AI ai in AvailableAIs)
            {
                list.Add(ai.ShortName);
            }
            string[] AIs = list.ToArray();


            radWhite = new RadioButton("White");
            radBlack = new RadioButton(radWhite, "Black");
            radWhite.Clicked += new EventHandler(radWhite_Activated); //This event handles both check and uncheck events
            //radWhite.Activated += new EventHandler(radWhite_Activated);
            //radBlack.Activated += new EventHandler(radBlack_Activated);

            main.Attach(radWhite, 0, 1, 0, 1, AttachOptions.Shrink | AttachOptions.Fill, AttachOptions.Shrink, 5, 0);
            main.Attach(radBlack, 0, 1, 1, 2, AttachOptions.Shrink | AttachOptions.Fill, AttachOptions.Shrink, 5, 0);

            ComboBox cmbWhite = new ComboBox(AIs);
            ComboBox cmbBlack = new ComboBox(AIs);
            cmbWhite.Active = 0;//Auto select the first item in the list
            cmbBlack.Active = 0;
            cmbWhite.Changed += new EventHandler(cmbWhite_Changed);
            cmbBlack.Changed += new EventHandler(cmbBlack_Changed);

            main.Attach(cmbWhite, 1, 2, 0, 1, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Shrink, 5, 0);
            main.Attach(cmbBlack, 1, 2, 1, 2, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Shrink, 5, 0);

            //AccelGroup group = new AccelGroup();
            Button btnWhite = Button.NewWithMnemonic("Show _white log");
            btnWhite.Clicked += new EventHandler(btnLog_Activated);

            Button btnBlack = Button.NewWithMnemonic("Show _black log");
            btnBlack.Clicked += new EventHandler(btnLog_Activated);

            main.Attach(btnWhite, 2, 3, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 5, 0);
            main.Attach(btnBlack, 2, 3, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 5, 0);

            return main;
        }

        #endregion

        #region Menus
        MenuItem CreateGameMenu()
        {
            //mainGroup = new AccelGroup();
            MenuItem gameItem = new MenuItem("Game");
            //gameItem.WidthRequest = 100;

            Menu mnuGame = new Menu();
            mnuGame.SetAccelPath("<UvsChess>/Game", mainGroup);
            MenuItem new_item = new MenuItem("New");
            MenuItem start_item = new MenuItem("Start");
            MenuItem stop_item = new MenuItem("Stop");
            MenuItem preferences_item = new MenuItem("Preferences");
            new_item.WidthRequest = 100;

            mnuGame.Append(new_item);
            mnuGame.Append(start_item);
            mnuGame.Append(stop_item);
            mnuGame.Append(preferences_item);

            start_item.Activated += new EventHandler(start_item_Activated);
            stop_item.Activated += new EventHandler(stop_item_Activated);
            new_item.Activated += new EventHandler(new_item_Activated);
            preferences_item.Activated += new EventHandler(preferences_item_Activated);


            //TODO: What hot-keys should we use for this menu? Keys that are close together, I think.
            // New      Ctrl+n
            // Start    Ctrl+r
            // Stop     Ctrl+t

            AccelMap.AddEntry("<UvsChess>/Game/New", (uint)Gdk.Key.n, ModifierType.ControlMask);
            AccelMap.AddEntry("<UvsChess>/Game/Start", (uint)Gdk.Key.r, ModifierType.ControlMask);
            AccelMap.AddEntry("<UvsChess>/Game/Stop", (uint)Gdk.Key.t, ModifierType.ControlMask);
            AccelMap.AddEntry("<UvsChess>/Game/Preferences", (uint)Gdk.Key.p, ModifierType.ControlMask);

            new_item.SetAccelPath("<UvsChess>/Game/New", mainGroup);
            start_item.SetAccelPath("<UvsChess>/Game/Start", mainGroup);
            stop_item.SetAccelPath("<UvsChess>/Game/Stop", mainGroup);
            preferences_item.SetAccelPath("<UvsChess>/Game/Preferences", mainGroup);

            gameItem.Submenu = mnuGame;
            return gameItem;
        }

        MenuItem CreateFileMenu()
        {
            //mainGroup = new AccelGroup();
            MenuItem fileItem = new MenuItem("File");

            Menu mnuGame = new Menu();
            mnuGame.SetAccelPath("<UvsChess>/File", mainGroup);

            MenuItem open_item = new MenuItem("Open");
            MenuItem save_item = new MenuItem("Save");
            MenuItem exit_item = new MenuItem("Exit");
            open_item.WidthRequest = 100;
            this.AddAccelGroup(mainGroup);

            //TODO: What hot-keys should we use for this menu?
            // Open     Ctrl+o
            // Save     Ctrl+s
            // Exit     Ctrl+q

            AccelMap.AddEntry("<UvsChess>/File/Open", (uint)Gdk.Key.o, ModifierType.ControlMask);
            AccelMap.AddEntry("<UvsChess>/File/Save", (uint)Gdk.Key.s, ModifierType.ControlMask);
            AccelMap.AddEntry("<UvsChess>/File/Exit", (uint)Gdk.Key.q, ModifierType.ControlMask);


            mnuGame.Append(open_item);
            mnuGame.Append(save_item);
            mnuGame.Append(exit_item);

            open_item.Activated += new EventHandler(load_item_Activated);
            save_item.Activated += new EventHandler(save_item_Activated);
            exit_item.Activated += new EventHandler(OnUserQuit);

            open_item.SetAccelPath("<UvsChess>/File/Open", mainGroup);
            save_item.SetAccelPath("<UvsChess>/File/Save", mainGroup);
            exit_item.SetAccelPath("<UvsChess>/File/Exit", mainGroup);


            fileItem.Submenu = mnuGame;
            return fileItem;
        }

        MenuItem CreateHistoryMenu()
        {

            MenuItem fileItem = new MenuItem("History");

            Menu mnuHistory = new Menu();
            mnuHistory.SetAccelPath("<UvsChess>/History", mainGroup);

            MenuItem open_item = new MenuItem("Open History");
            MenuItem save_item = new MenuItem("Save History");
            MenuItem clear_item = new MenuItem("Clear History");
            open_item.WidthRequest = 100;
            this.AddAccelGroup(mainGroup);

            //TODO: What hot-keys should we use for this menu?
            // Open     Ctrl+o
            // Save     Ctrl+s
            // Exit     Ctrl+q

            //AccelMap.AddEntry("<UvsChess>/History/Open", (uint)Gdk.Key.o, ModifierType.ControlMask);
            //AccelMap.AddEntry("<UvsChess>/History/Save", (uint)Gdk.Key.s, ModifierType.ControlMask);


            mnuHistory.Append(open_item);
            mnuHistory.Append(save_item);
            mnuHistory.Append(clear_item);

            open_item.Activated += new EventHandler(load_history_item_Activated);
            save_item.Activated += new EventHandler(save_history_item_Activated);
            clear_item.Activated += new EventHandler(clear_history_item_Activated);

            //open_item.SetAccelPath("<UvsChess>/History/Open", mainGroup);
            //save_item.SetAccelPath("<UvsChess>/History/Save", mainGroup);


            fileItem.Submenu = mnuHistory;
            return fileItem;
        }
        MenuBar CreateMenus()
        {
            Gtk.MenuBar menuBar = new MenuBar();
            MenuItem fileMenu = CreateFileMenu();
            menuBar.Append(fileMenu);
            MenuItem gameMenu = CreateGameMenu();
            menuBar.Append(gameMenu);
            MenuItem historyMenu = CreateHistoryMenu();
            menuBar.Append(historyMenu);

            return menuBar;
        }
        #endregion

        #region History view constructor and events
        void CreateTreeView()
        {
            _history = new TreeView();

            _store = new ListStore(typeof(string), typeof(string)); // "message", "fenboard"

            _history.Model = _store;
            _history.Selection.Changed += new EventHandler(Selection_Changed);

            TreeViewColumn col = new TreeViewColumn();
            col.Title = "History";

            CellRendererText celRenderer = new CellRendererText();
            col.PackStart(celRenderer, true);
            _history.AppendColumn(col);
            col.AddAttribute(celRenderer, "text", 0);

            for (int i = 0; i < 10; ++i)
            {
                AddToHistory("test message " + i);
            }

        }

        /// <summary>
        /// This event happens if the user scrolls through the history with the arrow keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Selection_Changed(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            TreeSelection sel = (TreeSelection)sender;
            //Log("Selection_Changed");

            TreeIter iter;
            if (sel.GetSelected(out iter))
            {
                //Log(iter.ToString());
                _history_Select_Row(iter);
            }
        }


        /// <summary>
        /// Event that fires when player clicks in history
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        //void _history_RowActivated(object o, RowActivatedArgs args)
        //{
        //    if (!IsRunning)
        //    {
        //        TreeIter iter;
        //        if (_store.GetIter(out iter, args.Path))
        //        {
        //            _history_Select_Row(iter);
        //        }
        //    }
        //}

        void _history_Select_Row(TreeIter iter)
        {
            object value = _store.GetValue(iter, 1);
            if (value != null)
            {
                //Detect if value is a fen board...

                string fenboard = (string)value;
                if (fenboard.Split('/').Length >= 7)
                {
                    ResetBoard((string)value);
                }
            }
        }

        #endregion

        #region GUI Control events
        void cmbBlack_Changed(object sender, EventArgs e)
        {
            setPlayersAI(BlackPlayer, ((ComboBox)sender).ActiveText);
        }

        void cmbWhite_Changed(object sender, EventArgs e)
        {
            setPlayersAI(WhitePlayer, ((ComboBox)sender).ActiveText);
        }

        void setPlayersAI(ChessPlayer player, string newAI)
        {
            if (player.AIName != newAI)//Set to a different AI
            {
                player.AIName = newAI;
                player.AI = null;//AI has changed, so set the AI to null.
            }
        }

        //void radBlack_Activated(object sender, EventArgs e)
        //{
        //    mainChessState.CurrentPlayerColor = ChessColor.Black;
        //}

        void radWhite_Activated(object sender, EventArgs e)
        {
            if (((Gtk.RadioButton)sender).Active)
            {
                mainChessState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                mainChessState.CurrentPlayerColor = ChessColor.Black;
            }
        }

        void btnLog_Activated(object sender, EventArgs e)
        {
            Button btn = (Gtk.Button)sender;
            if (btn.Label.ToLower().Contains("white"))
            {
                toggleLogWindow(ChessColor.White);
            }
            else if (btn.Label.ToLower().Contains("black"))
            {
                toggleLogWindow(ChessColor.Black);
            }
            else
            {
                showErrorMsg("Can't find chess color");
            }

        }



        #endregion

        #region Menu handlers

        #region Game menu
        void new_item_Activated(object sender, EventArgs e)
        {
            stop_item_Activated(null, null);
            clear_history_item_Activated(null, null);
            Log("New game...");
            ResetBoard(ChessState.StartState);
        }

        void stop_item_Activated(object sender, EventArgs e)
        {
            Log("Stopping game...");
            statusLabel.Text = "Game has been stopped.";
            eventBox.ModifyBg(StateType.Normal);
            chessBoardControl.IsLocked = false;
            IsRunning = false;
        }

        void start_item_Activated(object sender, EventArgs e)
        {
            Log("Starting game...");
            Color c = new Color(0xff, 0x00, 0x00);

            statusLabel.Text = "Game is running.";
            eventBox.ModifyBg(StateType.Normal, c);
            StartGame();
        }

        void preferences_item_Activated(object sender, EventArgs e)
        {
            PreferencesGUI prefs = new PreferencesGUI();
            //prefs.Show();


            int result = prefs.Run();


            prefs.Destroy();
        }
        #endregion

        #region File menu
        void save_item_Activated(object sender, EventArgs e)
        {

            Log("Saving game...");
            FileChooserDialog dialog = new FileChooserDialog("Save game", this, FileChooserAction.Save);

            dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            dialog.AddButton(Stock.Save, ResponseType.Ok);

            FileFilter filter = new FileFilter();
            filter.AddPattern("*.fen");
            filter.Name = "*.fen";
            dialog.AddFilter(filter);

            int result = dialog.Run();
            if ((ResponseType)result == ResponseType.Ok)
            {
                string filename = dialog.Filename;
                Log("Saving file: " + filename);
                StreamWriter outfile = new StreamWriter(filename);

                string fenboard = mainChessState.ToFenBoard();
                outfile.WriteLine(fenboard);
                outfile.Close();
            }

            dialog.Destroy();

        }

        void load_item_Activated(object sender, EventArgs e)
        {

            Log("Loading game...");
            FileChooserDialog dialog = new FileChooserDialog("Load game", this, FileChooserAction.Open);

            dialog.AddButton(Stock.Open, ResponseType.Ok);
            dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            FileFilter filter = new FileFilter();
            filter.AddPattern("*.fen");
            filter.Name = "*.fen";
            dialog.AddFilter(filter);

            int result = dialog.Run();

            if ((ResponseType)result == ResponseType.Ok)
            {
                string filename = dialog.Filename;
                Log("Opening file: " + filename);

                StreamReader infile = new StreamReader(filename);
                string fenboard = infile.ReadLine();

                Log("FEN board: " + fenboard);

                ResetBoard(fenboard);
                infile.Close();
            }
            dialog.Destroy();
        }
        #endregion

        #region History menu
        void load_history_item_Activated(object sender, EventArgs e)
        {
            Log("Loading history");
        }

        void save_history_item_Activated(object sender, EventArgs e)
        {
            Log("Saving history");
            FileChooserDialog dialog = new FileChooserDialog("Save history", this, FileChooserAction.Save);

            dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            dialog.AddButton(Stock.Save, ResponseType.Ok);

            //            FileFilter filter = new FileFilter();
            //            filter.AddPattern("*.fen");
            //            filter.Name = "*.fen";
            //            dialog.AddFilter(filter);

            int result = dialog.Run();
            if ((ResponseType)result == ResponseType.Ok)
            {
                string filename = dialog.Filename;
                Log("Saving history to " + filename);
                StreamWriter outfile = new StreamWriter(filename);

                TreeIter iter = new TreeIter();
                if (_store.GetIterFirst(out iter))
                    do
                    {
                        string line = (string)_store.GetValue(iter, 0);
                        string fenboard = (string)_store.GetValue(iter, 1);

                        line += ":" + fenboard;

                        Log(line);
                        outfile.WriteLine(line);

                    }
                    while (_store.IterNext(ref iter));
                outfile.Close();
            }
            dialog.Destroy();

        }

        void clear_history_item_Activated(object sender, EventArgs e)
        {
            Log("Clearing history");
            ClearHistory();
        }

        #endregion

        #endregion

        #region Game play methods and events
        void Play()
        {
            //This method run in its own thread.

            //Load the AI if it isn't loaded already
            LoadAI(WhitePlayer);
            LoadAI(BlackPlayer);

            IsRunning = true;
            //ChessMove currentMove = null;
            chessBoardControl.IsLocked = true;

            while (IsRunning)
            {
                if (mainChessState.CurrentPlayerColor == ChessColor.White)
                {
                    DoNextMove(WhitePlayer, BlackPlayer);
                }
                else
                {
                    DoNextMove(BlackPlayer, WhitePlayer);
                }
            }

            Log("Game Over");
            IsRunning = false; //This is redundant, but it makes the code clear
            chessBoardControl.IsLocked = false;

        }

        void DoNextMove(ChessPlayer player, ChessPlayer opponent)
        {

            ChessMove nextMove = null;
            //DateTime start = DateTime.Now;
            bool isValidMove = false;
            ChessState newstate = null;


            if (player.IsComputer)
            {
                nextMove = GetNextAIMove(player, mainChessState.CurrentBoard.Clone());

                newstate = mainChessState.Clone();
                newstate.MakeMove(nextMove);
                if (opponent.IsComputer)
                {
                    isValidMove = opponent.AI.IsValidMove(newstate);
                }
                else
                {
                    isValidMove = true;
                }

                //isValidMove = isValidMove && !isOverTime(player, thread_time, TurnWaitTime);
                //isValidMove = isValidMove && !isOverTime(player, thread_time, PreferencesGUI.TurnLength);
                isValidMove = isValidMove && !isOverTime(player, thread_time, UserPrefs.Time);

            }
            else //player is human
            {
                while (!isValidMove)
                {
                    chessBoardControl.IsLocked = false;

                    nextMove = GetNextHumanMove();

                    chessBoardControl.IsLocked = true;

                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);
                    isValidMove = opponent.AI.IsValidMove(newstate);
                }
            }

            if (isValidMove)//update the board
            {
                chessBoardControl.ResetBoard(newstate.CurrentBoard);
            }
            else //or reset the board
            {
                chessBoardControl.ResetBoard(mainChessState.CurrentBoard);
            }


            AddToHistory(player.Color.ToString() + ": " + nextMove.ToString(), newstate.ToFenBoard());

            if (isValidMove)
            {

                //update mainChessState for valid 
                mainChessState = newstate;

                if (player.Color == ChessColor.Black)
                {
                    mainChessState.FullMoves++;//Increment fullmoves after black's turn
                }

                //Determine if a pawn was moved or a kill was made.
                if (ResetHalfMove())
                {
                    mainChessState.HalfMoves = 0;
                }
                else
                {
                    mainChessState.HalfMoves++;
                }

                Log(mainChessState.ToFenBoard());

            }
            else
            {
                IsRunning = false;
                Log(String.Format("Invalid move returned, {0} loses!", (player.Color == ChessColor.Black) ? "Black" : "White"));

            }

            //chessBoardControl.OnExposeEvent();

            //GLib.Timeout.Add(100,G

            //Selection_Changed(null, null);

            //GLib.
            //swindow.Activate();
            //this.OnExposeEvent(null);  
            //_store.EmitRowChanged(null, null);
            //TreeIter iter;
            //_store.GetIterFirst(out iter);
            //TreePath path = _store.GetPath(iter);
            //_store.EmitRowChanged(path, iter);
            //_history_Select_Row(iter);
        }


        //This method checks if a pawn was moved or a kill was made.
        private bool ResetHalfMove()
        {
            ChessMove move = mainChessState.PreviousMove;
            //Check for a pawn move
            ChessPiece piece = mainChessState.PreviousBoard[move.From.Row, move.From.Column];
            if ((piece == ChessPiece.WhitePawn) || (piece == ChessPiece.BlackPawn))
            {
                return true;
            }

            //Check for a kill
            piece = mainChessState.PreviousBoard[move.To.Row, move.To.Column];
            if (piece != ChessPiece.Empty)
            {
                return true;
            }

            return false;
        }

        #region Threading stuff

        public IAsyncResult StartGame()
        {
            //TODO: disable radio buttons
            PlayDelegate pd = new PlayDelegate(Play); //Start a new thread from this method
            return pd.BeginInvoke(new AsyncCallback(EndPlay), null);
        }

        private void EndPlay(IAsyncResult ar)
        {
            try
            {
                AsyncResult result = (AsyncResult)ar;
                PlayDelegate pd = (PlayDelegate)result.AsyncDelegate;
                pd.EndInvoke(ar);
                OnPlayCompleted();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chess->MainForm->EndPlay: " + ex.Message);
            }
        }
        protected void OnPlayCompleted()
        {
            if (PlayCompleted != null)
            {
                PlayCompleted();
            }
        }

        #endregion

        ChessMove GetNextHumanMove()
        {
            pieceMovedEvent.Reset();
            pieceMovedEvent.WaitOne();

            return humanMove;
        }

        ChessMove GetNextAIMove(ChessPlayer player, ChessBoard board)
        {
            thread_player = player;
            thread_board = board;

            //Add threading here. Wait n seconds then call ChessAI.EndTurn()
            ThreadStart job = new ThreadStart(threadedNextMove);
            Thread thread = new Thread(job);

            DateTime startTime = DateTime.Now;
            thread.Start();

            //Thread.Sleep(TurnWaitTime);//Time of turn
            Thread.Sleep(UserPrefs.Time);

            player.AI.EndTurn();
            thread.Join();

            thread_time = DateTime.Now - startTime;
            thread_time = DateTime.Now.Subtract(startTime);

            return thread_move;
        }
        private void threadedNextMove()
        {
            ChessPlayer player = thread_player;
            ChessMove nextMove = player.AI.GetNextMove(thread_board, player.Color);

            thread_move = nextMove;

        }

        private bool isOverTime(ChessPlayer player, TimeSpan time, int limit)
        {
            bool isovertime = false;
            Console.WriteLine("{0} stopped after {1:0} ms", player.AIName, time.TotalMilliseconds);

            //do we need/want a buffer ?
            limit += 1000;  //time buffer

            if ((time.TotalMilliseconds > (double)limit) && (player.IsComputer))
            {
                isovertime = true;
                Log("Too Much Time: Move timeout occurred!");
            }
            return isovertime;
        }

        void HumanMovedPieceEvent(ChessMove move)
        {

            if (IsRunning)
            {
                Log("Human move:");
                humanMove = move;
                pieceMovedEvent.Set();
            }
            else
            {
                Log("Pregame setup:");
                mainChessState.CurrentBoard.MakeMove(move);
                Log(mainChessState.CurrentBoard.ToFenBoard());
            }

        }

        void ResetBoard(string fenboard)
        {
            // ResetBoard is called from Load Game, New Game, and History review
            mainChessState.FromFenBoard(fenboard);

            if (mainChessState.CurrentPlayerColor == ChessColor.White)
            {
                radWhite.Active = true;
            }
            else
            {
                radBlack.Active = true;
            }

            chessBoardControl.ResetBoard(mainChessState.CurrentBoard);

            //_store.Clear();

            Log("Reseting board");
            Log("Current Player: " + mainChessState.CurrentPlayerColor);
            Log("HalfMoves: " + mainChessState.HalfMoves);
            Log("FullMove: " + mainChessState.FullMoves);

        }

        #endregion

        #region Loading DLLs
        void SearhForAIs()
        {
            AvailableAIs.Add(new AI("Human"));
            //AvailableAIs.Add("Human");
            string appFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string[] dlls = Directory.GetFiles(appFolder, "*.dll");

            foreach (string dll in dlls)
            {
                LoadAIsFromFile(dll);
            }
        }

        void LoadAIsFromFile(string filename)
        {
            try
            {
                System.Reflection.Assembly assem = System.Reflection.Assembly.LoadFile(filename);
                System.Type[] types = assem.GetTypes();

                foreach (System.Type type in types)
                {
                    System.Type[] interfaces = type.GetInterfaces();

                    foreach (System.Type inter in interfaces)
                    {
                        if (inter == typeof(UvsChess.IChessAI))
                        {
                            IChessAI ai = (IChessAI)assem.CreateInstance(type.FullName);
                            AI tmp = new AI(ai.Name);
                            tmp.FileName = filename;
                            tmp.FullName = type.FullName;
                            AvailableAIs.Add(tmp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chess->MainForm->LoadAI: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads the AI for the chess player. If the AI is already loaded, it will just return.
        /// </summary>
        /// <param name="player"></param>
        void LoadAI(ChessPlayer player)
        {
            if ((player.AIName == null) || (player.AIName == "Human"))
            {
                return;
            }
            else if (player.AI != null)
            {
                return;
            }

            AI tmpAI = null;

            foreach (AI t in AvailableAIs)
            {
                if (t.ShortName == player.AIName)
                {
                    tmpAI = t;
                    break;
                }
            }
            System.Reflection.Assembly assem = System.Reflection.Assembly.LoadFile(tmpAI.FileName);
            IChessAI ai = (IChessAI)assem.CreateInstance(tmpAI.FullName);
            player.AI = ai;

        }

        #endregion

        #region Helpers

        void OnUserQuit(object o, EventArgs args)
        {
            Application.Quit();
        }

        public void AddToHistory(string message)
        {
            AddToHistory(message, string.Empty);
        }

        public void AddToHistory(string message, string fenboard)
        {

            Gtk.Application.Invoke(delegate
            {
                TreeIter iter = _store.AppendValues(message, fenboard);

                //scroll history down to latest entry
                TreePath path = _store.GetPath(iter);
                _history.ScrollToCell(path, null, false, 0.0f, 0.0f);
            });
        }

        public void ClearHistory()
        {

            Gtk.Application.Invoke(delegate
            {
                _store.Clear();
            });
        }

        void showErrorMsg(string message)
        {
            MessageDialog md = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, message);
            md.Run();
            md.Destroy();
        }

        #endregion
    }

}
#endif