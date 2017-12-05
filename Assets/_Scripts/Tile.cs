using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour 
{
    // 地块上的物体
    private Unit _unit;
    public Unit Unit
    {
        get { return _unit; }
        set { _unit = value; }
    }

    // 位置索引
    private int _xIdx;
    public int X
    {
        get { return _xIdx; }
    }
    private int _yIdx;
    public int Y
    {
        get { return _yIdx; }
    }

    /************ 外部调用接口 ************/
    // 设置位置
    public void SetPosition(int x, int y)
    {
        _xIdx = x;
        _yIdx = y;
    }

    // 是否为空地
    public bool IsEmpty()
    {
        return _unit == null;
    }

    // 获取与另一个地块的距离
    public int GetDistance(Tile other)
    {
        return Mathf.Abs(_xIdx - other._xIdx) + Mathf.Abs(_yIdx - other._yIdx);
    }
}
