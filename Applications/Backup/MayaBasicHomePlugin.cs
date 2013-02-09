using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Action = MediaPortal.GUI.Library.Action;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.Util;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Cornerstone.Tools;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.MainUI;
using WindowPlugins.GUITVSeries;
using TVSeriesHelper = WindowPlugins.GUITVSeries.Helper;



namespace ProcessPlugins.MayaBasicHomePlugin
{
  [PluginIcons("MayaBasicHomePlugin.Plugin.png", "MayaBasicHomePlugin.PluginDisabled.png")]
  public class MayaBasicHomePlugin : IPlugin, ISetupForm
    {

        #region declarations
        int mypointer = 0;
        int myprevimage = 0;
        int numberofitems = 0;
        int sel_image = 0;
        int topitem1;
        int topitem2;
        int topitem3;
        int bottomitem1;
        int bottomitem2;
        int bottomitem3;
        string myskin = "";
        string mousecontrol = "";
        List<string> ms_prefixes = new List<string>();
        List<string> ms_headings = new List<string>();
        List<int> ms_windowids = new List<int>();
        List<int> ms_subids = new List<int>();
        List<string> ms_window_params = new List<string>();
        List<string> ms_sub_params = new List<string>();
        string _hyperLinkParameter = "";
        List<string> ms_bgimages = new List<string>();
        List<string> ms_bgpaths = new List<string>();
        private ArrayList Children = new ArrayList();
        private ArrayList subChildren = new ArrayList();
        bool hasSetNeighbours = false;
        bool pauseMainMenu = false;
        string selectedSubItem = "35";
        int selectedID = 0;
        int location = 0;
        Timer mytimer = new Timer();
        public string[] mostTVSeriesRecents = new string[3];
        public string[] mostMovPicsRecents = new string[3];


        private string _savedLocation = string.Empty;
        private string _locationCode = "UKXX0085";
        private bool isweatherloaded;
        private string _urlSatellite = string.Empty;
        private string _urlTemperature = string.Empty;
        private string _urlUvIndex = string.Empty;
        private string _urlWinds = string.Empty;
        private string _urlHumidity = string.Empty;
        private string _urlPreciptation = string.Empty;
        private string _urlViewImage = string.Empty;
        private ArrayList _listLocations = new ArrayList();
        string imagePath = string.Empty;
        private ImageView _imageView = ImageView.Satellite;

        #endregion



        private enum ImageView
        {
            Satellite = 0,
            Temperature = 1,
            UVIndex = 2,
            Winds = 3,
            Humidity = 4,
            Precipitation = 5,
        }

        private class LocationInfo
        {
          public string City;
          public string CityCode;
          public string UrlSattelite;
          public string UrlTemperature;
          public string UrlUvIndex;
          public string UrlWinds;
          public string UrlHumidity;
          public string UrlPrecip;
        }





        public class Child
        {
            public int Owner;
            public int ID;
            public string Name;
            public int Hyperlink;
            public string Parameter;

            public Child(int intOwner, int strID, string strName, int intHyperlink, string strParam)
            {
                this.Owner = intOwner;
                this.ID = strID;
                this.Name = strName;
                this.Hyperlink = intHyperlink;
                this.Parameter = strParam;
            }

            public override string ToString()
            {
                return this.ID + " : " + this.Name;
            }

        }

        private void SetProperty(string property, string value)
        {
            if (property == null)
                return;
            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = " ";

            GUIPropertyManager.SetProperty(property, value);


        }

        private void SetBlankProperty(string property, string value)
        {
            if (property == null)
                return;
            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = "";

            GUIPropertyManager.SetProperty(property, value);
        }



        private void SetSubmenu(int owner)
        {
            GUIControl.HideControl(35, 566);
            //Log.Info("hidectrl 1");

            for (int i = 0; i < 6; i++)
            {
                SetProperty("#subitem" + i, "");
            }


            int a = 0;
            subChildren.Clear();
            ms_subids.Clear();
            ms_sub_params.Clear();
            foreach (Child mychild in Children)
            {
                if (mychild.Owner == owner)
                {
                    subChildren.Add(mychild);
                    ms_subids.Add(mychild.Hyperlink);
                    ms_sub_params.Add(mychild.Parameter);
                    a++;


                    //GUIControl.SetControlLabel(35, (500 + a), mychild.Name);
                    //GUIControl.SetControlLabel(35, (600 + a), mychild.Name);

                    SetProperty("#subitem" + a, mychild.Name);
                }

            }

            if (a > 0)
            {
                SetProperty("#isarrowvisible", "yes");
                GUIControl.ShowControl(35, 566);
                //Log.Info("showctrl");
            }
            else
            {
                SetProperty("#isarrowvisible", "no");
                GUIControl.HideControl(35, 566);
                //Log.Info("hidectrl 2");
            }
        }


        private void SetNeighbours(int mypointer, int numberofitems)
        {

            //SetProperty("#items", "mypointer:" + mypointer + " number:" + numberofitems);


            if (ms_headings.Count > 0)
            {
                if (mypointer - 2 > 0)
                {
                    topitem3 = mypointer - 3;
                    topitem2 = mypointer - 2;
                    topitem1 = mypointer - 1;
                }

                else if (mypointer - 1 > 0)
                {
                    topitem3 = numberofitems;
                    topitem2 = mypointer - 2;
                    topitem1 = mypointer - 1;
                }
                else if (mypointer > 0)
                {
                    topitem3 = numberofitems - 1;
                    topitem2 = numberofitems;
                    topitem1 = mypointer - 1;
                }
                else if (mypointer - 1 == 0)
                {
                    topitem3 = numberofitems - 1;
                    topitem2 = numberofitems;
                    topitem1 = 0;
                }
                else if (mypointer == 0)
                {
                    topitem3 = numberofitems - 2;
                    topitem2 = numberofitems - 1;
                    topitem1 = numberofitems;
                }







                if (mypointer + 1 > numberofitems)
                {
                    bottomitem1 = 0;
                }
                else
                {
                    bottomitem1 = mypointer + 1;
                }




                if (mypointer == numberofitems)
                {
                    bottomitem2 = 1;
                }

                else if (mypointer + 1 == numberofitems)
                {
                    bottomitem2 = 0;
                }

                else if (mypointer + 1 > numberofitems)
                {
                    bottomitem2 = 1;
                }

                else
                {
                    bottomitem2 = mypointer + 2;
                }



                if (mypointer == numberofitems)
                {
                    bottomitem3 = 2;
                }
                else if (mypointer + 2 == numberofitems)
                {
                    bottomitem3 = 0;
                }
                else if (mypointer + 1 == numberofitems)
                {
                    bottomitem3 = 1;
                }
                else
                {
                    bottomitem3 = mypointer + 3;
                }

                if (ms_headings[topitem3] != null)
                {
                    SetProperty("#topitem3", ms_headings[topitem3]);
                }
                if (ms_headings[topitem2] != null)
                {
                    SetProperty("#topitem2", ms_headings[topitem2]);
                }
                if (ms_headings[topitem1] != null)
                {
                    SetProperty("#topitem1", ms_headings[topitem1]);
                }
                if (ms_headings[bottomitem1] != null)
                {
                    SetProperty("#bottomitem1", ms_headings[bottomitem1]);
                }
                if (ms_headings[bottomitem2] != null)
                {
                    SetProperty("#bottomitem2", ms_headings[bottomitem2]);
                }
                if (hasSetNeighbours == true && ms_headings[bottomitem3] != null)
                {
                    SetProperty("#bottomitem3", ms_headings[bottomitem3]);
                }
                else
                {
                    SetProperty("#bottomitem3", " ");
                }
                hasSetNeighbours = true;
            }

        }



