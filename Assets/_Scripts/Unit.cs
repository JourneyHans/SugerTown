using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour 
{
    public Sprite[] spriteList;
    [SerializeField]
    private int _level;
    [SerializeField]
    private int _value;
    [SerializeField]
    private string _name;
    [SerializeField]
    private int _isSpecial;     // 1为普通，2为高级（分数翻倍）

    private Tweener _doTween;   // 当前的Action
    private Vector2 _originPos; // 保存初始位置(世界坐标)
    private int _nextLevel;     // 即将转换的下一等级

    // 生长的土地（父节点）
    private Tile _tile;
    public Tile Tile
    {
        get { return _tile; }
        set { _tile = value; }
    }

    // 位置索引
    private int _xIdx;
    public int X{
        get { return _xIdx; }
    }
    private int _yIdx;
    public int Y
    {
        get { return _yIdx; }
    }

    // 获取等级
    public int Level
    {
        get { return _level; }
    }

    // 获取即将转换的下一等级
    public int NextLevel
    {
        get { return _nextLevel; }
        set { _nextLevel = value; }
    }

    // 设置父节点，并建立关系
    public void SetParent(Tile tile)
    {
        DetachParent();
        _tile = tile;
        transform.SetParent(tile.transform, false);
        _originPos = transform.position;
        tile.Unit = this;
    }

    // 确认放置
    public void Place()
    {
        ResetState();
    }

    // 解除与父节点的关系
    public void DetachParent()
    {
        if (_tile)
        {
            _tile.Unit = null;
            _tile = null;
        }
    }

    // 设置数据
    public void SetData(int level, int x, int y, int isSpecial = 1)
    {
        _level = level;
        _xIdx = x;
        _yIdx = y;
        _isSpecial = isSpecial;
        _value = GlobalValue.ValueByLevel[_level] * _isSpecial;
        name = "Unit_" + (y * 6 + x + 1);

        GetComponent<SpriteRenderer>().sprite = spriteList[_level - 1];
    }

    // 显示合体信息
    public void ShowCombineInfo()
    {
        print("Level " + _level + " Unit");
    }

    // 准备放置的动作
    public void ReadyToPlace()
    {
        SpriteRenderer spRender = GetComponent<SpriteRenderer>();
        _doTween = spRender.DOFade(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    // 准备合并
    public void ReadyToCombine(Vector2 des)
    {
        transform.position = _originPos;
        Vector2 delta = (des - _originPos) / 2;
        _doTween = transform.DOMove(delta, 0.5f).SetRelative().SetLoops(-1, LoopType.Yoyo);
    }

    // 开始合体
    public void StartCombine(Vector2 des, TweenCallback callback = null)
    {
        ResetState();
        transform.DOMove(des, 0.3f).OnComplete(() =>
        {
            Destroy(gameObject);
            if (callback != null)
                callback();
        });
    }

    // 重置状态
    public void ResetState()
    {
        _doTween.Kill();
        SpriteRenderer spRender = GetComponent<SpriteRenderer>();
        spRender.color = Color.white;
        transform.DOMove(_originPos, 0.1f);
    }
}
