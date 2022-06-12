using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using Autodesk.Revit.DB;
using Microsoft.Win32;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using System.Reflection;
using System.ComponentModel;

namespace pza.Tools
{
    internal class AddinConfig
    {
        private static Configuration addinConfig = null;
        private static string exeFilePath = Assembly.GetExecutingAssembly().Location;

        //internal bool ConfigFileExists()
        //{
        //    string configFile = string.Concat(exeFilePath, ".config");
        //    if (File.Exists(configFile)) return true;
        //    else return false;
        //}

        internal bool ConfigSet()
        {
            try
            {
                addinConfig = ConfigurationManager.OpenExeConfiguration(exeFilePath);
                return true;
            }
            catch
            {
                DeleteConfigFile();
                return false;
            }
        }

        private static bool DeleteConfigFile()
        {
            addinConfig = null;
            try
            {
                string configFile = string.Concat(exeFilePath, ".config");
                if (File.Exists(configFile)) File.Delete(configFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal string ReadConfigSetting(string key)
        {
            if ( ConfigSet() )
            {
                try
                {
                    var element = addinConfig.AppSettings.Settings[key];
                    if (element != null && !String.IsNullOrWhiteSpace(element.Value)) return element.Value;
                }
                catch
                {
                    DeleteConfigFile();
                }
            }
            return null;
        }

        internal bool AddUpdateConfigSetting(string key, string value)
        {
            if ( ConfigSet() )
            {
                try
                {
                    var settings = addinConfig.AppSettings.Settings;
                    if (settings[key] == null)
                    {
                        settings.Add(key, value);
                    }
                    else
                    {
                        settings[key].Value = value;
                    }
                    addinConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(addinConfig.AppSettings.SectionInformation.Name);
                    return true;
                }
                catch (ConfigurationErrorsException)
                {
                    DeleteConfigFile();
                }
            }
            return false;
        }


        public static string FileOpenDialog(string ext, string iniDir, string filter)
        {
            var filePath = string.Empty;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.DefaultExt = ext;
                openFileDialog.InitialDirectory = iniDir;
                openFileDialog.Filter = filter;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName; //full + file
                }
            }
            return filePath;
        }
    }
}
