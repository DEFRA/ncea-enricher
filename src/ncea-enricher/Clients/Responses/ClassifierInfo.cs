namespace Ncea.Classifier.Microservice.Clients.Responses
{
    public class ClassifierInfo
    {
        [Newtonsoft.Json.JsonProperty("code", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Code { get; set; } = null!;

        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Name { get; set; } = null!;

        [Newtonsoft.Json.JsonProperty("level", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int Level { get; set; }

        [Newtonsoft.Json.JsonProperty("classifiers", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ICollection<ClassifierInfo>? Classifiers { get; set; }

    }
}
