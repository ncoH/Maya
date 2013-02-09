using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using System.Reflection;
using Microsoft.Win32;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Profile;
using System.Diagnostics;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
using WindowPlugins.GUITVSeries;
using MediaPortal.GUI.Music;
using MediaPortal.GUI.View;

namespace ProcessPlugins.MayaBasicHomePlugin
{
    public partial class MayaSkinConfig : Form
    {
        string configDir;
        string path;
        string WP_Path;
        string myskin = "";
        List<string> ids = new List<string>();
        List<menuItem> AvailmenuItems = new List<menuItem>();
        List<menuItem> menuItems = new List<menuItem>();
        List<menuItem> submenuItems = new List<menuItem>();
        Dictionary<int, int> values = new Dictionary<int, int>();
        List<menuItem> HomemenuItems = new List<menuItem>();
        List<menuItem> PluginmenuItems = new List<menuItem>();
        private ArrayList Parents = new ArrayList();
        private ArrayList Children = new ArrayList();
        private ArrayList newChildren = new ArrayList();
        private ArrayList tempChildren = new ArrayList();
        private ArrayList singleChildren = new ArrayList();
        bool configloaded = false;
        bool tvhasbeenadded = false; // Fix for TV showing up two times in MP module-list. Bug?
        bool biggerfonts = false;
        bool biggerminiguide = false;
        bool now_playing_fanart = false;
        bool recordings_multiseat = false;
        bool recentlyFanart = false;
        bool recentlyMoviesFanart = false;
        bool MovingPicturesListview_large = false;
        bool EnableBackgroundImage = false;
        SkinInfo skInfo = new SkinInfo();
        MayaHelper helper = new MayaHelper();

        public static List<string> theTVSeriesViews = new List<string>();
        public static List<string> theMusicViews = new List<string>();

        public static List<KeyValuePair<string, string>> tvseriesViews = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> musicViews = new List<KeyValuePair<string, string>>();


        public MayaSkinConfig()
        {
            InitializeComponent();
        }

        #region Parmeter Handling

        /// <summary>
        /// Get list of views in TVseries database
        /// Key: should be used as hyperlinkParameter
        /// Val: can be used as a default display name
        /// </summary>    
        private List<KeyValuePair<string, string>> GetTVSeriesViews()
        {
            // check if we have already got them
            if (tvseriesViews.Count == 0)
                tvseriesViews = DBView.GetSkinViews();

            return tvseriesViews;
        }

        /// <summary>
        /// Get list of views in Music Views database
        /// Key: should be used as hyperlinkParameter
        /// Val: can be used as a default display name
        /// </summary>    
        private List<KeyValuePair<string, string>> GetMusicViews()
        {
            if (musicViews.Count == 0)
            {
                MusicViewHandler MusicViews = new MusicViewHandler();
                foreach (ViewDefinition MusicView in MusicViews.Views)
                {
                    string viewKey = MusicView.Name;
                    string viewValue = MusicView.LocalizedName;
                    KeyValuePair<string, string> skinview = new KeyValuePair<string, string>(viewKey, viewValue);
                    musicViews.Add(skinview);
                }
            }

            return musicViews;
        }
        
        #endregion

        public class Owner
        {

            public int ID;
            public string Name;
            public int Hyperlink;
            public string property;

            public Owner(int strID, string strName, int intHyperlink, string property)
            {
                this.ID = strID;
                this.Name = strName;
                this.Hyperlink = intHyperlink;
                this.property = property;
            }

            public override string ToString()
            {
                return this.ID + " : " + this.Name;
            }

        }


        public class Child
        {
            public int Owner;
            public int ID;
            public string Name;
            public int Hyperlink;
            public string property;

            public Child(int intOwner, int strID, string strName, int intHyperlink, string property)
            {
                this.Owner = intOwner;
                this.ID = strID;
                this.Name = strName;
                this.Hyperlink = intHyperlink;
                this.property = property;
            }

            public override string ToString()
            {
                return this.ID + " : " + this.Name;
            }

        }
        


