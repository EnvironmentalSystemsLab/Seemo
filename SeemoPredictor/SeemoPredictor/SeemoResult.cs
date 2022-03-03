using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;

namespace SeemoPredictor
{

    public class SeemoResult
    {
        string TimeStamp { get; set; }
        string SeemoVersion { get; set; }

        public List<SmoSensorWithResults> Results { get; set; } = new List<SmoSensorWithResults>();

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        static public SeemoResult FromJSON(string txt)
        {
            return JsonConvert.DeserializeObject<SeemoResult>(txt);
        }

        public void ToFile(string path)
        {
            File.WriteAllText(path, this.ToJSON());
        }

        static public SeemoResult FromFile(string path)
        {
            var txt = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SeemoResult>(txt);
        }

        public override string ToString()
        {
            return "Resutls for " + Results.Count + " nodes.";
        }
    }



}



