using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour 
{
    // 地块上的物体
    public Unit Unit { get; set; }

    // 位置索引
    public int X { get; set; }
    public int Y { get; set; }

    /************ 外部调用接口 ************/
    // 设置位置
    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    // 是否为空地
    public bool IsEmpty()
    {
        return Unit == null;
    }

    // 获取与另一个地块的距离
    public int GetDistance(Tile other)
    {
        return Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y);
    }
}
