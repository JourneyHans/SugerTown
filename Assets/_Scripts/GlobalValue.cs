static public class GlobalValue
{
    // 普通物体等级对应的分数
    public static int[] NormalScoreByLevel = 
    {
        5,
        20,
        100,
        500,
        2500,
        10000,
        40000,
    };

    // 可移动物体等级对应的分数
    public static int[] MoveScoreByLevel = 
    {
        0,
        100,
        500,
        2500,
    };

    public static int[] NormalLevelList = { 1, 2, 3, 4 };   // 能够随机出的等级
    public static float[] NormalProbList = { 0.8f, 0.1f, 0.08f, 0.02f }; // 各个等级出现的概率

    public static int[] UnitClassList = { 1, 2 };   // 1代表普通物体，2代表可移动物体
    public static float[] UnitSpawnProbList = { 0.8f, 0.2f };   // 产生两种物体的概率
}