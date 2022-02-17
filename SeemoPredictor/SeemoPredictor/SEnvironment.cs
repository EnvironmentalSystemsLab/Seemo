using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class SEnvironment
    {
        public Mesh SceneMesh { get; set; }
        public List<int> FaceCnt { get; set; }
        public List<Mesh> Pavements { get; set; }
        public List<Mesh> Grass { get; set; }

        public SEnvironment()
        {

        }

        public SEnvironment(Mesh _scenemesh, List<int> _facecnt, List<Mesh> _pavements, List<Mesh> _grass)
        {
            SceneMesh = _scenemesh;
            FaceCnt = _facecnt;
            Pavements = _pavements;
            Grass = _grass;
        }


    }
}