        private void noItems(string myMessage)
        {
            GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            if (myMessage != "")
            {
                dlg.SetHeading(myMessage);
            }
            else
            {
                dlg.SetHeading("No menuitems configured");
            }
            dlg.SetLine(1, "Maya found no menuitems,");
            dlg.SetLine(2, "please run the config-plugin");
            dlg.SetLine(3, "to generate your BasicHome menu.");
            dlg.DoModal(GUIWindowManager.ActiveWindow);

        }




        private void LoadWeatherSettings()
        {
            _listLocations.Clear();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                _locationCode = xmlreader.GetValueAsString("weather", "location", string.Empty);
                bool bFound = false;

                for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
                {
                    string cityTag = String.Format("city{0}", i);
                    string strCodeTag = String.Format("code{0}", i);
                    string strSatUrlTag = String.Format("sat{0}", i);
                    string strTempUrlTag = String.Format("temp{0}", i);
                    string strUVUrlTag = String.Format("uv{0}", i);
                    string strWindsUrlTag = String.Format("winds{0}", i);
                    string strHumidUrlTag = String.Format("humid{0}", i);
                    string strPrecipUrlTag = String.Format("precip{0}", i);
                    string city = xmlreader.GetValueAsString("weather", cityTag, string.Empty);
                    string strCode = xmlreader.GetValueAsString("weather", strCodeTag, string.Empty);
                    _urlSatellite = xmlreader.GetValueAsString("weather", strSatUrlTag, string.Empty);
                    _urlTemperature = xmlreader.GetValueAsString("weather", strTempUrlTag, string.Empty);
                    _urlUvIndex = xmlreader.GetValueAsString("weather", strUVUrlTag, string.Empty);
                    _urlWinds = xmlreader.GetValueAsString("weather", strWindsUrlTag, string.Empty);
                    _urlHumidity = xmlreader.GetValueAsString("weather", strHumidUrlTag, string.Empty);
                    _urlPreciptation = xmlreader.GetValueAsString("weather", strPrecipUrlTag, string.Empty);
                    if (city.Length > 0 && strCode.Length > 0)
                    {
                        if (_urlSatellite.Length == 0)
                        {
                            //strSatURL = "http://www.heute.de/CMO/frontend/subsystem_we/WeShowPicture/0,6008,161,00.gif";
                            _urlSatellite = @"http://images.intellicast.com/WeatherImg/SatelliteLoop/hieusat_None_anim.gif";
                        }

                        LocationInfo loc = new LocationInfo();
                        loc.City = city;
                        loc.CityCode = strCode;
                        loc.UrlSattelite = _urlSatellite;
                        loc.UrlTemperature = _urlTemperature;
                        loc.UrlUvIndex = _urlUvIndex;
                        loc.UrlWinds = _urlWinds;
                        loc.UrlHumidity = _urlHumidity;
                        loc.UrlPrecip = _urlPreciptation;
                        _listLocations.Add(loc);

                        if (String.Compare(_locationCode, strCode, true) == 0)
                        {
                            bFound = true;
                        }
                    }
                }

                if (!bFound)
                {
                    if (_listLocations.Count > 0)
                    {
                        _locationCode = ((LocationInfo)_listLocations[0]).CityCode;
                    }
                }
            }
        }



        private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
        {
            //Log.Info("Maya: tag=" + tag + ", tagValue=" + tagValue);

            if (tag == "#currentmoduleid" && tagValue.IndexOf("35") != -1)
            {
                SetSubmenu(mypointer);
            }
            else if (tag == "#highlightedbutton" && tagValue.IndexOf("#subitem") != -1)
            {
                bool test1 = String.Compare(tagValue.Substring(0, 8), "#subitem") == 0; // This is true.
                selectedSubItem = tagValue.Substring(8);
                int numVal = Convert.ToInt32(selectedSubItem) - 1;

                selectedID = Convert.ToInt32(tagValue.Substring(8)) - 1;
                SetProperty("#highlightedbutton", subChildren[numVal].ToString().Substring(4));
                //Log.Info("Maya : bool=" + test1 + " selected = " + selectedSubItem);

                //Log.Info("Maya: paus hovedmenu");
                pauseMainMenu = true;
            }
            else if (tag == "#homeitem")
            {
                //Log.Info("Maya: genstart hovedmenu");
                pauseMainMenu = false;
            }
            else if (tag == "#highlightedbutton" && tagValue.ToString() == "")
            {
                //Log.Info("Maya: genstart hovedmenu");
                pauseMainMenu = false;

            }


            if (GUIWindowManager.ActiveWindow == 35)
            {

                location = (int)ms_windowids[mypointer];
                if (location == 9811) //Series
                {

                    GUIControl.ShowControl(35, 123);
                }
                else
                    GUIControl.HideControl(35, 123);

                if (location == 96742) //MovingPictures
                {
                    GUIControl.ShowControl(35, 119);
                }
                else
                    GUIControl.HideControl(35, 119);

                if (location == 1) //Recordings
                {
                    GUIControl.ShowControl(35, 118);
                }
                else
                    GUIControl.HideControl(35, 118);

                if (location == 501) //Music
                {
                    GUIControl.ShowControl(35, 117);
                }
                else
                    GUIControl.HideControl(35, 117);

            }
            else
                return;

            }
        







