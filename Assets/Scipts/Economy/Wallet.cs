using UnityEngine;

public static class Wallet
{
    private const string Key = "Coins";

    public static int Coins => PlayerPrefs.GetInt(Key, 0);

    public static void Add(int amount)
    {
        PlayerPrefs.SetInt(Key, Coins + amount);
        PlayerPrefs.Save();
    }
}