using System;
using System.Collections.Generic;
using Xunit;
using NameClassification;

namespace NameNationalityClassifier.Tests
{
  public class NameNationalityClassifierTests
  {
    private readonly NameClassification.NameNationalityClassifier _classifier;

    public NameNationalityClassifierTests()
    {
      _classifier = new NameClassification.NameNationalityClassifier("features.json");
    }

    [Theory]
    [InlineData("Wang", "china")]
    [InlineData("Petrov", "russia")]
    [InlineData("Smith", "rest")]
    [InlineData("Zhang", "china")]
    [InlineData("Ivanov", "russia")]
    public void PredictNationality_ShouldReturnExpectedNationality(string name, string expectedNationality)
    {
      // Act
      string result = _classifier.PredictNationality(name);

      // Assert
      Assert.Equal(expectedNationality, result);
    }

    [Fact]
    public void GetPredictionProbabilities_ShouldReturnValidProbabilities()
    {
      // Act
      Dictionary<string, double> probabilities = _classifier.GetPredictionProbabilities("Wang");

      // Assert
      Assert.Equal(3, probabilities.Count);
      Assert.Contains("china", probabilities.Keys);
      Assert.Contains("russia", probabilities.Keys);
      Assert.Contains("rest", probabilities.Keys);

      // Sum should be approximately 1.0
      double sum = 0;
      foreach (var prob in probabilities.Values)
      {
        sum += prob;
        Assert.InRange(prob, 0, 1);
      }
      Assert.InRange(sum, 0.99, 1.01);

      // China should have the highest probability for Wang
      Assert.True(probabilities["china"] > probabilities["russia"]);
      Assert.True(probabilities["china"] > probabilities["rest"]);
    }
  }
}