        public void OnTimerEvent(object source, EventArgs e)
        {
            GUIControl.FocusControl(2600, 4);
            mytimer.Enabled = false;





            if (!isweatherloaded)
            {
                LoadWeatherSettings();
                isweatherloaded = true;
            }

            GUIWindow pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);


            GUIImage _curDay = pWindow.GetControl(21) as GUIImage;
            if (_curDay != null && _curDay.IsVisible)
            {
                string myDay = _curDay.FileName;
                myDay = myDay.Replace(".png", "");
                Log.Info("dag: " + myDay);
                string[] icon = myDay.Split('\\');
                GUIPropertyManager.SetProperty("#maya.weather.background", icon[icon.Length-1]);
             }


            GUIImage _urlImage = pWindow.GetControl(1000) as GUIImage;
            if (_urlImage != null && _urlImage.IsVisible)
            {
                imagePath = _urlImage.FileName;
                //Log.Info("imgpath:" + imagePath);
            }
            string _nowLabelLocation = GUIPropertyManager.GetProperty("#infoservice.weather.location");
            if (_savedLocation != _nowLabelLocation)
            {
                Log.Info("Maya BasicHome plugin: Location changed to: " + _nowLabelLocation);

                int i = 0;
                int selected = 0;
                //					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT);
                foreach (LocationInfo loc in _listLocations)
                {
                    string city = loc.City;
                    //Log.Info("loc2:" + city);
                    int pos = city.IndexOf(",");
                    //						if (pos>0) city=city.Substring(0,pos);
                    //							GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT,city);
                    if (_locationCode == loc.CityCode)
                    {
                        //Log.Info("loc: " + city + " blev fundet: " + loc.UrlSattelite);
                        //GUIPropertyManager.SetProperty("#infoservice.weather.location", city);
                        _urlSatellite = loc.UrlSattelite;
                        _urlTemperature = loc.UrlTemperature;
                        _urlUvIndex = loc.UrlUvIndex;
                        _urlWinds = loc.UrlWinds;
                        _urlHumidity = loc.UrlHumidity;
                        _urlPreciptation = loc.UrlPrecip;
                        selected = i;
                    }
                    i++;
                }
                _savedLocation = _nowLabelLocation;

                if (_urlSatellite.Length > 0 && imagePath == _urlSatellite)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.sat", "weather_icon_sat_focus.png");
                }
                else if (_urlSatellite.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.sat", "weather_icon_sat_nofocus.png");
                }

