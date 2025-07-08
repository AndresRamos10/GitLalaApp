namespace LalaHealthCare.App.Extensions;

public static class DictionaryExtensions
{
    public static string SerializeToJson(this Dictionary<string, string> dict)
    {
        return System.Text.Json.JsonSerializer.Serialize(dict);
    }
}
