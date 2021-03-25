using System.IO;
using LitJson;

public class Config {
    public string miraiEndpoint;
    public long miraiQQ;
    public string authKey;
    public static Config FromFile() {
        return JsonMapper.ToObject<Config>(File.ReadAllText("flandre_bot_config.json"));
    }
}
