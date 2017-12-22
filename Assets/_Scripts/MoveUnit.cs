using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : Unit
{
    // 存活时显示的精灵
    public Sprite aliveSp;

    // 是否存活
    public bool IsAlive { get; set; }

    // 获取分数
    public new int Score
    {
        get
        {
            _score = GlobalValue.MoveScoreByLevel[NextLevel - 1] * Special;
            return _score;
        }
    }

    // 设置是否存活
    public void SetAlive(bool isAlive)
    {
        IsAlive = isAlive;
        if (isAlive)
        {
            GetComponent<SpriteRenderer>().sprite = aliveSp;
        }
        else
        {
            SetData(1);
        }
    }

    // 移动
    public void MoveToTile(Tile tile)
    {
        SetParent(tile);
    }


}
