using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.ML;
using Newtonsoft.Json;

namespace SeemoPredictor
{
    public class ConsumeOverallRating
    {
        public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string MLNetModelPath = Path.GetFullPath(Path.Combine(assemblyFolder + @"\OverallRatingModel.zip"));

        private static Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictionEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(CreatePredictionEngine);

        //public static string MLNetModelPath = Path.Combine("OverallRatingModel.zip");   //Path.GetFullPath("MLModelCooling.zip");
        //private static string TRAIN_DATA_FILEPATH = @"C:\Users\hahee\OneDrive\바탕 화면\ES lab\final data.CSV";

        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app


        public static ModelOutput Predict(ModelInput input)
        {
            ModelOutput result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<ModelInput, ModelOutput> CreatePredictionEngine()
        {


            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            return predEngine;
        }
    }
}
