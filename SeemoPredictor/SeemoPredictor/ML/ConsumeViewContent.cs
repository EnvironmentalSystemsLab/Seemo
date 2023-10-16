using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;

namespace SeemoPredictor
{
    public class ConsumeViewContent
    {
        public static string assemblyFolder = "";
        public static string MLNetModelPath = "";

        //public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //public static string MLNetModelPath = Path.GetFullPath(Path.Combine(assemblyFolder + @"\ViewContent.zip"));

        private static Lazy<PredictionEngine<ModelInputViewContent, ModelOutputViewContent>> PredictionEngine = new Lazy<PredictionEngine<ModelInputViewContent, ModelOutputViewContent>>(CreatePredictionEngine);

        //public static string MLNetModelPath = Path.Combine("OverallRatingModel.zip");   //Path.GetFullPath("MLModelCooling.zip");
        //private static string TRAIN_DATA_FILEPATH = @"C:\Users\hahee\OneDrive\바탕 화면\ES lab\final data.CSV";

        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app


        public static ModelOutputViewContent Predict(ModelInputViewContent input, string assemblyFolderFromGH)
        {
            assemblyFolder = assemblyFolderFromGH;
            MLNetModelPath = Path.GetFullPath(Path.Combine(assemblyFolder + @"\x64\Debug\net48\ViewContent.zip"));

            ModelOutputViewContent result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<ModelInputViewContent, ModelOutputViewContent> CreatePredictionEngine()
        {

            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInputViewContent, ModelOutputViewContent>(mlModel);

            return predEngine;
        }
    }
}
