/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino.Geometry;


namespace SeemoPredictor
{
    public class ViewResult
    {

        //public List<ViewSensor> Sensors { get; set; }
        public Point3d Pt { get; set; }
        public Vector3d Vec { get; set; }
        //public List<Ray3d> WindowRays { get; set; }
        //public List<Vector3d> WindowRayVectors { get; set; }
        public SeemoResult ResultData { get; set; }
        //generate class of result data , "reflection"(dictionary) iterate all featrues in clas , "csv helper" 

        public ViewResult()
        {

        }

        public ViewResult(Point3d _pt, Vector3d _vec)
        {
            Pt = _pt;
            Vec = _vec;

        }


        public static ViewResult fromJSON(string json)
        {
            return DeserializeJSON<ViewResult>(json);
        }
        public string toJSON()
        {
            return SerializeJSON<ViewResult>(this);
        }


        private static T DeserializeJSON<T>(string json)
        {
            json = json.Trim();
            if ((json.StartsWith("{") && json.EndsWith("}")) || //For object
                (json.StartsWith("[") && json.EndsWith("]"))) //For array
            {
                var set = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<T>(json, set);
            }
            else { return default(T); }
        }

        private static string SerializeJSON<T>(T component)
        {
            var set = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            return JsonConvert.SerializeObject(component, set);
        }


    }
}
*/