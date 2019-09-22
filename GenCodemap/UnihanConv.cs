using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
namespace GenCodemap
{
    public class UnihanConv
    {
        private Dictionary<int, Tuple<string,int>> readings;
        private bool firsttime = true;
        private string lang = "zh";
        private Dictionary<string, string[]> priority = new Dictionary<string, string[]>
        {
            {"kr", new []{ "kKorean", "kMandarin", "kJapaneseOn", "kVietnamese", "kCantonese", "kJapaneseKun" } },
            {"zh", new []{ "kMandarin", "kCantonese", "kKorean", "kJapaneseOn", "kVietnamese", "kJapaneseKun" } },
            {"yue", new []{ "kCantonese", "kMandarin", "kKorean", "kJapaneseOn", "kVietnamese", "kJapaneseKun" } },
            {"ja", new []{ "kJapaneseOn", "kJapaneseKun", "kMandarin", "kCantonese", "kKorean", "kVietnamese" } },
            {"vn", new []{ "kVietnamese", "kMandarin", "kCantonese", "kJapaneseOn", "kJapaneseKun", "kKorean"} }
        };
        private Dictionary<string, string> pronounce_char_map = new Dictionary<string, string>
        {
            {"â","a"},{"à","a"},{"ắ","a"},{"ă","a"},{"ấ","a"},
            {"ü","u"},{"ụ","u"},{"ú","u"},{"ử","u"},{"ư","u"},
            {"ù","u"},
            {"é","e"},
            {"ọ","o"},{"ố","o"},{"ộ","o"},{"ơ","o"},{"ớ","o"},
        };
        public UnihanConv(string lang = "zh")
        {
            if(priority.ContainsKey(lang))
            {
                this.lang = lang;
            }
            
        }
        public void Run(string source, string dest)
        {
            var max_len = 0;
            var tbl = new Dictionary<string, string[]>();
            process_readings(source, tbl);
            max_len = Math.Max(max_len, tbl.Count);
            var out_fn = dest + ".json";
            var json = JsonConvert.SerializeObject(tbl);
            File.WriteAllText(out_fn, json);

        }

        public void check_category(int lcode, string category, string pron)
        {
            
            if(priority[this.lang].Contains(category))
            {
                var p = Array.IndexOf(priority[this.lang], category);
                if(!(readings.ContainsKey(lcode) && readings[lcode].Item2 < p))
                {
                    if(new string[] {"kMandarin", "kCantonese"}.Contains(category))
                    {
                        var r1 = Regex.Replace(pron, @"(\w+)[1-5]", "$1 ");
                        readings[lcode] = Tuple.Create(r1, p);
                    }
                    else if(category == "kHanyuPinyin")
                    {
                        var r1 = Regex.Replace(pron, @"\w+\.\w+:(\w+)", "$1 ");
                        readings[lcode] = Tuple.Create(r1, p);
                    }
                    else
                    {
                        readings[lcode] = Tuple.Create($"{pron} ", p);
                    }
                }
            }
            
        }

        public void gen_map(Dictionary<string, string[]> tbl, int ucode)
        {
            if(ucode == 0)
            {
                return;
            }
            string[] tmap = new string[]{};
            foreach(var i in Enumerable.Range(0, 256))
            {
                if(readings.ContainsKey(i))
                {
                    var reading = readings[i].Item1;
                    if(reading.All(x=>(int)x < 128))
                    {
                        tmap.Append(reading);
                    }
                    else
                    {
                        byte[] bytes = Encoding.Default.GetBytes(reading);
                        tmap.Append(Encoding.UTF8.GetString(bytes));
                    }
                }
                else
                {
                    tmap.Append("");
                }
            }
            tbl[$"x{ucode.ToString("X").ToLower()}"] = tmap;
        }

        public void process_readings(string source, Dictionary<string, string[]> tbl)
        {
            var oucode = 0;
            string pattern = @"U\+([0-9A-F]{2,3})([0-9A-F]{2}\b)";
            using (var textReader = File.OpenText(source))
            {
                string line;
                while( (line = textReader.ReadLine()) != null )
                {
                    var items = line.Substring(0, line.Length - 1).Split("\t");
                    try
                    {
                        var code = Regex.Replace(items[0], pattern, "$1\t$2").Split("\t");
                        var category = items[1];
                        var pron = Regex.Replace(items[2].Split(' ')[0], "[^\00-\x7f]",(Match match) =>{
                            return pronounce_char_map[match.Value];
                        });
                        if(code.Length == 0)
                        {
                            continue;
                        }
                        var ucode = int.Parse(code[0],System.Globalization.NumberStyles.HexNumber);
                        var lcode = int.Parse(code[1],System.Globalization.NumberStyles.HexNumber);
                        if(oucode != ucode)
                        {
                            gen_map(tbl, oucode);
                            oucode = ucode;
                            readings = new Dictionary<string, string[]>();
                        }
                        check_category(lcode, category, pron);
                    }
                    catch
                    {
                        continue;
                    }
                }
                gen_map(tbl, oucode);
            }
        }

    }
}