        private void LoadWindowPlugins()
        {
            string directory = Config.GetSubFolder(Config.Dir.Plugins, "windows");
            if (!Directory.Exists(directory)) return;

            using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                string[] files = Directory.GetFiles(directory, "*.dll");
                foreach (string pluginFile in files)
                {
                    try
                    {
                        Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

                        if (pluginAssembly != null)
                        {
                            Type[] exportedTypes = pluginAssembly.GetExportedTypes();
                            foreach (Type type in exportedTypes)
                            {
                                // an abstract class cannot be instanciated
                                if (type.IsAbstract) continue;
                                // Try to locate the interface we're interested in
                                if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
                                {
                                    try
                                    {
                                        // Create instance of the current type
                                        object pluginObject = Activator.CreateInstance(type);
                                        ISetupForm pluginForm = pluginObject as ISetupForm;

                                        if (pluginForm != null)
                                        {
                                            if (pluginForm.PluginName().Equals("Home")) continue;
                                            if (pluginForm.PluginName().Equals("my Plugins")) continue;
                                            if (pluginForm.PluginName().Equals("Topbar")) continue; //Works?

                                            string enabled = xmlreader.GetValue("plugins", pluginForm.PluginName());
                                            if (enabled.CompareTo("yes") != 0) continue;

                                            string showInHome = xmlreader.GetValue("home", pluginForm.PluginName());

                                            if (showInHome.CompareTo("no") == 0)
                                            {
                                                menuItem item = new menuItem();
                                                item.name = pluginForm.PluginName();
                                                item.hyperlink = pluginForm.GetWindowId();
                                                item.bgImage = "";
                                                PluginmenuItems.Add(item);
                                            }

                                            if (showInHome.CompareTo("yes") == 0)
                                            {
                                                menuItem item = new menuItem();
                                                item.name = pluginForm.PluginName();
                                                item.hyperlink = pluginForm.GetWindowId();
                                                item.bgImage = "";



                                                if (item.name == "TV" && tvhasbeenadded != true)  // Fix for TV showing up two times in MP module-list. Bug?
                                                {
                                                    tvhasbeenadded = true;
                                                    HomemenuItems.Add(item);
                                                    
                                                }
                                                else if (item.name != "TV")
                                                {
                                                    HomemenuItems.Add(item);
                                                }

                                                
                                            }
                                        }
                                    }
                                    catch (Exception setupFormException)
                                    {
                                      Log.Info("Maya BasicHome plugin: Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                                      Log.Info("Maya BasicHome plugin: Current class is :{0}", type.FullName);
                                        Log.Info(setupFormException.StackTrace);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception unknownException)
                    {
                      Log.Info("Maya BasicHome plugin: Exception in plugin loading :{0}", unknownException.Message);
                        Log.Info(unknownException.StackTrace);
                    }
                }
            }

            // Manually add the plugins window
            menuItem item2 = new menuItem();
            item2.name = "Plugins";
            item2.hyperlink = 34;
            HomemenuItems.Add(item2);

        }





        public static List<FileInfo> getFilesRecursive(DirectoryInfo inputDir)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo[] childDirs = new DirectoryInfo[] { };

            try
            {
                fileList.AddRange(inputDir.GetFiles("*"));
                childDirs = inputDir.GetDirectories();
            }

            catch (Exception e)
            {
                Log.Error("Maya BasicHome plugin: Error while retrieving files/directories for: " + inputDir.FullName, e);
                throw e;
            }


            return fileList;
        }









        /// <summary>
        /// Load the Path information from the Config File into the dictionary
        /// </summary>
        /// <returns></returns>
        private string ReadConfig(string configDir)
        {
            // Make sure the file exists before we try to do any processing
            string strFileName = configDir + "\\MediaPortalDirs.xml";
            string strpath = string.Empty;
            Log.Info("Maya BasicHome plugin: Look for config in " + strFileName);
            if (File.Exists(strFileName))
            {
                Log.Info("Maya BasicHome plugin: Found config");
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(strFileName);
                    if (xml.DocumentElement == null)
                    {
                        return string.Empty;
                    }
                    XmlNodeList dirList = xml.DocumentElement.SelectNodes("/Config/Dir");
                    if (dirList == null || dirList.Count == 0)
                    {
                        return string.Empty;
                    }
                    foreach (XmlNode nodeDir in dirList)
                    {
                        if (nodeDir.Attributes != null)
                        {
                            XmlNode dirId = nodeDir.Attributes.GetNamedItem("id");
                            if (dirId != null && dirId.InnerText != null && dirId.InnerText.Length > 0)
                            {
                                if (dirId.InnerText == "Skin")
                                {
                                    XmlNode path = nodeDir.SelectSingleNode("Path");
                                    if (path != null)
                                    {
                                        string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                                        strpath = path.InnerText.ToString();
                                        strpath = strpath.Replace("%PROGRAMDATA%", commonData);
                                        strpath = strpath.Replace("%ProgramData%", commonData);
                                        //Log.Info("Maya BasicHome plugin: " + dirId.InnerText + ":" + strpath);
                                        return strpath;
                                    }
                                }
                            }
                        }
                    }
                    return strpath;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }



        private void frmMayaBasicHomeEditor_Load(object sender, EventArgs e)
        {
            LoadWindowPlugins();




              configDir = Config.GetFolder(Config.Dir.Base);
              
              string configDirXML = ReadConfig(configDir);
              //Log.Info("Maya BasicHome plugin: Skindir is " + configDirXML);
              configDir = configDirXML;



            // Check to see if Maya is the selected skin (halt plugin if not)
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Base, "MediaPortalDirs.xml")))
            {
                myskin = "Maya";
            }




            if (configDir == null)
                return;
            // Check to see if Maya is the selected skin (halt plugin if not)
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                myskin = "Maya";
            }

            musicViews = GetMusicViews();
            cboParameterViews.Items.Clear();
            theMusicViews.Clear();
            
            foreach (KeyValuePair<string, string> mvv in musicViews)
            {
                theMusicViews.Add(mvv.Value);
            }

            cboParameterViews.Visible = false;
            lblParameter.Visible = false;

            string filename = Path.Combine(Path.Combine(SkinInfo.mpPaths.pluginPath, "windows"), "MP-TVSeries.dll");
            if (MayaHelper.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 5, 1265), filename))
            {
                tvseriesViews = GetTVSeriesViews();

                cboParameterViews.Items.Clear();
                theTVSeriesViews.Clear();

                foreach (KeyValuePair<string, string> tvv in tvseriesViews)
                {
                    theTVSeriesViews.Add(tvv.Value);
                    //cboParameterViews.Items.Add(tvv.Value);
                }

            }

            path = configDir + "\\" + myskin;
            if (System.IO.Directory.Exists(path))
            {

                WP_Path = configDir + "\\" + myskin + "\\Media\\Backdrops";
                if (System.IO.Directory.Exists(WP_Path))
                {
                    string[] fileEntries = System.IO.Directory.GetFiles(WP_Path);
                    int count = 0;
                    foreach (string file in fileEntries)
                    {
                        fileEntries[count] = System.IO.Path.GetFileName(file);
                        count++;
                    }
                    new_bgimage.Items.AddRange(fileEntries);

                    //Log.Info("Maya BasicHome plugin: Path " + path + " found");
                    if (loadIDs(true) == true)
                    {
                        avail_list.Enabled = true;
                        //Enable whatever needs to be enabled...
                    }
                }
            }
            else
            {
                Log.Info("Maya BasicHome plugin: Path " + path + " not found");
            }

