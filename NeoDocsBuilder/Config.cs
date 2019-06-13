using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoDocsBuilder
{
    public static class Config
    {
        private static bool? lazyload;
        private static bool? bootstrap4;
        private static bool? codePrettify;
        private static string origin;
        private static string destination;
        private static string template;
        public static bool? Lazyload
        {
            get {
                if (lazyload == null)
                    Refresh();
                return lazyload;
            }
        }
        public static bool? Bootstrap4
        {
            get
            {
                if (bootstrap4 == null)
                    Refresh();
                return bootstrap4;
            }
        }

        public static bool? CodePrettify
        {
            get
            {
                if (codePrettify == null)
                    Refresh();
                return codePrettify;
            }
        }
        public static string Origin
        {
            get
            {
                if (origin == null)
                    Refresh();
                return origin;
            }
        }
        public static string Destination
        {
            get
            {
                if (destination == null)
                    Refresh();
                return destination;
            }
        }
        public static string Template
        {
            get
            {
                if (template == null)
                    Refresh();
                return template;
            }
        }
        private static void Refresh()
        {
            var json = JObject.Parse(File.ReadAllText("config.json"))["ApplicationConfiguration"];
            lazyload = bool.Parse(json["lazyload"].ToString());
            bootstrap4 = bool.Parse(json["bootstrap4"].ToString());
            codePrettify = bool.Parse(json["codePrettify"].ToString());
            origin = json["origin"].ToString();
            destination = json["destination"].ToString();
            template = json["template"].ToString();
        }
    }
}
