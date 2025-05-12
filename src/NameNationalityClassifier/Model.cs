namespace NameClassification
{
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