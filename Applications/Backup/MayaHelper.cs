using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using MediaPortal.Configuration;

namespace ProcessPlugins.MayaBasicHomePlugin
{
    class MayaHelper
    {

        public string fileVersion(string fileToCheck)
        {
            if (File.Exists(fileToCheck))
            {
                FileVersionInfo fv = FileVersionInfo.GetVersionInfo(fileToCheck);
                return fv.FileVersion;

            }
            else
                return "0.0.0.0";
        }

        #region Assembly Helpers
        public static bool IsAssemblyAvailable(string name, Version ver)
        {
            return IsAssemblyAvailable(name, ver, null);
        }
        public static bool IsAssemblyAvailable(string name, Version ver, string filename)
        {
            bool result = false;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly a in assemblies)
            {
                try
                {
                    if (a.GetName().Name == name)
                    {
                        if (a.GetName().Version >= ver)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    result = false;
                }
            }

            if (!result)
            {
                try
                {
                    Assembly assembly = null;
                    if (string.IsNullOrEmpty(filename))
                    {
                        assembly = Assembly.ReflectionOnlyLoad(name);
                        if (assembly.GetName().Version >= ver) result = true;
                    }
                    else
                    {
                        assembly = Assembly.ReflectionOnlyLoadFrom(filename);
                        if (assembly.GetName().Version >= ver) result = true;
                    }
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

         #endregion

    }
}
