using System.Linq;

public static class Helper {
    public const long MyID = long.MaxValue;
    public static bool TryCommandParse(this string input, string commandMatch, out string cmd) {
        if (!input.Contains(commandMatch)) { cmd = null; return false; }
        cmd = input.Split(commandMatch).Last().Trim();
        return true;
    }
}
