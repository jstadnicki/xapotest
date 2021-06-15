namespace Common
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T command);
        T Deserialize<T>(string json);
    }
}