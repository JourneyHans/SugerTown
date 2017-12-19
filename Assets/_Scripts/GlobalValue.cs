static public class GlobalValue
{
    // 等级对应的分数
    public static int[] ScoreByLevel = new int[]
    {
        5,
        20,
        100,
        500,
        2500,
        10000,
        40000,
    };

    public static int[] LevelList = { 1, 2, 3, 4 };   // 能够随机出的等级
    public static float[] ProbabilityList = { 0.8f, 0.1f, 0.08f, 0.02f }; // 各个等级出现的概率
}