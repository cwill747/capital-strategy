using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapitalStrategy.Configuration
{
    public static class ApplicationSettings
    {
        public static string appsalt;
        public static string serverURL;

        static ApplicationSettings()
        {
            appsalt = "Ay2cXjA4";
            serverURL = "cwill.us";
        }
    }
}
