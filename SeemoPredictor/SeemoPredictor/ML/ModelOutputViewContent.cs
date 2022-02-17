using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace SeemoPredictor
{
    public class ModelOutputViewAccess
    {
        [ColumnName("Score")]
        
        public float ViewAccessB { get; set; }

    }
}