                if (_urlTemperature.Length > 0 && imagePath == _urlTemperature)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.temp", "weather_icon_temp_focus.png");
                }
                else if (_urlTemperature.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.temp", "weather_icon_temp_nofocus.png");
                }

                if (_urlUvIndex.Length > 0 && imagePath == _urlUvIndex)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.uv", "weather_icon_uv_focus.png");
                }
                else if (_urlUvIndex.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.uv", "weather_icon_uv_nofocus.png");
                }

                if (_urlWinds.Length > 0 && imagePath == _urlWinds)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.wind", "weather_icon_wind_focus.png");
                }
                else if (_urlWinds.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.wind", "weather_icon_wind_nofocus.png");
                }

                if (_urlHumidity.Length > 0 && imagePath == _urlHumidity)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.hum", "weather_icon_hum_focus.png");
                }
                else if (_urlHumidity.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.hum", "weather_icon_hum_nofocus.png");
                }

                if (_urlPreciptation.Length > 0 && imagePath == _urlPreciptation)
                {

                    GUIPropertyManager.SetProperty("#maya.weather.icons.prec", "weather_icon_prec_focus.png");
                }
                else if (_urlPreciptation.Length > 0)
                {
                    GUIPropertyManager.SetProperty("#maya.weather.icons.prec", "weather_icon_prec_nofocus.png");
                }


            }





        }








        public void OnTimerEvent2(object source, EventArgs e)
        {
            GUIControl.FocusControl(96742, 6);
            mytimer.Enabled = false;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, 96742, 6, 0, 0, 0, null);
            GUIWindow win = GUIWindowManager.GetWindow(96742);
            if (win != null)
            {
              win.OnMessage(msg);
            }


        }

        public void OnTimerEvent3(object source, EventArgs e)
        {

            Log.Info("wiki venstre 3");
            GUIControl.HideControl(4711, 20);
            GUIControl.FocusControl(4711, 10);
            mytimer.Enabled = false;
        }

        public void OnTimerEvent4(object source, EventArgs e)
        {

            Log.Info("wiki venstre 4");
            GUIControl.HideControl(4711, 25);
            GUIControl.FocusControl(4711, 10);
            mytimer.Enabled = false;
        }

        public void OnTimerEvent5(object source, EventArgs e)
        {

            Log.Info("wiki venstre 5");
            //GUIControl.HideControl(4711, 10);
            GUIControl.ShowControl(4711, 20);
            GUIControl.FocusControl(4711, 20);
            mytimer.Enabled = false;
        }

        public void OnTimerEvent6(object source, EventArgs e)
        {
            GUIWindow pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindowEx);
            GUIImage _pinIcon = pWindow.GetControl(31) as GUIImage;
            if (_pinIcon != null && _pinIcon.IsVisible)
            {
                string _Icon = _pinIcon.FileName;
                _Icon = _Icon.Replace(".png", "_guide.png");
                GUIPropertyManager.SetProperty("#maya.tvguide.recpin", _Icon);
            }
            else
            {
                GUIPropertyManager.SetProperty("#maya.tvguide.recpin", string.Empty);
            }
            mytimer.Enabled = false;
        }







        private void UpdateDetailImages()
        {
            switch (_imageView)
            {
                case ImageView.Satellite:
                    _urlViewImage = _urlSatellite;
                    break;
                case ImageView.Temperature:
                    _urlViewImage = _urlTemperature;
                    break;
                case ImageView.UVIndex:
                    _urlViewImage = _urlUvIndex;
                    break;
                case ImageView.Winds:
                    _urlViewImage = _urlWinds;
                    break;
                case ImageView.Humidity:
                    _urlViewImage = _urlHumidity;
                    break;
                case ImageView.Precipitation:
                    _urlViewImage = _urlPreciptation;
                    break;
            }
            //DetailImageUpdate(_urlViewImage);
        }

        private void InitMostRecents()
        {
            if (MayaHelper.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 5, 1268)))
            {
                setTVSeriesEvents(false);
                getLastThreeAddedTVSeries();
            }

            if (MayaHelper.IsAssemblyAvailable("MovingPictures", new Version(1, 0, 6, 1116)))
            {
                setMovingPicturesEvents(false);
                getLastThreeAddedMovies();
            }
        }

        void setMovingPicturesEvents(bool unsubscribe)
        {
            if (!unsubscribe)
            {
                MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);
            }
            else
            {
                MovingPicturesCore.DatabaseManager.ObjectInserted -= new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);
            }
        }

        void setTVSeriesEvents(bool unsubscribe)
        {
            if (!unsubscribe)
            {
                OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(OnTVSeriesParseCompleted);
            }
            else
            {
                OnlineParsing.OnlineParsingCompleted -= new OnlineParsing.OnlineParsingCompletedHandler(OnTVSeriesParseCompleted);
            }
        }

        void getLastThreeAddedMovies()
        {
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();

            // Filter out any watched movies

            movies.RemoveAll(movie => movie.UserSettings[0].WatchedCount > 0);

            // get filter criteria of movies protected by parental conrols
            bool pcFilterEnabled = MovingPicturesCore.Settings.ParentalControlsEnabled;
            DBFilter<DBMovieInfo> pcFilter = MovingPicturesCore.Settings.ParentalControlsFilter;

            // apply parental control filter to movie list
            List<DBMovieInfo> filteredMovies = pcFilterEnabled ? pcFilter.Filter(movies).ToList() : movies;

            // Sort list in to most recent first
            filteredMovies.Sort((m1, m2) =>
            {
                return m2.DateAdded.CompareTo(m1.DateAdded);
            });

            // Remove anything older than 30 days
            filteredMovies.RemoveAll(movie => movie.DateAdded < DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0, 0)));

            RecentlyAdded.recentAddedMovies = filteredMovies;

            // Clear the properties first
            for (int i = 1; i < 4; i++)
            {
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".title", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".thumb", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".fanart", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".runtime", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".genres", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".year", string.Empty);
                SetProperty("#maya.recentlyAdded.movie" + i.ToString() + ".score", string.Empty);

            }
            // Now take the first 3 
            int mrMovieNumber = 1;
            foreach (DBMovieInfo movie in filteredMovies)
            {

                string movGenres = movie.Genres.ToString();
                string genre = movGenres.Replace("|", ", ");
                char[] MyChar = { ',', ' ' };
                genre = genre.Trim(MyChar);

                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".title", movie.Title);
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".thumb", movie.CoverThumbFullPath);
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".fanart", movie.BackdropFullPath);
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".runtime", GetMovieRuntime(movie) + " min");
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".genres", genre.ToString());
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".year", movie.Year.ToString());
                SetProperty("#maya.recentlyAdded.movie" + mrMovieNumber.ToString() + ".score", Math.Round(movie.Score, MidpointRounding.AwayFromZero).ToString());
                ++mrMovieNumber;
                if (mrMovieNumber == 4)
                    break;
            }
        }

        string GetMovieRuntime(DBMovieInfo movie)
        {
            string minutes = string.Empty;
            if (movie == null) return minutes;

            if (MovingPicturesCore.Settings.DisplayActualRuntime && movie.ActualRuntime > 0)
            {
                // Actual Runtime or (MediaInfo result) is in milliseconds
                // convert to minutes
                minutes = ((movie.ActualRuntime / 1000) / 60).ToString();
            }
            else
                minutes = movie.Runtime.ToString();

            return minutes;
        }

        void getLastThreeAddedTVSeries()
        {

            // get list of the 3 most recently added episodes in tvseries database
            // use file created date rather than added as we dont want to see all episodes for new databases
            RecentlyAdded.recentAddedEpisodes = DBEpisode.GetMostRecent(MostRecentType.Created, 30, 3, true);

            // Clear the properties first
            for (int i = 1; i < 4; i++)
            {
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".title", string.Empty);
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".episodetitle", string.Empty);
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".episodenumber", string.Empty);
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".season", string.Empty);
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".thumb", string.Empty);
                SetProperty("#maya.recentlyAdded.series" + i.ToString() + ".fanart", string.Empty);
            }

            // Set properties
            int mrEpisodeNumber = 1;
            foreach (DBEpisode episode in RecentlyAdded.recentAddedEpisodes)
            {
                DBSeries series = Helper.getCorrespondingSeries(episode[DBEpisode.cSeriesID]);
                if (series != null)
                {
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".title", series.ToString());
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".episodetitle", episode[DBEpisode.cEpisodeName]);
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".episodenumber", episode[DBEpisode.cEpisodeIndex]);
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".season", episode[DBEpisode.cSeasonIndex]);
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".thumb", series.Poster);
                    SetProperty("#maya.recentlyAdded.series" + mrEpisodeNumber.ToString() + ".fanart", Fanart.getFanart(episode[DBEpisode.cSeriesID]).FanartFilename);
                    ++mrEpisodeNumber;
                }
            }
            setMostRecents();
        }


        void getMostRecents()
        {


            for (int i = 0; i < 3; i++)
            {
                mostTVSeriesRecents[i] = null;
                mostMovPicsRecents[i] = null;
                try
                {
                    mostTVSeriesRecents[i] = GUIPropertyManager.GetProperty("#maya.recentlyAdded.series" + (i + 1).ToString() + ".fanart");
                }
                catch { }
                try
                {
                    mostMovPicsRecents[i] = GUIPropertyManager.GetProperty("#maya.recentlyAdded.movie" + (i + 1).ToString() + ".fanart");
                }
                catch { }
                if (mostTVSeriesRecents[i] == " ")
                    mostTVSeriesRecents[i] = null;

                if (mostMovPicsRecents[i] == " ")
                    mostMovPicsRecents[i] = null;
            }
        }

        private void setMostRecents()
        {
            string seasonNum = null;
            string episodeNum = null;
            string formattedSE = null;

            for (int i = 0; i < 3; i++)
            {
                seasonNum = GUIPropertyManager.GetProperty("#maya.recentlyAdded.series" + (i + 1).ToString() + ".season");
                episodeNum = GUIPropertyManager.GetProperty("#maya.recentlyAdded.series" + (i + 1).ToString() + ".episodenumber");

                if (seasonNum != null && episodeNum != null)
                {

                    formattedSE = "S" + seasonNum.PadLeft(2, '0') + "E" + episodeNum.PadLeft(2, '0');
                    SetProperty("#maya.RecentlyAdded." + (i + 1).ToString(), formattedSE);
                }

            }

        }

        private void OnMovieAdded(DatabaseTable obj)
        {
            // check that object insertion was a movie
            if (obj.GetType() == typeof(DBMovieInfo))

                getLastThreeAddedMovies();
        }

        private void OnTVSeriesParseCompleted(bool dataUpdated)
        {
            // if tvseries has new or changed data update recents
            if (dataUpdated)
            {
                getLastThreeAddedTVSeries();
            }
        }

        /// <summary>
        /// Play most recently added Episode
        /// </summary>
        /// <param name="index">index of episode to be played</param>
        private void PlayEpisode(int index)
        {
            if (RecentlyAdded.recentAddedEpisodes != null && index <= RecentlyAdded.recentAddedEpisodes.Count)
            {
                // send off to tvseries video player
                if (PlayMedia.episodePlayer == null)
                    PlayMedia.episodePlayer = new VideoHandler();

                PlayMedia.episodePlayer.ResumeOrPlay(RecentlyAdded.recentAddedEpisodes[index - 1]);
            }
        }

        /// <summary>
        /// Play most recently added movie
        /// </summary>
        /// <param name="index">index of movie to be played</param>
        private void PlayMovie(int index)
        {
            if (RecentlyAdded.recentAddedMovies != null && index <= RecentlyAdded.recentAddedMovies.Count)
            {
                // send off to movingpics video player
                if (PlayMedia.moviePlayer == null)
                    PlayMedia.moviePlayer = new MoviePlayer(new MovingPicturesGUI());

                PlayMedia.moviePlayer.Play(RecentlyAdded.recentAddedMovies[index - 1]);

            }
        }

        // Catch actions up, down, enter/play and home/back
        public void OnAction(Action action)
        {


            int actwin = GUIWindowManager.ActiveWindowEx;


            if (actwin == 510) //Music PlayingNow + MyLyrics
            {
                GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                if (vWindow != null)
                {
                    bool bReturn1 = false;
                    bool bReturn2 = false;
                    bool bReturn3 = false;
                    GUIControl control1 = vWindow.GetControl(155);
                    GUIControl control2 = vWindow.GetControl(5102);
                    GUIControl control3 = vWindow.GetControl(5104);
                    if (control1 != null && control2 != null && control3 != null)
                    {
                        bReturn1 = control1.Focus;
                        bReturn2 = control2.Focus;
                        bReturn3 = control3.Focus;
                    }                                    
                }
            }

            if (actwin == 600 || actwin == 761) //TV Guide screen
            {
                    mytimer.Interval = 50;
                    mytimer.Enabled = true;
                    mytimer.Tick += new System.EventHandler(OnTimerEvent6);
            }

            if (actwin == 2600) //Weather screen
            {




                try
                {
                /*
                    string lastupdate = GUIPropertyManager.GetProperty("#infoservice.weather.lastupdated.datetime");
                    if (lastupdate.Length > 0)
                    {
                        DateTime templongdate = DateTime.Parse(lastupdate);
                        string finallongdate = templongdate.ToString("D", CultureInfo.CurrentCulture.DateTimeFormat);
                        string shortdate = templongdate.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);

                        GUIPropertyManager.SetProperty("#maya.weather.update.date", finallongdate);
                        GUIPropertyManager.SetProperty("#maya.weather.update.time", shortdate);
                    }
                    */

                    if (action.wID == Action.ActionType.ACTION_SELECT_ITEM)
                    {
                        mytimer.Interval = 1000;
                        mytimer.Enabled = true;
                        mytimer.Tick += new System.EventHandler(OnTimerEvent);
                        //GUIControl.FocusControl(2600, 4);
                        //return false;
                        //GUIControl.HideControl(2600, 4);
                    }
                }
                catch
                {
                    Log.Info("Maya BasicHome plugin: exception in DateTime.Parse()");
                }
            }


            if (actwin == 4711 && action.wID == Action.ActionType.ACTION_MOVE_LEFT) //User pressed Left in Wikipedia
            {


                bool bReturn1 = false;
                bool bReturn2 = false;
                GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                if (vWindow != null)
                {
                    // Note: This'll only work for unique id's
                    GUIControl control1 = vWindow.GetControl(20);
                    GUIControl control2 = vWindow.GetControl(25);
                    if (control1 != null && control2 != null)
                    {

                        bReturn1 = control1.Focus;
                        bReturn2 = control2.Focus;
                        if (bReturn1)
                        {
                            mytimer.Interval = 100;
                            mytimer.Enabled = true;
                            mytimer.Tick += new System.EventHandler(OnTimerEvent3);
                        }

                        else if (bReturn2)
                        {
                            mytimer.Interval = 100;
                            mytimer.Enabled = true;
                            mytimer.Tick += new System.EventHandler(OnTimerEvent4);
                        }
                        else
                        {
                            mytimer.Interval = 100;
                            mytimer.Enabled = true;
                            mytimer.Tick += new System.EventHandler(OnTimerEvent5);
                        }
                    }
                }


             }

            if (actwin == 9811 && action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
            {
                GUIControl.FocusControl(9811, 50);
            } 



            if (actwin == 96742 && action.wID == Action.ActionType.ACTION_SELECT_ITEM) //User pressed Enter in Moving Pictures
            {
                bool bReturn = false;
                GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                if (vWindow != null)
                {
                    // Note: This'll only work for unique id's
                    GUIControl control = vWindow.GetControl(66);
                    if (control != null)
                    {
                        bReturn = control.Focus;
                        if (bReturn)
                        {
                            mytimer.Interval = 100;
                            mytimer.Enabled = true;
                            mytimer.Tick += new System.EventHandler(OnTimerEvent2);
                        }
                    }
                }


                //Log.Info("synlig:" + bReturn.ToString());
                GUIControl.HideControl(GUIWindowManager.ActiveWindow, 66);
                GUIControl.HideControl(GUIWindowManager.ActiveWindow, 6);
            }

            if (actwin == 96742 && action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) //User pressed Back in Moving Pictures
            {
                GUIControl.FocusControl(96742, 6);
                bool bReturn = false;
                                GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                                if (vWindow != null)
                                {
                                    GUIControl control = vWindow.GetControl(50);
                                    if (control != null)
                                    {
                                        bReturn = control.IsVisible;
                                        if (bReturn)
                                        {
                                            GUIControl.FocusControl(96742, 50);
                                        }
                                    }

                                }
            }

            if (actwin == 35) //BasicHome screen
            {
                //Log.Info("MayaSkin: " + action.wID.ToString());
                //GUIControl.HideControl(35, 566);

                getMostRecents();
               
                
                numberofitems = ms_headings.Count - 1;

                bool haschildren2 = false;
                foreach (Child mychild in Children)
                {
                    if (mychild.Owner == mypointer)
                    {
                        haschildren2 = true;

                    }

                }
                if (haschildren2 == false)
                {
                    GUIControl.HideControl(35, 566);
                }

                if (GUIWindowManager.ActiveWindow == 35 && action.wID == Action.ActionType.ACTION_MOVE_LEFT)
                {

                    bool bReturn1 = false;
                    bool bReturn2 = false;
                    bool bReturn3 = false;
                    bool bReturn4 = false;
                    bool haschildren = false;

                    GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                    if (vWindow != null)
                    {
                        // Note: This'll only work for unique id's
                        GUIControl control1 = vWindow.GetControl(801);
                        GUIControl control2 = vWindow.GetControl(802);
                        GUIControl control3 = vWindow.GetControl(901);
                        GUIControl control4 = vWindow.GetControl(902);

                        bReturn1 = control1.Focus;
                        bReturn2 = control2.Focus;
                        bReturn3 = control3.Focus;
                        bReturn4 = control4.Focus;

                        if (bReturn1 | bReturn2 | bReturn3 | bReturn4)
                        {
                            foreach (Child mychild in Children)
                            {
                                if (mychild.Owner == mypointer)
                                {
                                    haschildren = true;
                                }

                            }
                            if (!haschildren)
                            {
                                GUIControl.FocusControl(35, 3);
                            }
                        }

                    }
                    //pauseMainMenu = true;
                }


                    

                // Right-key was pressed in home screen
                else if (GUIWindowManager.ActiveWindow == 35 && action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
                {
                    bool bReturn1 = false;
                    bool bReturn2 = false;
                    bool bReturn3 = false;
                    bool bReturn4 = false;

                    GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                    if (vWindow != null)
                    {
                        // Note: This'll only work for unique id's

                        GUIControl control1 = vWindow.GetControl(801);
                        GUIControl control2 = vWindow.GetControl(802);
                        GUIControl control3 = vWindow.GetControl(901);
                        GUIControl control4 = vWindow.GetControl(902);

  
                        bReturn1 = control1.Focus;
                        bReturn2 = control2.Focus;
                        bReturn3 = control3.Focus;
                        bReturn4 = control4.Focus;

                        if (bReturn1 | bReturn2 | bReturn3 | bReturn4)
                        {

                            if (location == 96742) //MovingPictures
                            {
                                GUIControl.FocusControl(35, 41414441);
                            }
                            else
                            {
                                pauseMainMenu = false;
                            }

                            if (location == 9811) //TVSeries
                            {
                                GUIControl.FocusControl(35, 41414442);
                            }
                            {
                                pauseMainMenu = false;
                            }
                        }
                    }
                }




                // Down-key was pressed in home screen
                else if (pauseMainMenu == false && GUIWindowManager.ActiveWindow == 35 && action.wID == Action.ActionType.ACTION_MOVE_DOWN)
                
                  {

                    bool aReturn1 = false;
                    bool aReturn2 = false;
                    bool aReturn3 = false;
                    bool aReturn4 = false;
                    bool aReturn5 = false;
                    bool aReturn6 = false;

                    GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                    if (vWindow != null)
                    {
                        // Note: This'll only work for unique id's
                        GUIControl control1 = vWindow.GetControl(221);
                        GUIControl control2 = vWindow.GetControl(222);
                        GUIControl control3 = vWindow.GetControl(223);
                        GUIControl control4 = vWindow.GetControl(231);
                        GUIControl control5 = vWindow.GetControl(232);
                        GUIControl control6 = vWindow.GetControl(233);

                        aReturn1 = control1.Focus;
                        aReturn2 = control2.Focus;
                        aReturn3 = control3.Focus;
                        aReturn4 = control4.Focus;
                        aReturn5 = control5.Focus;
                        aReturn6 = control6.Focus;


                        if (!aReturn1 && !aReturn2 && !aReturn3 && !aReturn4 && !aReturn5 && !aReturn6)
                        {


                            if (mypointer == numberofitems)
                            {
                                myprevimage = mypointer;
                                mypointer = 0;
                            }
                            else
                            {
                                myprevimage = mypointer;
                                mypointer++;
                            }
                            SetNeighbours(mypointer, numberofitems);
                            if (ms_headings.Count > 0)
                            {
                                SetProperty("#homeitem", ms_headings[mypointer]);
                                SetSubmenu(mypointer);
                                GUIControl.SetControlLabel(35, 801, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 802, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 901, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 902, ms_headings[mypointer]);

                                if (sel_image == 0)
                                {
                                    SetBlankProperty("#visbgimage1", "");
                                    SetProperty("#visbgimage2", "yes");
                                    SetProperty("#bgimage1", ms_bgimages[myprevimage]);
                                    SetProperty("#bgimage2", ms_bgimages[mypointer]);
                                    sel_image = 1;
                                }
                                else
                                {
                                    SetProperty("#visbgimage1", "yes");
                                    SetBlankProperty("#visbgimage2", "");
                                    SetProperty("#bgimage1", ms_bgimages[mypointer]);
                                    SetProperty("#bgimage2", ms_bgimages[myprevimage]);
                                    sel_image = 0;
                                }

                            }
                        }
                    }
                    else
                    {
                        noItems("");
                    }
                    return;

                   }

                // Up-key was pressed in home screen
                else if (pauseMainMenu == false && GUIWindowManager.ActiveWindow == 35 && action.wID == Action.ActionType.ACTION_MOVE_UP)
                {
                    bool aReturn1 = false;
                    bool aReturn2 = false;
                    bool aReturn3 = false;
                    bool aReturn4 = false;
                    bool aReturn5 = false;
                    bool aReturn6 = false;

                    GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                    if (vWindow != null)
                    {
                        // Note: This'll only work for unique id's
                        GUIControl control1 = vWindow.GetControl(221);
                        GUIControl control2 = vWindow.GetControl(222);
                        GUIControl control3 = vWindow.GetControl(223);
                        GUIControl control4 = vWindow.GetControl(231);
                        GUIControl control5 = vWindow.GetControl(232);
                        GUIControl control6 = vWindow.GetControl(233);

                        aReturn1 = control1.Focus;
                        aReturn2 = control2.Focus;
                        aReturn3 = control3.Focus;
                        aReturn4 = control4.Focus;
                        aReturn5 = control5.Focus;
                        aReturn6 = control6.Focus;


                        if (!aReturn1 && !aReturn2 && !aReturn3 && !aReturn4 && !aReturn5 && !aReturn6)
                        {

                            if (mypointer == 0)
                            {
                                myprevimage = mypointer;
                                mypointer = numberofitems;
                            }
                            else
                            {
                                myprevimage = mypointer;
                                mypointer--;
                            }

                            SetNeighbours(mypointer, numberofitems);
                            if (ms_headings.Count > 0)
                            {
                                SetProperty("#homeitem", ms_headings[mypointer]);
                                SetSubmenu(mypointer);
                                GUIControl.SetControlLabel(35, 801, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 802, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 901, ms_headings[mypointer]);
                                GUIControl.SetControlLabel(35, 902, ms_headings[mypointer]);

                                if (sel_image == 0)
                                {
                                    SetBlankProperty("#visbgimage1", "");
                                    SetProperty("#visbgimage2", "yes");
                                    SetProperty("#bgimage1", ms_bgimages[myprevimage]);
                                    SetProperty("#bgimage2", ms_bgimages[mypointer]);
                                    sel_image = 1;
                                }
                                else
                                {
                                    SetProperty("#visbgimage1", "yes");
                                    SetBlankProperty("#visbgimage2", "");
                                    SetProperty("#bgimage1", ms_bgimages[mypointer]);
                                    SetProperty("#bgimage2", ms_bgimages[myprevimage]);
                                    sel_image = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        noItems("");
                    }
                    return;
                }

                // User pressed Enter or Play in home screen
                else if (GUIWindowManager.ActiveWindow == 35 && (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)) //Sure??
                {

                    if (sel_image == 0)
                    {
                        SetProperty("#bgimage2", "");
                    }
                    else
                    {
                        SetProperty("#bgimage1", "");
                    }

                    GUIControl.HideControl(35, 566);

                    bool aReturn1 = false;
                    bool aReturn2 = false;
                    bool aReturn3 = false;
                    bool aReturn4 = false;
                    bool aReturn5 = false;
                    bool aReturn6 = false;
                    bool aReturn7 = false;
                    bool aReturn8 = false;
                    bool aReturn9 = false;
                    bool aReturn10 = false;

                    GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                    if (vWindow != null)
                    {
                        // Note: This'll only work for unique id's
                        GUIControl control1 = vWindow.GetControl(2);
                        GUIControl control2 = vWindow.GetControl(3);
                        GUIControl control3 = vWindow.GetControl(4);
                        GUIControl control4 = vWindow.GetControl(5);
                        GUIControl control5 = vWindow.GetControl(231);
                        GUIControl control6 = vWindow.GetControl(232);
                        GUIControl control7 = vWindow.GetControl(233);
                        GUIControl control8 = vWindow.GetControl(221);
                        GUIControl control9 = vWindow.GetControl(222);
                        GUIControl control10 = vWindow.GetControl(223);

                        aReturn1 = control1.Focus;
                        aReturn2 = control2.Focus;
                        aReturn3 = control3.Focus;
                        aReturn4 = control4.Focus;
                        aReturn5 = control5.Focus;
                        aReturn6 = control6.Focus;
                        aReturn7 = control7.Focus;
                        aReturn8 = control8.Focus;
                        aReturn9 = control9.Focus;
                        aReturn10 = control10.Focus;

                        if (aReturn5)
                        {
                            PlayEpisode(1);

                        }
                        else if (aReturn6)
                        {

                            PlayEpisode(2);

                        }
                        else if (aReturn7)
                        {

                            PlayEpisode(3);
                        }
                        else if (aReturn8)
                        {

                            PlayMovie(1);
                        }
                        else if (aReturn9)
                        {

                            PlayMovie(2);
                        }
                        else if (aReturn10)
                        {

                            PlayMovie(3);
                        }

                        if (!aReturn1 && !aReturn2 && !aReturn3 && !aReturn4 && !aReturn5 && !aReturn6 && !aReturn7 && !aReturn8 && !aReturn9 && !aReturn10)
                        {

                            //Do this if it was the main-menu that called the action
                            if (pauseMainMenu == false)
                            {
                                location = (int)ms_windowids[mypointer];
                                _hyperLinkParameter = ms_window_params[mypointer];
                                Log.Info("Maya Skin MainmenuParameter:" + _hyperLinkParameter);

                            }
                            else
                            {
                                location = (int)ms_subids[selectedID];
                                _hyperLinkParameter = ms_sub_params[selectedID];
                                Log.Info("Maya Skin SubmenuParameter:" + _hyperLinkParameter);
                            }
                            if (location > -1)
                            {
                                
                                GUIWindowManager.ActivateWindow((int)location, _hyperLinkParameter);

                                if (_hyperLinkParameter == "")
                                {
                                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, location, 0, null);
                                    GUIWindowManager.SendThreadMessage(msg);
                                }
                                pauseMainMenu = false;
                                return;

                            }
                        }
                    }
                }

                // User went home
                else if (action.wID == Action.ActionType.ACTION_SWITCH_HOME || action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
                {






                    GUIControl.HideControl(35, 566);
                    //Log.Info("MayaSkin: User went home");
                    if (ms_headings.Count > 0)
                    {
                        SetProperty("#homeitem", ms_headings[mypointer]);
                    }
                    else
                    {
                        noItems("");
                    }
                    //SetSubmenu(mypointer);
                    pauseMainMenu = false;
                }
                return;
            }
        }


        public void Start()
        {

            // Check to see if MayaSkin is the selected skin (halt plugin if not)
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                myskin = xmlreader.GetValueAsString("skin", "name", "");
                mousecontrol = xmlreader.GetValueAsString("general", "mousesupport", "");
            }

            
            
            if (myskin.IndexOf("Maya") != -1)
            {
                Log.Info("Maya BasicHome plugin: Skin found");
                //Skin is Maya, continue

                // Load settings and set the default item
                if (!File.Exists(Config.GetFile(Config.Dir.Config, "MayaSkin.xml")))
                {
                    Log.Info("Maya BasicHome plugin: Config-file MayaSkin.xml not found.");
                    Stop();
                }
                else
                {
                    using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MayaSkin.xml")))
                    {
                            string mytest = xmlreader.GetValueAsString("MayaBasicHome", "menuItemName0", "");
                            if (mytest.Length > 0)
                            {





                                int a = 0;
                                var submenus = new Object[250, 3];
                                do
                                {

                                    mytest = xmlreader.GetValueAsString("MayaBasicHome", "menuItemName" + a, "");
                                    if (mytest != "")
                                    {
                                        ms_headings.Add(xmlreader.GetValueAsString("MayaBasicHome", "menuItemName" + a, ""));
                                        ms_windowids.Add(xmlreader.GetValueAsInt("MayaBasicHome", "menuItemHyperlink" + a, 0));
                                        ms_bgimages.Add("Backdrops/" + xmlreader.GetValueAsString("MayaBasicHome", "menuItemBgImage" + a, ""));
                                        ms_window_params.Add(xmlreader.GetValueAsString("MayaBasicHome", "menuItemParameter" + a, ""));



                                        int b = 0;
                                        do
                                        {
                                            //Read submenu
                                            if (xmlreader.GetValueAsString("MayaBasicHomeSubmenu" + a, "submenuItemName" + b, "") != "")
                                            {
                                                //Log.Info("found some submenus, :" + a + ", " + b + "navn: " + xmlreader.GetValueAsString("MayaBasicHomeSubmenu" + a, "submenuItemName" + b, ""));
                                                Children.Add(new Child(a, b, xmlreader.GetValueAsString("MayaBasicHomeSubmenu" + a, "submenuItemName" + b, ""), xmlreader.GetValueAsInt("MayaBasicHomeSubmenu" + a, "submenuItemHyperlink" + b, 0), xmlreader.GetValueAsString("MayaBasicHomeSubmenu" + a, "submenuItemParameter" + b, "")));
                                            }
                                            b++;
                                        } while (b < 250);




                                    }
                                    a++;
                                } while (a < 25);


                                SetNeighbours(0, ms_headings.Count - 1);
                                SetSubmenu(0);
                                if (ms_headings.Count > 0)
                                {
                                    GUIControl.SetControlLabel(35, 801, ms_headings[0]);
                                    GUIControl.SetControlLabel(35, 802, ms_headings[0]);
                                    GUIControl.SetControlLabel(35, 901, ms_headings[0]);
                                    GUIControl.SetControlLabel(35, 902, ms_headings[0]);
                                    SetProperty("#homeitem", ms_headings[0]);
                                    SetProperty("#bgimage1", ms_bgimages[0]);
                                    SetProperty("#bgimage2", ms_bgimages[0]);
                                }



                            }
                            else
                            {
                                Log.Info("Maya BasicHome plugin: No menuitems configured");
                                Stop();
                            }


                        if (xmlreader.GetValueAsInt("MayaBasicHome", "enWeather", 0) == 1)
                        {
                            SetProperty("#maya.weather.visible", "yes");
                        }



                        if (xmlreader.GetValueAsInt("MayaBasicHome", "enRSS", 0) == 1)
                        {
                            SetProperty("#maya.rss.visible", "yes");
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "clockBasicHome", 0) == 1)
                        {
                            SetProperty("#maya.clock.clockBasicHome", "yes");
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "clockHome", 0) == 1)
                        {
                            SetProperty("#maya.clock.clockHome", "yes");
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "clockElsewhere", 0) == 1)
                        {
                            SetProperty("#maya.clock.clockElsewhere", "yes");
                        }
                        
                        if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartTVSeries", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.fanart.TVSeries", "yes");
                            }
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartMovingPictures", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.fanart.MovingPictures", "yes");
                            }
                        }


                        if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedSeries", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.recentlyAddedSeries", "yes");
                            }
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedMovies", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.recentlyAddedMovies", "yes");
                            }
                        }
                        
                        if (xmlreader.GetValueAsInt("MayaBasicHome", "EnableBackgroundImage", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.backgroundImage", "yes");
                            }
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedRecordings", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.recentlyAddedRecordings", "yes");
                            }
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedMusic", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.recentlyAddedMusic", "yes");
                            }

                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartMusic", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.fanart.Music", "yes");
                            }
                        }

                        if (xmlreader.GetValueAsInt("MayaBasicHome", "EnableBackgroundImagePlugins", 0) == 1)
                        {
                            if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 0) == 1)
                            {
                                SetProperty("#maya.bgImage.Plugins", "yes");
                            }
                        }

                        try
                        {
                            string lastupdate = GUIPropertyManager.GetProperty("#infoservice.weather.lastupdated.datetime");
                            if (lastupdate.Length > 0)
                            {
                                DateTime templongdate = DateTime.Parse(lastupdate);
                                string finallongdate = templongdate.ToString("D", CultureInfo.CurrentCulture.DateTimeFormat);
                                string shortdate = templongdate.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);

                                GUIPropertyManager.SetProperty("#maya.weather.update.date", finallongdate);
                                GUIPropertyManager.SetProperty("#maya.weather.update.time", shortdate);
                            }


                            
                        }
                        catch
                        {
                            Log.Info("Maya BasicHome plugin: exception in DateTime.Parse()");
                        }


                        if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedMovies", 0) == 1 || xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedSeries", 0) == 1)
                        {
                            InitMostRecents();
                            
                        }


                        GUIGraphicsContext.OnNewAction += new OnActionHandler(OnAction);
                        GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);

                    }




                }
            }
        }

        public void Stop()
        {
            Log.Info("Maya BasicHome plugin: Stopped");
        }


        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return "Maya BasicHome Configuration";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return "BasicHome.xml modifier, part of the Maya skin for MediaPortal (by joostzilla and pilehave).";
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return "joostzilla, pilehave";
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            MayaSkinConfig config = new MayaSkinConfig();
            config.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // Get Windows-ID
        public int GetWindowId()
        {
            // WindowID of windowplugin belonging to this setup
            // enter your own unique code
            return -1;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has it's own setup screen
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// If the plugin should have it's own button on the main menu of MediaPortal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true : plugin needs it's own button on home
        /// false : plugin does not need it's own button on home</returns>

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
          strButtonText = String.Empty;
          strButtonImage = String.Empty;
          strButtonImageFocus = String.Empty;
          strPictureImage = String.Empty;
          return false;
        }



        #endregion









    }
}