/*
Created by SharpDevelop.
 * Thorsten Lorenz
 * Date: 1/24/2009
 * Time: 12:46 PM
 */
namespace PokerTell.Plugins.PlayerPeeker
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    using Infrastructure.Interfaces;

    using Interfaces;

    using log4net;

    /// <summary>
    /// This queries the PlayerPeek Statistics for a list of players.
    /// The solution to navigate to the webpage via the webbrowser including loading all the ads etc.
    /// is very slow, but is the only thing that works.
    /// I was unable to figure out how to login using the HttpRequest version.
    /// This version works like this:
    /// We initiate the adding of players and point the webbrowser to the submit string and wait for
    /// it to fire wbb_Completed.
    /// Once it loaded the final stats page it adds the player's stats and starts the webbrowser again
    /// this time to add the next player.
    /// This is done until all players have been added.
    /// </summary>
    public partial class PlayerPeekerForm : Form, IPlayerPeekerForm
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly List<string> _playersWithoutStats;

        readonly Dictionary<string, PeekerStats> _queriedStats;

        readonly ISettings _settings;

        readonly WebBrowser _wbb;

        int _currentIndex;

        IEnumerable<string> _newFoundPlayers;

        public PlayerPeekerForm(ISettings settings)
        {
            _settings = settings;
            InitializeComponent();
            _playersWithoutStats = new List<string>();
            _queriedStats = new Dictionary<string, PeekerStats>();

            _wbb = new WebBrowser { ScriptErrorsSuppressed = true };
            _wbb.DocumentCompleted += WebBrowser_DocumentCompleted;
           
            PersistState();
        }

        public void ActivateAndRestore()
        {
            Activate();
            WindowState = FormWindowState.Normal;
            PersistState();
        }

        public event Action ResetRequested;

        enum StateProperties
        {
            PlayerPeekForm, 
            Location, 
            Size
        }

        public void NewPlayersFound(IEnumerable<string> newPlayers)
        {
            _newFoundPlayers = newPlayers;
            _currentIndex = 0;

            // Initiate Progressbar
            lblStatus.Text = "Retrieving ...";

            // Start adding new Players
            QueryNextPlayer(_currentIndex);
        }

        static bool DataGridViewContains(DataGridView dgv, string strValue, int column)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells[column].Value != null && row.Cells[column].Value.ToString().Equals(strValue))
                {
                    return true;
                }
            }

            return false;
        }

        static string GetSubmitHtml(string playerName)
        {
            const string pokerSite = "pokerstars";

            return "<BODY onLoad = \"document.Find.submit()\">" +
                   "<form name=\"Find\" action=http://www.playerpeek.com/players.html method=post " +
                   "enctype=\"application/x-www-form-urlencoded\">" + "Name: " +
                   "<INPUT TYPE=\"text\" NAME=\"player\" VALUE=\"" + playerName + "\">" + "Site:" +
                   "<INPUT TYPE=\"text\" NAME=\"site\" VALUE=\"" + pokerSite + "\">" +
                   "<INPUT TYPE=\"submit\" ACTION=\"http://www.playerpeek.com/players.html\" VALUE=\"Search\" METHOD=\"get\"" +
                   "id=\"submit_single\"><o:p></o:p></span></p>" + "</form>";
        }

        static void LoginToPlayerPeekCom()
        {
            var frmLogin = new Form { Size = new Size(800, 600) };
            var loginBrowser = new WebBrowser { Dock = DockStyle.Fill };
            frmLogin.Controls.Add(loginBrowser);
            frmLogin.Show();
            loginBrowser.Navigate("http://www.playerpeek.com/");
        }

        void AddStatsToGrid(PeekerStats stats)
        {
            dgvStats.Rows.Add(
                stats.Name.ToString(), 
                stats.GamesPlayed.ToString(), 
                stats.Profit.ToString(), 
                stats.AvgProfit.ToString(), 
                stats.PercROI.ToString(), 
                stats.AvgBuyin.ToString(), 
                stats.PercITM.ToString(), 
                stats.PercFinalTables.ToString(), 
                stats.PercWins.ToString(), 
                stats.AvgFinish.ToString(), 
                stats.AvgEntrants.ToString());
        }

        void FrmPlayerPeekFormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
            {
                SaveState();
                WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }

            e.Cancel = true;
        }

        void FrmPlayerPeekSizeChanged(object sender, EventArgs e)
        {
            dgvStats.Height = Height - 47;
        }

        void InvokeResetRequested()
        {
            Action requested = ResetRequested;
            if (requested != null)
            {
                requested();
            }
        }

        void LoginToolStripMenuItemClick(object sender, EventArgs e)
        {
            LoginToPlayerPeekCom();
        }

        void PersistState()
        {
            string strPrefix = StateProperties.PlayerPeekForm + ".";

            // Get the bounds of the screen in which the Form is currently and only 
            // Apply loaded location only if it is within those bounds
            Point loadLocation = _settings.RetrievePoint(strPrefix + StateProperties.Location);
            if (ThisPointIsOnOneOfTheConnectedScreens(loadLocation))
            {
                Location = loadLocation;
            }
            else
            {
                Log.Debug(StateProperties.PlayerPeekForm + " Location in settings was off bounds");
            }

            Size = _settings.RetrieveSize(strPrefix + StateProperties.Size, Size);
        }

        static bool ThisPointIsOnOneOfTheConnectedScreens(Point thePoint)
        {
            bool foundScreenThatContainsThePoint = false;

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i].Bounds.Contains(thePoint))
                    foundScreenThatContainsThePoint = true;
            }

            return foundScreenThatContainsThePoint;
        }

        void QueryNextPlayer(int index)
        {
            try
            {
                if (index < _newFoundPlayers.Count())
                {
                    if (! _playersWithoutStats.Contains(_newFoundPlayers.ElementAt(index)) &&
                        ! DataGridViewContains(dgvStats, _newFoundPlayers.ElementAt(index), 0))
                    {
                        // Have we queried this player before (e.g. he was at our table but we got moved and
                        // now he got also moved and is at our table again
                        if (_queriedStats.ContainsKey(_newFoundPlayers.ElementAt(index)))
                        {
                            AddStatsToGrid(_queriedStats[_newFoundPlayers.ElementAt(index)]);

                            // Query the next Player
                            QueryNextPlayer(++_currentIndex);
                        }
                        else
                        {
                            _wbb.DocumentText = GetSubmitHtml(_newFoundPlayers.ElementAt(index));

                            // Don't query next player right away but wait instead until WebBrowser Navigate completes and
                            // automatically will query next player
                        }
                    }
                    else
                    {
                        // We didn't query this player because it was either in the Datagrid or has no stats
                        // Query the next Player
                        QueryNextPlayer(++_currentIndex);
                    }
                }
                else
                {
                    lblStatus.Text = "Ready";
                    dgvStats.Sort(dgvStats.Columns[0], ListSortDirection.Ascending);
                }
            }
            catch (Exception excep)
            {
                Log.Error("Unexpected", excep);
            }
        }

        void ResetToolStripMenuItemClick(object sender, EventArgs e)
        {
            InvokeResetRequested();
        }

        void SaveState()
        {
            string strPrefix = StateProperties.PlayerPeekForm + ".";
            _settings.Set(strPrefix + StateProperties.Location, Location);
            _settings.Set(strPrefix + StateProperties.Size, Size);
        }

        void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            PeekerStats stats;
            var sndWbb = (WebBrowser)sender;
            string strElement;

            try
            {
                Application.DoEvents();

                // Did we get to results page?
                if (e.Url.Host.Contains("playerpeek"))
                {
                    // Did we find any stats for the player?
                    if (e.Url.AbsolutePath.Contains("pokerstars"))
                    {
                        strElement = string.Empty;

                        // Get Html Text for parsing
                        foreach (HtmlElement iHtml in sndWbb.Document.GetElementsByTagName("HTML"))
                        {
                            strElement = iHtml.InnerText;
                        }

                        // Parse Data and put into datagrid
                        stats = new PeekerStats(strElement);

                        // Make sure we didn't add it before, althoug we should never get here
                        // if it is contained already  (see: QueryNextPlayer)
                        if (_queriedStats.ContainsKey(stats.Name.Value))
                        {
                            _queriedStats[stats.Name.Value] = stats;
                        }
                        else
                        {
                            _queriedStats.Add(stats.Name.Value, stats);
                        }

                        AddStatsToGrid(stats);
                    }
                    else
                    {
                        _playersWithoutStats.Add(_newFoundPlayers.ElementAt(_currentIndex));
                    }

                    // Initiate next player search
                    QueryNextPlayer(++_currentIndex);
                }
            }
            catch (Exception excep)
            {
                Log.Error("Unexpected", excep);
            }
        }
    }
}