                if (loadMenuIDs(true) == true)
                {
                    used_list.Enabled = true;
                    used_list_submenu.Enabled = true;
                    //Enable whatever needs to be enabled...
                }

        }


        //
        // Key/value read methods TVSeries
        //
        // TVSeries Key
        //
        public string getTVSeriesViewKey(string value)
        {
            foreach (KeyValuePair<string, string> tvv in tvseriesViews)
            {
                if (tvv.Value.ToLower() == value.ToLower())
                    return tvv.Key;
            }
            return "false";
        }
        // TVSeries Value
        //
        public string getTVSeriesViewValue(string key)
        {
            foreach (KeyValuePair<string, string> tvv in tvseriesViews)
            {
                if (tvv.Key.ToLower() == key.ToLower())
                    return tvv.Value;
            }
            return "false";
        }
        //
        // Key/value read methods Music
        //
        // Music key
        //
        public string getMusicViewKey(string value)
        {
            foreach (KeyValuePair<string, string> mvv in musicViews)
            {
                if (mvv.Value.ToLower() == value.ToLower())
                    return mvv.Key;
            }
            return "false";
        }
        //Music Value
        //
        public string getMusicViewValue(string key)
        {
            foreach (KeyValuePair<string, string> mvv in musicViews)
            {
                if (mvv.Key.ToLower() == key.ToLower())
                    return mvv.Value;
            }
            return "false";
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {

          if (new_name.Text != "")
          {
            if (avail_list.SelectedItem != null)
            {
            toolStripStatusLabel1.Text = avail_list.SelectedItem.ToString() + " added to menu";
            }
            menuItem item = new menuItem();
            item.name = new_name.Text;
            item.identifier = menuItems.Count;
            item.bgImage = new_bgimage.Text;
            //item.property = cboParameterViews.Text;

            int num1;
            bool res = int.TryParse(new_windowid.Text, out num1);
            if (res == false)
            {
                item.hyperlink = -1;
            }
            else
            {
                item.hyperlink = num1;
            }

            if (cboParameterViews.SelectedIndex != -1)
            {
                if (item.hyperlink == 9811)
                {
                    item.property = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());
                }

                if (item.hyperlink == 501)
                {
                    item.property = getMusicViewKey(cboParameterViews.SelectedItem.ToString());
                }

            }
            // else
            //     item.property = "false";

            menuItems.Add(item);
            used_list.Items.Add(item.name);

            // Clear items
            new_name.Text = "";
            new_bgimage.Text = "";
            new_windowid.Text = "";
            cboParameterViews.Text = "";
            if (used_list.Items.Count > 0)
              avail_list.SelectedIndex = -1;
          }
          else
          {
            MessageBox.Show("Please enter a name for this item", "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }

        }

        private void btnSubAdd_Click(object sender, EventArgs e)
        {

            if (used_list.SelectedItem == null)
            {
                MessageBox.Show("Please select the Mainmenu this item belongs to", "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (avail_list.SelectedItem == null)
            {
                MessageBox.Show("Please select an item from the 'Available modules' list", "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

          if (new_windowid.Text != "" && new_name.Text != "")
          {
            used_list_submenu.Enabled = true;
            toolStripStatusLabel1.Text = avail_list.SelectedItem.ToString() + " added to menu";
            menuItem item = new menuItem();
            item.name = new_name.Text;
            item.owner = menuItems[used_list.SelectedIndex].identifier;
            item.hyperlink = Convert.ToInt32(new_windowid.Text);
            if (cboParameterViews.SelectedIndex != -1)
            {
                if (item.hyperlink == 9811)
                {
                    item.property = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());
                }

                if (item.hyperlink == 501)
                {
                    item.property = getMusicViewKey(cboParameterViews.SelectedItem.ToString());
                }

            }
            newChildren.Add(new Child(item.owner, newChildren.Count, new_name.Text, Convert.ToInt32(new_windowid.Text), item.property));
            //tempChildren.Add(new Child(item.owner, tempChildren.Count, new_name.Text, Convert.ToInt32(new_windowid.Text)));
            used_list_submenu.Items.Add(item.name);

            // Clear items
            new_name.Text = "";
            new_windowid.Text = "";
            cboParameterViews.Text = "";
            if (used_list_submenu.Items.Count > 0)
              avail_list.SelectedIndex = -1;
          }
          else
          {
            MessageBox.Show("All fields must be completed", "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }

        }






        private void btnRemove_Click(object sender, EventArgs e)
        {
          removeToolStripMenuItem_Click(sender, e);
        }

        private void btnSubRemove_Click(object sender, EventArgs e)
        {
          removeToolStripSubMenuItem_Click(sender, e);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
          if (used_list.SelectedItem != null)
          {
            int index = used_list.SelectedIndex;
            int selected = menuItems[used_list.SelectedIndex].identifier;
            menuItem used_item = new menuItem();
            used_item.name = new_name.Text;
            used_item.bgImage = new_bgimage.Text;
            used_item.identifier = selected;
            //used_item.property = cboParameterViews.Text;
              if(new_windowid.Text.Length > 0)
              {
                  used_item.hyperlink = Convert.ToInt32(new_windowid.Text);
              }
              else
              {
                  used_item.hyperlink = -1;
              }

              if (cboParameterViews.SelectedIndex != -1)
              {
                  if (used_item.hyperlink == 9811)
                  {
                      used_item.property = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());
                  }

                  if (used_item.hyperlink == 501)
                  {
                      used_item.property = getMusicViewKey(cboParameterViews.SelectedItem.ToString());
                  }

              }

            used_list.Items.RemoveAt(index);
            used_list.Items.Insert(index, used_item.name);

            menuItems.RemoveAt(index);
            menuItems.Insert(index, used_item);
          }
        }





        private void btnSubUpdate_Click(object sender, EventArgs e)
        {
          if (used_list_submenu.SelectedItem != null)
          {



              int index = used_list_submenu.SelectedIndex;
              int owner = menuItems[used_list.SelectedIndex].identifier;
              int hyperlink = 0;
              string property = "";

              if (new_windowid.Text.Length > 0)
              {
                  hyperlink = Convert.ToInt32(new_windowid.Text);
              }
              else
              {
                  hyperlink = -1;
              }

              if (cboParameterViews.SelectedIndex != -1)
              {
                  if (hyperlink == 9811)
                  {
                      property = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());
                  }

                  if (hyperlink == 501)
                  {
                      property = getMusicViewKey(cboParameterViews.SelectedItem.ToString());
                  }

              }

              Child newchild = new Child(owner, index, new_name.Text, hyperlink, property);


              used_list_submenu.Items.RemoveAt(index);
              used_list_submenu.Items.Insert(index, new_name.Text);


              int a = 0;
              int itemtoremove = 0;
              foreach (Child mychild in newChildren)
              {
                if (mychild.Owner == owner && mychild.ID == index)
                {
                  itemtoremove = a;
                }
                a++;
              }

              singleChildren.RemoveAt(index);
              singleChildren.Insert(index, newchild);

              newChildren.RemoveAt(itemtoremove);
              newChildren.Insert(itemtoremove, newchild);






          }
        }


        // Move selected menu item down one position in list. 
        private void btMoveDown_Click(object sender, EventArgs e)
        {
            // Do nothing if no item is selected or if already at bottom
            if (used_list.SelectedItem != null && used_list.SelectedIndex < used_list.Items.Count - 1)
            {
                    int index = used_list.SelectedIndex;
                    Object listItem = used_list.SelectedItem;
                    menuItem mnuItem = menuItems[index];
                    used_list.Items.RemoveAt(index);
                    menuItems.RemoveAt(index);
                    if (index < used_list.Items.Count)
                    {
                        used_list.Items.Insert(index + 1, listItem);
                        menuItems.Insert(index + 1, mnuItem);
                    }
                    else
                    {
                        used_list.Items.Add(listItem);
                        menuItems.Add(mnuItem);
                    }
                    used_list.SelectedIndex = index + 1;
            }
        }

        // Move selected menu item down one position in list. 
        private void btSubMoveDown_Click(object sender, EventArgs e)
        {
          // Do nothing if no item is selected or if already at bottom
          if (used_list_submenu.SelectedItem != null && used_list_submenu.SelectedIndex < used_list_submenu.Items.Count - 1)
          {
            int index = used_list_submenu.SelectedIndex;
            int a = 0;
            int itemID = 0;
            int selID = 0;
            int itemtoremove = 0;
            int selected = menuItems[used_list.SelectedIndex].identifier;
            foreach (Child mychild in singleChildren)
            {
              if (mychild.Owner == selected && a == used_list_submenu.SelectedIndex)
              {
                //Log.Info("fjern elm. " + mychild.Name);
                itemtoremove = a;
                itemID = mychild.ID;
              }
              a++;
            }

            singleChildren.RemoveAt(index);

            a = 0;
            foreach (Child mychild in newChildren)
            {
                if (mychild.Owner == selected && mychild.ID == index)
              {
                itemtoremove = a;
                selID = mychild.ID;
              }
              a++;
            }

            newChildren.RemoveAt(itemtoremove);

            Child newchild = new Child(selected, selID, new_name.Text, Convert.ToInt32(new_windowid.Text), cboParameterViews.Text);
            Object listItem = used_list_submenu.SelectedItem;
            used_list_submenu.Items.RemoveAt(index);
            if (index < used_list_submenu.Items.Count)
            {
              used_list_submenu.Items.Insert(index + 1, listItem);
              newChildren.Insert(itemtoremove + 1, newchild);
              singleChildren.Insert(index + 1, newchild);
            }
            else
            {
              used_list_submenu.Items.Add(listItem);
              newChildren.Add(newchild);
              singleChildren.Add(newchild);
            }
            used_list_submenu.SelectedIndex = index + 1;
          }
        }






        // Move selected menu item up one position in list. 
        private void btMoveUp_Click(object sender, EventArgs e)
        {
          // Do nothing if no item is selected or if already at top
          if (used_list.SelectedItem != null && used_list.SelectedIndex > 0)
          {
            int index = used_list.SelectedIndex;
            Object listItem = used_list.SelectedItem;
            menuItem mnuItem = menuItems[index];
            used_list.Items.RemoveAt(index);
            menuItems.RemoveAt(index);
            used_list.Items.Insert(index - 1, listItem);
            menuItems.Insert(index - 1, mnuItem);
            used_list.SelectedIndex = index - 1;
          }

        }

        // Move selected menu item up one position in list. 
        private void btSubMoveUp_Click(object sender, EventArgs e)
        {
          // Do nothing if no item is selected or if already at top
          if (used_list_submenu.SelectedItem != null && used_list_submenu.SelectedIndex > 0)
          {

            int index = used_list_submenu.SelectedIndex;
            int selected = menuItems[used_list.SelectedIndex].identifier;
            int a = 0;
            int itemID = 0;
            int place = 0;
            string name = string.Empty;
            string childname = string.Empty;
            int hyper = 0;
            string property = string.Empty;
            Child child = null;

            tempChildren.Clear();
            
            foreach (Child mychild in newChildren)
            {
              //MessageBox.Show("ejer:" + selected + "id:" + mychild.ID + "op:" + index + ":" + mychild.Name);
              if (mychild.Owner == selected && mychild.ID == index)
              {
                place = mychild.ID-1;
                itemID = mychild.ID;
                name = mychild.Name;
                hyper = mychild.Hyperlink;
                child = new Child(selected, place, name, hyper, property);
              }

              else if (mychild.Owner == selected && mychild.ID == index - 1)
              {
                place = mychild.ID+1;
                itemID = mychild.ID;
                name = mychild.Name;
                hyper = mychild.Hyperlink;
                property = mychild.property;
                child = new Child(selected, place, name, hyper, property);
              }
              else
              {
                place = mychild.ID;
                itemID = mychild.ID;
                name = mychild.Name;
                hyper = mychild.Hyperlink;
                property = mychild.property;
                child = new Child(mychild.Owner, place, name, hyper, property);
              }
              tempChildren.Add(child);
              a++;
            }
            newChildren.Clear();
            newChildren.AddRange(tempChildren);

            tempChildren.Clear();
            a = 0;
            do
            {



              foreach (Child mychild in newChildren)
              {
                if (mychild.ID == a)
                {
                  place = mychild.ID;
                  itemID = mychild.ID;
                  name = mychild.Name;
                  hyper = mychild.Hyperlink;
                  property = mychild.property;
                  child = new Child(mychild.Owner, place, name, hyper, property);
                  tempChildren.Add(child);

                }
              }
            
                  a++;
            }
            while (a < newChildren.Count);

            newChildren.Clear();
            newChildren.AddRange(tempChildren);

            Object listItem = used_list_submenu.SelectedItem;
            used_list_submenu.Items.RemoveAt(index);

            used_list_submenu.Items.Insert(index - 1, listItem);

            used_list_submenu.SelectedIndex = index - 1;



            new_windowid.Text = "" + hyper;
                new_name.Text = "" + name;
                toolStripStatusLabel1.Text = "Window ID: " + hyper;

                foreach (Child mychild in newChildren)
                {
                  MessageBox.Show(mychild.ID + " " + mychild.Name);
                }


          }

        }



        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int a = 0;
          if (used_list.SelectedItem != null)
          {
            int owner = menuItems[used_list.SelectedIndex].identifier;
            menuItems.RemoveAt(used_list.SelectedIndex);
            used_list.Items.Remove(used_list.SelectedItem);

            tempChildren.Clear();
            tempChildren.AddRange(newChildren);

            foreach (Child mychild in newChildren)
            {
                if (mychild.Owner == owner)
                {
                    //MessageBox.Show("Fandt " + mychild.Name + " der ejes af " + mychild.Owner + " og er på index " + a);
                    tempChildren.RemoveAt(a);
                    a--;
                }
                a++;
            }

            newChildren.Clear();
            newChildren.AddRange(tempChildren);
            used_list_submenu.Items.Clear();
            //Save();
          }
          else
            MessageBox.Show("No item is selected for removal", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void removeToolStripSubMenuItem_Click(object sender, EventArgs e)
        {
          if (used_list_submenu.SelectedItem != null)
          {
              int a = 0;
              int owner = menuItems[used_list.SelectedIndex].identifier;
              int selected = used_list_submenu.SelectedIndex;

              tempChildren.Clear();
              tempChildren.AddRange(newChildren);

              foreach (Child mychild in newChildren)
              {
                  if(mychild.Owner == owner && a == selected)
                  {
                      MessageBox.Show(selected + "-fjern " + mychild.Name + " der ejes af " + mychild.Owner);
                      tempChildren.RemoveAt(a);
                  }
                  a++;
              }

              newChildren.Clear();
              newChildren.AddRange(tempChildren);
            used_list_submenu.Items.Remove(used_list_submenu.SelectedItem);
          }
          else
            MessageBox.Show("No item is selected for removal", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

 
        // Save settings to file
        private void btSave_Click(object sender, EventArgs e)
        {

            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MayaSkin.xml")))
            {
                if (configloaded)
                {
                    int a = 0;
                    do
                    {
                      xmlwriter.RemoveEntry("MayaBasicHome", "menuItemName" + a);
                      xmlwriter.RemoveEntry("MayaBasicHome", "MayaBasicHomeSubmenu" + a);
                      xmlwriter.RemoveEntry("MayaBasicHome", "menuItemBgImage" + a);
                      xmlwriter.RemoveEntry("MayaBasicHome", "menuItemParameter" + a);

                      int b = 0;
                      do
                      {
                          xmlwriter.RemoveEntry("MayaBasicHomeSubmenu" + a, "submenuItemName" + b);
                          xmlwriter.RemoveEntry("MayaBasicHomeSubmenu" + a, "submenuItemHyperlink" + b);
                          xmlwriter.RemoveEntry("MayaBasicHomeSubmenu" + a, "submenuItemParameter" + b);
                          b++;
                      } while (b < 250);


                        a++;
                    } while (a < 25);
                }





                int mycount = 0;
                //menuItems.Sort(delegate(menuItem li1, menuItem li2) { return li1.identifier.CompareTo(li2.identifier); });

                foreach (menuItem item in menuItems)
                {
                  xmlwriter.SetValue("MayaBasicHome", "menuItemName" + mycount, item.name);
                  xmlwriter.SetValue("MayaBasicHome", "menuItemHyperlink" + mycount, item.hyperlink);
                  xmlwriter.SetValue("MayaBasicHome", "menuItemBgImage" + mycount, item.bgImage);
                  xmlwriter.SetValue("MayaBasicHome", "menuItemParameter" + mycount, item.property);

                  int mycount2 = 0;
                  foreach (Child mychild in newChildren)
                  {
                      //Log.Info("tester gem: " + mychild.Name);
                      if (item.identifier == mychild.Owner)
                      {
                          xmlwriter.SetValue("MayaBasicHomeSubmenu" + mycount, "submenuItemName" + mycount2, mychild.Name);
                          xmlwriter.SetValue("MayaBasicHomeSubmenu" + mycount, "submenuItemHyperlink" + mycount2, mychild.Hyperlink);
                          xmlwriter.SetValue("MayaBasicHomeSubmenu" + mycount, "submenuItemParameter" + mycount2, mychild.property);
                          mycount2++;
                      }
                  }
                  mycount++;
                }




            xmlwriter.SetValue("MayaBasicHome", "enWeather", enWeather.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "enRSS", enRSS.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "biggerFonts", radio_font_bigger.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "biggerMiniguide", radio_miniguide_bigger.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "NowPlayingFanart", radio_now_playing_fanart.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "clockBasicHome", checkBox1_clock.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "clockHome", checkBox2_clock.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "clockElsewhere", checkBox3_clock.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recordingsMultiseat", radio_recordings_multiseat.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "fanartTVSeries", checkBox1_tvseries.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "fanartMovingPictures", checkBox2_movingpictures.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyAddedSeries", cbRecentTvSeries.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyFanart", radio_recentlyFanart.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyAddedMovies", cbRecentMovies.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyMoviesFanart", radio_recentlyMoviesFanart.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "MovingPicturesListview_large", radio_MovPicsList_large.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "EnableBackgroundImage", cbBgImage.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyAddedRecordings", cbRecentRecordings.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "recentlyAddedMusic", cbRecentMusic.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "Enable_iFX", cBiFX.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "fanartMusic", cB_FanartMusic.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "fanartVideo", cB_FanartVideo.Checked ? 1 : 0);
            xmlwriter.SetValue("MayaBasicHome", "EnableBackgroundImagePlugins", cbBgImagePlugins.Checked ? 1 : 0);
                

            }

            if (radio_font_bigger.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Big fonts set");
              if (System.IO.Directory.Exists(path))
              {
                System.IO.File.Copy(path + "\\custom files\\references.bigfonts.xml", path + "\\references.xml", true);
                System.IO.File.Copy(path + "\\custom files\\fonts.bigfonts.xml", path + "\\fonts.xml", true);
              }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Normal fonts set");
              if (System.IO.Directory.Exists(path))
              {
                  System.IO.File.Copy(path + "\\custom files\\references.normalfonts.xml", path + "\\references.xml", true);
                  System.IO.File.Copy(path + "\\custom files\\fonts.normalfonts.xml", path + "\\fonts.xml", true);
              }
            }


            if (radio_miniguide_bigger.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Big miniguide set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\TVMiniGuide.big.xml", path + "\\TVMiniGuide.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Normal miniguide set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\TVMiniGuide.normal.xml", path + "\\TVMiniGuide.xml", true);
                }
            }



            if (radio_now_playing_fanart.Checked == true)
            {
              //Log.Info("Maya BasicHome plugin: Now Playing fanart set");
              if (System.IO.Directory.Exists(path))
              {
                System.IO.File.Copy(path + "\\custom files\\MyMusicPlayingNow.fanart.xml", path + "\\MyMusicPlayingNow.xml", true);
              }
            }
            else
            {
              //Log.Info("Maya BasicHome plugin: Now Playing normal set");
              if (System.IO.Directory.Exists(path))
              {
                System.IO.File.Copy(path + "\\custom files\\MyMusicPlayingNow.normal.xml", path + "\\MyMusicPlayingNow.xml", true);
              }
            }


            if (radio_recordings_multiseat.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Recordings Multiseat set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.facade.recordings.server.xml", path + "\\common.facade.recordings.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mytvrecordedtv.server.xml", path + "\\mytvrecordedtv.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mytvscheduler.server.xml", path + "\\mytvschedulerServer.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myradiorecorded.server.xml", path + "\\myradiorecorded.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Recordings Singleseat set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.facade.recordings.xml", path + "\\common.facade.recordings.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mytvrecordedtv.xml", path + "\\mytvrecordedtv.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mytvscheduler.xml", path + "\\mytvschedulerServer.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myradiorecorded.xml", path + "\\myradiorecorded.xml", true);
                }
            }

            if (radio_recentlyFanart.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: recentlyAdded TVSeries fanart set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\recentlyaddedSeries.Fanart.xml", path + "\\BasicHome.recentlyaddedSeries.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: recentlyAdded TVSeries Full set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\recentlyaddedSeries.Full.xml", path + "\\BasicHome.recentlyaddedSeries.xml", true);
                }
            }

            if (radio_recentlyMoviesFanart.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: recentlyAdded Movies fanart set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\recentlyaddedMovies.Fanart.xml", path + "\\BasicHome.recentlyaddedMovies.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: recentlyAdded Movies Full set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\recentlyaddedMovies.Full.xml", path + "\\BasicHome.recentlyaddedMovies.xml", true);
                }
            }

            if (radio_MovPicsList_large.Checked == true)
            {
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.noinfo.xml", path + "\\movingpictures.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.mediainfo.listview.noinfo.xml", path + "\\movingpictures.mediainfo.listview.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.fanart.noinfo.xml", path + "\\movingpictures.fanart.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.facade.noinfo.xml", path + "\\movingpictures.facade.xml", true);
                }
            }
            else
            {
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.xml", path + "\\movingpictures.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.mediainfo.listview.xml", path + "\\movingpictures.mediainfo.listview.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.fanart.xml", path + "\\movingpictures.fanart.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\movingpictures.facade.xml", path + "\\movingpictures.facade.xml", true);
                }
            }

            if (cbBgImage.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Background images set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\myHome.fanart.xml", path + "\\myHome.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myHomePlugIns.fanart.xml", path + "\\myHomePlugIns.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Background images not set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\myHome.normal.xml", path + "\\myHome.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myHomePlugIns.normal.xml", path + "\\myHomePlugIns.xml", true);
                }
            }

            if (cB_FanartMusic.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Background images set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.window.music.fanart.xml", path + "\\common.window.music.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicgenres.fanart.xml", path + "\\mymusicgenres.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicsongs.fanart.xml", path + "\\mymusicsongs.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Background images not set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.window.music.default.xml", path + "\\common.window.music.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicgenres.default.xml", path + "\\mymusicgenres.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicsongs.default.xml", path + "\\mymusicsongs.xml", true);
                }
            }

            if (cB_FanartVideo.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Background images set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.window.video.fanart.xml", path + "\\common.window.video.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myvideo.fanart.xml", path + "\\myvideo.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Background images not set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.window.video.default.xml", path + "\\common.window.video.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\myvideo.default.xml", path + "\\myvideo.xml", true);
                }
            }

            if (cBiFX.Checked == true)
            {
                //Log.Info("Maya BasicHome plugin: Background images set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\BasicHome.iFX.xml", path + "\\BasicHome.xml", true);
                }
            }
            else
            {
                //Log.Info("Maya BasicHome plugin: Background images not set");
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.File.Copy(path + "\\custom files\\common.window.music.default.xml", path + "\\common.window.music.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicgenres.default.xml", path + "\\mymusicgenres.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\mymusicsongs.default.xml", path + "\\mymusicsongs.xml", true);
                    System.IO.File.Copy(path + "\\custom files\\BasicHome.default.xml", path + "\\BasicHome.xml", true);
                }
            }


            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }






        private bool loadIDs(bool onLoad)
        {
            avail_list.Enabled = true;
            avail_list.Items.Clear();
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
            {
                try
                {
                    if (file.ToLower().StartsWith("common") == false && file.ToLower().Contains("dialog") == false
                        && file.ToLower().Contains("wizard") == false && file.ToLower().Contains("basichome") == false)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(file);
                        XmlNode node = doc.DocumentElement.SelectSingleNode("/window/id");

                        ids.Add(node.InnerText);
                        avail_list.Items.Add(file.Remove(0, file.LastIndexOf(@"\") + 1).Replace(".xml", ""));
                        menuItem avail_item = new menuItem();
                        avail_item.name = file.Remove(0, file.LastIndexOf(@"\") + 1).Replace(".xml", "");
                        avail_item.hyperlink = Convert.ToInt32(node.InnerText);
                        avail_item.bgImage = "";
                        AvailmenuItems.Add(avail_item);
                    }
                }
                catch { }
            }

            if (avail_list.Items.Count > 0)
            {
                //loadSkin("BasicHome.xml");
              avail_list.Items.Clear();
              foreach (menuItem item in AvailmenuItems)
              {
                avail_list.Items.Add(item.name);
              }
                return true;
            }
            else
            {
                // Dont need to complain when first loading the app as its possible that the skin isnt installed
                if (!onLoad)
                    MessageBox.Show("Error reading directory.");
                return false;

            }

        }


        public bool loadMenuIDs(bool onLoad)
        {

            menuItems.Clear();
            used_list.Enabled = true;
            used_list.Items.Clear();
            used_list_submenu.Items.Clear();


            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MayaSkin.xml")))
            {

                int a = 0;

                do
                {
                    //Read menu
                  if (xmlreader.GetValueAsString("MayaBasicHome", "menuItemName" + a, "") != "")
                  {
                    Parents.Add(new Owner(a, xmlreader.GetValueAsString("MayaBasicHome", "menuItemName" + a, ""), xmlreader.GetValueAsInt("MayaBasicHome", "menuItemName" + a, 0), xmlreader.GetValueAsString("MayaBasicHome", "menuItemProperty" + a, "")));
                      menuItem used_item = new menuItem();
                      used_item.identifier = a;
                      used_item.name = xmlreader.GetValueAsString("MayaBasicHome", "menuItemName" + a, "");
                      used_item.hyperlink = xmlreader.GetValueAsInt("MayaBasicHome", "menuItemHyperlink" + a, 0);
                      used_item.bgImage = xmlreader.GetValueAsString("MayaBasicHome", "menuItemBgImage" + a, "");
                      used_item.property = xmlreader.GetValueAsString("MayaBasicHome", "menuItemParameter" + a, "");
                      menuItems.Add(used_item);
                      used_list.Items.Add(used_item.name);
                      configloaded = true;

                      
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
                newChildren = Children;

                enWeather.Checked = true;
                enRSS.Checked = true;
                radio_font_normal.Checked = true;
                radio_miniguide_normal.Checked = true;
                radio_now_playing_normal.Checked = true;
                checkBox1_clock.Checked = true;
                checkBox2_clock.Checked = true;
                checkBox3_clock.Checked = true;
                radio_recordings_singleseat.Checked = true;
                checkBox1_tvseries.Checked = true;
                checkBox2_movingpictures.Checked = true;
                cbRecentTvSeries.Checked = true;
                cbRecentMovies.Checked = true;
                cbBgImage.Checked = true;
                cbRecentRecordings.Checked = true;
                cbRecentMusic.Checked = true;
                cBiFX.Checked = true;
                cB_FanartMusic.Checked = true;
                cB_FanartVideo.Checked = true;
                cbBgImagePlugins.Checked = true;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "enWeather", 1) != 1)
                  enWeather.Checked = false;
                if (xmlreader.GetValueAsInt("MayaBasicHome", "enRSS", 1) != 1)
                  enRSS.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "biggerFonts", 0) == 0)
                {
                  radio_font_normal.Checked = true;
                  radio_font_bigger.Checked = false;
                }
                else
                {
                  radio_font_normal.Checked = false;
                  radio_font_bigger.Checked = true;
                }

                if (xmlreader.GetValueAsInt("MayaBasicHome", "biggerMiniguide", 0) == 0)
                {
                  radio_miniguide_normal.Checked = true;
                  radio_miniguide_bigger.Checked = false;
                }
                else
                {
                  radio_miniguide_normal.Checked = false;
                  radio_miniguide_bigger.Checked = true;
                }

                if (xmlreader.GetValueAsInt("MayaBasicHome", "NowPlayingFanart", 0) == 0)
                {
                  radio_now_playing_normal.Checked = true;
                  radio_now_playing_fanart.Checked = false;
                }
                else
                {
                  radio_now_playing_normal.Checked = false;
                  radio_now_playing_fanart.Checked = true;
                }

                if (xmlreader.GetValueAsInt("MayaBasicHome", "clockBasicHome", 1) != 1)
                    checkBox1_clock.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "clockHome", 1) != 1)
                    checkBox2_clock.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "clockElsewhere", 1) != 1)
                    checkBox3_clock.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "recordingsMultiseat", 0) == 0)
                {
                    radio_recordings_singleseat.Checked = true;
                    radio_recordings_multiseat.Checked = false;
                }
                else
                {
                    radio_recordings_singleseat.Checked = false;
                    radio_recordings_multiseat.Checked = true;
                }
                if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartTVSeries", 1) != 1)
                    checkBox1_tvseries.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartMovingPictures", 1) != 1)
                    checkBox2_movingpictures.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedSeries", 1) != 1)
                    cbRecentTvSeries.Checked = false;
                
                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyFanart", 0) == 0)
                {
                    radio_recentlyFull.Checked = true;
                    radio_recentlyFanart.Checked = false;
                }
                else
                {
                    radio_recentlyFull.Checked = false;
                    radio_recentlyFanart.Checked = true;
                }

                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedMovies", 1) != 1)
                    cbRecentMovies.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyMoviesFanart", 0) == 0)
                {
                    radio_recentlyMoviesFull.Checked = true;
                    radio_recentlyMoviesFanart.Checked = false;
                }
                else
                {
                    radio_recentlyMoviesFull.Checked = false;
                    radio_recentlyMoviesFanart.Checked = true;
                }

                if (xmlreader.GetValueAsInt("MayaBasicHome", "MovingPicturesListview_large", 0) == 0)
                {
                    radio_MovPicsList_normal.Checked = true;
                    radio_MovPicsList_large.Checked = false;
                }
                else
                {
                    radio_MovPicsList_normal.Checked = false;
                    radio_MovPicsList_large.Checked = true;
                }
                if (xmlreader.GetValueAsInt("MayaBasicHome", "EnableBackgroundImage", 1) != 1)
                    cbBgImage.Checked = false;
                
                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedRecordings", 1) != 1)
                    cbRecentRecordings.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "recentlyAddedMusic", 1) != 1)
                    cbRecentMusic.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "Enable_iFX", 1) != 1)
                    cBiFX.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartMusic", 1) != 1)
                    cB_FanartMusic.Checked = false;
                
                if (xmlreader.GetValueAsInt("MayaBasicHome", "fanartVideo", 1) != 1)
                    cB_FanartVideo.Checked = false;

                if (xmlreader.GetValueAsInt("MayaBasicHome", "EnableBackgroundImagePlugins", 1) != 1)
                    cbBgImagePlugins.Checked = false;

            }


            //return true;
            if (used_list.Items.Count > 0)
            {
                return true;
            }
            else
            {
                // Dont need to complain when first loading the app as its possible that the skin isnt installed
                if (!onLoad)
                    MessageBox.Show("Error reading directory.");
                return false;

            }

            // Remove double entries
            int cnt = used_list.Items.Count;
            for (int i = 1; i < (cnt / 2) + 1; i++)
            {
                used_list.Items.RemoveAt(i);
                menuItems.RemoveAt(i);
            }
            cnt = used_list.Items.Count - 1;
            used_list.Items.RemoveAt(cnt);
            menuItems.RemoveAt(cnt);


        }



        public void loadSubMenuIDs(int owner)
        {
        }





        private void avail_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (avail_list.SelectedIndex >= 0)
            {
                if(radioButton1.Checked) {
                    new_windowid.Text = "" + AvailmenuItems[avail_list.SelectedIndex].hyperlink;
                    new_name.Text = "" + AvailmenuItems[avail_list.SelectedIndex].name;
                    
                    toolStripStatusLabel1.Text = "Window ID: " + AvailmenuItems[avail_list.SelectedIndex].hyperlink;
                //used_list.SelectedIndex = -1;

                    switch (AvailmenuItems[avail_list.SelectedIndex].hyperlink)
                    {
                        case 9811:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theTVSeriesViews;
                            cboParameterViews.Text = "";
                            break;
                        case 501:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theMusicViews;
                            cboParameterViews.Text = "";
                            break;
                        default:
                            cboParameterViews.Visible = false;
                            lblParameter.Visible = false;
                            break;
                    }
                }

                if (radioButton2.Checked)
                {
                    new_windowid.Text = "" + HomemenuItems[avail_list.SelectedIndex].hyperlink;
                    new_name.Text = "" + HomemenuItems[avail_list.SelectedIndex].name;
                    toolStripStatusLabel1.Text = "Window ID: " + HomemenuItems[avail_list.SelectedIndex].hyperlink;
                    //cboParameterViews.Text = "";
                  //used_list.SelectedIndex = -1;

                    switch (HomemenuItems[avail_list.SelectedIndex].hyperlink)
                    {
                        case 9811:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theTVSeriesViews;
                            cboParameterViews.Text = "";
                            break;
                        case 501:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theMusicViews;
                            cboParameterViews.Text = "";
                            break;
                        default:
                            cboParameterViews.Visible = false;
                            lblParameter.Visible = false;
                            break;
                    }

                }

                if (radioButton3.Checked)
                {
                    new_windowid.Text = "" + PluginmenuItems[avail_list.SelectedIndex].hyperlink;
                    new_name.Text = "" + PluginmenuItems[avail_list.SelectedIndex].name;
                    toolStripStatusLabel1.Text = "Window ID: " + PluginmenuItems[avail_list.SelectedIndex].hyperlink;
                    cboParameterViews.Text = "";
                  //used_list.SelectedIndex = -1;

                    switch (PluginmenuItems[avail_list.SelectedIndex].hyperlink)
                    {
                        case 9811:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theTVSeriesViews;
                            cboParameterViews.Text = "";
                            break;
                        case 501:
                            cboParameterViews.Visible = true;
                            lblParameter.Visible = true;
                            cboParameterViews.DataSource = theMusicViews;
                            cboParameterViews.Text = "";
                            break;
                        default:
                            cboParameterViews.Visible = false;
                            lblParameter.Visible = false;
                            break;
                    }
                }

            }
        }

        private void used_list_SelectedIndexChanged(object sender, EventArgs e)
        {
          if (used_list.SelectedIndex >= 0)
          {
              used_list_submenu.Items.Clear();
              singleChildren.Clear();
              foreach (Child mychild in newChildren)
              {
                  if (mychild.Owner == menuItems[used_list.SelectedIndex].identifier)
                  {
                      singleChildren.Add(mychild);
                      used_list_submenu.Items.Add(mychild.Name);
                  }
              }
              if (menuItems[used_list.SelectedIndex].hyperlink > -1)
              {
                  new_windowid.Text = "" + menuItems[used_list.SelectedIndex].hyperlink;           
              }
              else
              {
                  new_windowid.Text = "";
              }
            new_name.Text = "" + menuItems[used_list.SelectedIndex].name;
            toolStripStatusLabel1.Text = "Window ID: " + menuItems[used_list.SelectedIndex].hyperlink;
            new_bgimage.Text = menuItems[used_list.SelectedIndex].bgImage;
            //cboParameterViews.Text = menuItems[used_list.SelectedIndex].property;
            
            avail_list.SelectedIndex = -1;

            switch (menuItems[used_list.SelectedIndex].hyperlink)
            {
                case 9811:
                    cboParameterViews.Visible = true;
                    lblParameter.Visible = true;
                    cboParameterViews.DataSource = theTVSeriesViews;
                    cboParameterViews.Text = menuItems[used_list.SelectedIndex].property;
                    break;
                case 501:
                    cboParameterViews.Visible = true;
                    lblParameter.Visible = true;
                    cboParameterViews.DataSource = theMusicViews;
                    cboParameterViews.Text = menuItems[used_list.SelectedIndex].property;
                    break;
                case 96742:
                    cboParameterViews.Visible = true;
                    lblParameter.Visible = true;
                    break;
                default:
                    cboParameterViews.Visible = false;
                    lblParameter.Visible = false;
                    break;
            }

          }
        }

        private void used_list_submenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = used_list_submenu.SelectedIndex;
            int a = 0;
            foreach (Child mychild in singleChildren)
            {
                if (a == selected)
                {
                    if (mychild.Hyperlink > -1)
                    {
                        new_windowid.Text = "" + mychild.Hyperlink;
                    }
                    else
                    {
                        new_windowid.Text = "";
                    }
                    new_name.Text = "" + mychild.Name;
                    cboParameterViews.Text = "" + mychild.property;
                    toolStripStatusLabel1.Text = "Window ID: " + mychild.Hyperlink;
                }
                a++;
            }

            //avail_list.SelectedIndex = -1;

        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
          System.Diagnostics.Process.Start("IExplore", "http://forum.team-mediaportal.com/mediaportal-plugins-47/infoservice-v1-1-5-day-weather-feeds-twitter-basic-home-04-09-2009-updated-60206/");
        }

        private void rBut1_CheckedChanged(object sender, System.EventArgs e)
        {
                avail_list.Items.Clear();
                foreach (menuItem item in AvailmenuItems)
                {
                    avail_list.Items.Add(item.name);
                }
        }

        private void rBut2_CheckedChanged(object sender, System.EventArgs e)
        {
            avail_list.Items.Clear();
            foreach (menuItem item in HomemenuItems)
            {
                avail_list.Items.Add(item.name);
            }
        }

        private void rBut3_CheckedChanged(object sender, System.EventArgs e)
        {
            avail_list.Items.Clear();
            foreach (menuItem item in PluginmenuItems)
            {
                avail_list.Items.Add(item.name);
            }
        }

        private void radio_font_normal_CheckedChanged(object sender, System.EventArgs e)
        {
          biggerfonts = false;

        }

        private void radio_font_bigger_CheckedChanged(object sender, System.EventArgs e)
        {
          biggerfonts = true;
        }


        private void radio_miniguide_normal_CheckedChanged(object sender, System.EventArgs e)
        {
            biggerminiguide = false;
        }

        private void radio_miniguide_bigger_CheckedChanged(object sender, System.EventArgs e)
        {
            biggerminiguide = true;
        }

        private void radio_now_playing_normal_CheckedChanged(object sender, System.EventArgs e)
        {
          now_playing_fanart = false;
        }

        private void radio_now_playing_fanart_CheckedChanged(object sender, System.EventArgs e)
        {
          now_playing_fanart = true;
        }

        private void radio_recordings_singleseat_CheckedChanged(object sender, EventArgs e)
        {
            recordings_multiseat = false;
        }

        private void radio_recordings_multiseat_CheckedChanged(object sender, EventArgs e)
        {
            recordings_multiseat = true;
        }

        private void cbRecentTvSeries_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRecentTvSeries.Checked == true)
                gbTvSeriesOptions.Enabled = true;
            else
                gbTvSeriesOptions.Enabled = false;
        }


        private void cbRecentMovies_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRecentMovies.Checked == true)
                gbMoviesOptions.Enabled = true;
            else
                gbMoviesOptions.Enabled = false;
        }

        private void radio_MovPicsList_normal_CheckedChanged(object sender, EventArgs e)
        {
            MovingPicturesListview_large = false;
        }

        private void radio_MovPicsList_large_CheckedChanged(object sender, EventArgs e)
        {
            MovingPicturesListview_large = true;
        }

        private void cBiFX_CheckedChanged(object sender, EventArgs e)
        {
            if (cBiFX.Checked == true)
                tCiFX.Enabled = true;

            else
                tCiFX.Enabled = false;

        }


        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void new_name_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbBgImagePlugins_CheckedChanged(object sender, EventArgs e)
        {

        }

    }

    public class menuItem
    {
        public int hyperlink;
        public string bgImage;
        public string name;
        public int identifier;
        public int owner;
        public string property;
    }

}