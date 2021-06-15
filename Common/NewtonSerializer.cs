namespace Common
{
    public class NewtonSerializer : IJsonSerializer
    {
        public string Serialize<T>(T command) => Newtonsoft.Json.JsonConvert.SerializeObject(command);
        public T Deserialize<T>(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }
}