using System.IO;
using System.Threading.Tasks;
using LitJson;

public class Persistence {
    public static readonly string FilePath = "persistence.json";
    public static async Task<JsonData> Load() {
        if (File.Exists(FilePath))
            return JsonMapper.ToObject(await File.ReadAllTextAsync(FilePath));
        return new JsonData();
    }
    public static async Task Save(JsonData data) {
        await File.WriteAllTextAsync(FilePath, JsonMapper.ToJson(data));
    }
    public static async Task WriteObject(string key, JsonData value) {
        var data = await Load();
        data[key] = value;
        await Save(data);
    }
    public static async Task<JsonData> GetObject(string key, JsonData defaultReturn = null) {
        var data = await Load();
        if (data.ContainsKey(key)) return data[key];
        return defaultReturn;
    }
}
