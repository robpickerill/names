using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NameClassification
{
  /// <summary>
  /// Class to predict nationality from names using a pre-trained logistic regression model
  /// </summary>
  public class NameNationalityClassifier
  {
    private Model _model;

    /// <summary>
    /// Initialize the classifier with a model loaded from a JSON file
    /// </summary>
    /// <param name="modelFilePath">Path to the JSON file containing model parameters</param>
    public NameNationalityClassifier(string modelFilePath)
    {
      LoadModel(modelFilePath);
    }

    /// <summary>
    /// Initialize the classifier with a pre-loaded model
    /// </summary>
    /// <param name="model">Model object containing features, classes, coefficients, and intercepts</param>
    public NameNationalityClassifier(Model model)
    {
      _model = model;
    }

    /// <summary>
    /// Predict the nationality of a name
    /// </summary>
    /// <param name="name">The name to classify</param>
    /// <returns>The predicted nationality (class name)</returns>
    public string PredictNationality(string name)
    {
      if (_model == null)
        throw new InvalidOperationException("Model has not been loaded");

      // Convert the name to lowercase
      name = name.ToLower();

      // Extract features from the name
      Dictionary<string, int> featureCounts = ExtractFeatures(name);

      // Calculate scores for each class using logistic regression
      double[] scores = new double[_model.classes.Length];

      for (int i = 0; i < _model.classes.Length; i++)
      {
        scores[i] = _model.intercepts[i];

        foreach (var feature in featureCounts)
        {
          int featureIndex = Array.IndexOf(_model.features, feature.Key);
          if (featureIndex >= 0)
          {
            scores[i] += _model.coefficients[i][featureIndex] * feature.Value;
          }
        }
      }

      // Find the class with the highest score
      int maxIndex = Array.IndexOf(scores, scores.Max());
      return _model.classes[maxIndex];
    }

    /// <summary>
    /// Get prediction probabilities for each class (using softmax)
    /// </summary>
    /// <param name="name">The name to classify</param>
    /// <returns>Dictionary mapping class names to probabilities</returns>
    public Dictionary<string, double> GetPredictionProbabilities(string name)
    {
      if (_model == null)
        throw new InvalidOperationException("Model has not been loaded");

      // Convert the name to lowercase
      name = name.ToLower();

      // Extract features from the name
      Dictionary<string, int> featureCounts = ExtractFeatures(name);

      // Calculate scores for each class using logistic regression
      double[] scores = new double[_model.classes.Length];

      for (int i = 0; i < _model.classes.Length; i++)
      {
        scores[i] = _model.intercepts[i];

        foreach (var feature in featureCounts)
        {
          int featureIndex = Array.IndexOf(_model.features, feature.Key);
          if (featureIndex >= 0)
          {
            scores[i] += _model.coefficients[i][featureIndex] * feature.Value;
          }
        }
      }

      // Convert scores to probabilities using softmax
      double[] probabilities = Softmax(scores);

      // Create dictionary with class names and probabilities
      Dictionary<string, double> result = new Dictionary<string, double>();
      for (int i = 0; i < _model.classes.Length; i++)
      {
        result[_model.classes[i]] = probabilities[i];
      }

      return result;
    }

    private Dictionary<string, int> ExtractFeatures(string name)
    {
      Dictionary<string, int> featureCounts = new Dictionary<string, int>();

      // Add a space at the beginning of the name to match the feature extraction scheme
      name = " " + name;

      // Extract character n-grams from the name
      for (int i = 0; i < name.Length; i++)
      {
        for (int length = 1; length <= 3; length++)
        {
          if (i + length <= name.Length)
          {
            string ngram = name.Substring(i, length);
            if (Array.IndexOf(_model.features, ngram) >= 0)
            {
              if (!featureCounts.ContainsKey(ngram))
                featureCounts[ngram] = 0;
              featureCounts[ngram]++;
            }
          }
        }
      }

      return featureCounts;
    }

    private double[] Softmax(double[] scores)
    {
      double[] exps = scores.Select(x => Math.Exp(x)).ToArray();
      double sum = exps.Sum();
      return exps.Select(exp => exp / sum).ToArray();
    }

    private void LoadModel(string filePath)
    {
      try
      {
        string jsonString = File.ReadAllText(filePath);
        _model = JsonSerializer.Deserialize<Model>(jsonString);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to load model from {filePath}: {ex.Message}", ex);
      }
    }
  }

  /// <summary>
  /// Model class to deserialize the JSON model file
  /// </summary>
  public class Model
  {
    public string[] features { get; set; }
    public string[] classes { get; set; }
    public double[][] coefficients { get; set; }
    public double[] intercepts { get; set; }
  }
}