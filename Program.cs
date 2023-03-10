using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IEModeExpiryFix
{
    class Program
    {
        static JObject Get(JObject ob, params string[] path)
        {
            if (ob == null) return default;
            if (path.Length == 1) return (JObject)ob.GetValue(path.First());
            else return Get((JObject)ob.GetValue(path.First()), path.Skip(1).ToArray());
        }

        static void CloseEdge()
        {
            Process[] Edge = Process.GetProcessesByName("msedge");
            if (Edge.Length == 0) return;

            foreach (var p in Edge) try { p.Kill(); } catch { }

            CloseEdge();
        }
        static void Main(string[] args)
        {
            CloseEdge();

            var log = new List<string>();

            var sf = Environment.SpecialFolder.LocalApplicationData;
            var appdata = Environment.GetFolderPath(sf);

            var ie = Path.Combine(appdata, "Microsoft", "Edge", "User Data");

            var files = Directory.GetFiles(ie, "Preferences", SearchOption.AllDirectories);

            foreach (var fname in files)
            {
                JObject json;
                var mod = false;

                using (var file = File.OpenText(fname))
                using (var reader = new JsonTextReader(file))
                {
                    json = (JObject)JToken.ReadFrom(reader);
                    Console.WriteLine(fname);
                    log.Add(fname);



                    var ob = Get(json, "dual_engine", "user_list_data_1");

                    ob?
                        .Children()
                        .Select(r =>
                        {
                            var name = ((JProperty)r).Name;
                            Console.WriteLine(name);
                            log.Add(name);

                            return (JObject)((JObject)r.Parent).GetValue(name);
                        })
                        .Select(r =>
                        {

                            mod = true;

                            var date = (JValue)r.GetValue("date_added");
                            var str = date.Value.ToString();
                            var l = str.Length;
                            var dateLong = Convert.ToInt64(str);

                            var dt = DateTime.FromFileTime(dateLong * 10);

                            DateTime start = new DateTime(2099, 10, 28, 22, 0, 0, DateTimeKind.Utc);
                            var newtime = start.ToFileTime().ToString().Substring(0, 17);

                            date.Value = newtime;

                            return true;
                        })
                        .ToList();

                    ob = Get(json, "dual_engine", "consumer_mode");

                    if (ob != null)
                    {
                        var val = (JValue)ob.GetValue("enabled_state");
                        if (val != null)
                            val.Value = 1;
                        else
                        {
                            ob.Add("enabled_state", 1);
                        }
                    }
                }

                if (mod)
                    using (var file = File.CreateText(fname))
                    using (var writer = new JsonTextWriter(file))
                        json.WriteTo(writer);
            }

            MessageBox.Show(string.Join("\r\n", log));
        }
    }
}
