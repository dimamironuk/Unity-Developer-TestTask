using UnityEngine;

public static class LevelProgress
{
    private const string Key = "LevelIndex";

    public static int Index => PlayerPrefs.GetInt(Key, 0);
    public static int Number => Index + 1;

    public static void Advance()
    {
        PlayerPrefs.SetInt(Key, Index + 1);
        PlayerPrefs.Save();
    }
}