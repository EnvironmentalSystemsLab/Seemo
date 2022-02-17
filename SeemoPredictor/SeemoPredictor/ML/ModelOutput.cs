using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace SeemoPredictor
{
    public class ModelOutput
    {
        [ColumnName("Score")]
        
        public float OverallRatingB { get; set; }

    }
}