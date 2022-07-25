using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.ML;
using Newtonsoft.Json;
using Microsoft.ML.Trainers.LightGbm;

namespace SeemoPredictor
{
    public class ConsumePrivacy
    {
        public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string MLNetModelPath = Path.GetFullPath(Path.Combine(assemblyFolder + @"\Privacy.zip"));

        private static Lazy<PredictionEngine<ModelInputPrivacy, ModelOutputPrivacy>> PredictionEngine = new Lazy<PredictionEngine<ModelInputPrivacy, ModelOutputPrivacy>>(CreatePredictionEngine);

        //public static string MLNetModelPath = Path.Combine("OverallRatingModel.zip");   //Path.GetFullPath("MLModelCooling.zip");
        //private static string TRAIN_DATA_FILEPATH = @"C:\Users\hahee\OneDrive\바탕 화면\ES lab\final data.CSV";

        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app


        public static ModelOutputPrivacy Predict(ModelInputPrivacy input)
        {
            ModelOutputPrivacy result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<ModelInputPrivacy, ModelOutputPrivacy> CreatePredictionEngine()
        {

            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInputPrivacy, ModelOutputPrivacy>(mlModel);

            return predEngine;
        }
    }
}
