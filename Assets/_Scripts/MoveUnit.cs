using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : Unit
{
    // 存活时显示的精灵
    public Sprite aliveSp;

    // 是否存活
    protected bool _isAlive = true;
    public bool IsAlive
    {
        get { return _isAlive; }
    }
    // 获取分数
    public new int Score
    {
        get
        {
            _score = GlobalValue.MoveScoreByLevel[_nextLevel - 1] * _isSpecial;
            return _score;
        }
    }

    // 设置是否存活
    public void SetAlive(bool isAlive)
    {
        GetComponent<SpriteRenderer>().sprite = isAlive ? aliveSp : spriteList[0];
    }

    // 移动
    public void MoveToTile(Tile tile)
    {
        SetParent(tile);
    }


}
