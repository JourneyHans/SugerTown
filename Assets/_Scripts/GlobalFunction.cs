using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GF
{
    // 打印控制
    public static bool enableLog = true;
    public static void MyPrint(object message)
    {
        if (!enableLog) return;
        Debug.Log(message);
    }

    public static void PrintUnitList(List<Unit> list)
    {
        foreach (var unit in list)
        {
            Debug.Log("Unit: " + unit.name + " X: " + unit.X + " Y: " + unit.Y);
        }
    }
}
