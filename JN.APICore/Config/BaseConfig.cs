using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace APICore
{
    public class BaseConfig
    {
       protected static string GetAppKey(string key, string defaultValue)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");

            string value = string.Empty;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                value = ConfigurationManager.AppSettings[key];
            }
            else
            {
                value = defaultValue;
                config.AppSettings.Settings.Add(key, value);
                config.Save();
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            return value;
        }
    }
}
