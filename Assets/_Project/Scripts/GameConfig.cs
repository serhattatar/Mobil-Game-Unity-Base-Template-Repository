using Utilities.BossMode;

public static class GameConfig
{
    [BossControl("Economy/Gold Drop Rate", true)]
    public static float GoldRate = 1.5f;

    [BossControl("Economy/Gem Price", true)]
    public static int GemPrice = 10;
}