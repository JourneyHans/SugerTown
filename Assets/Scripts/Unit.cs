using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour 
{
    public Sprite[] spriteList;
    [SerializeField]
    protected int _score;

    protected Tweener _doTween;   // 当前的Action
    protected Vector2 _originPos; // 保存初始位置(世界坐标)

    // 等级
    public int Level { get; set; }

    // 即将转换的下一等级
    public int NextLevel { get; set; }

    // 是否为Special物体，1为普通，2为特殊，分数翻倍
    public int Special { set; get; }

    // 生长的土地（父节点）
    public Tile Tile { get; set; }

    // 位置索引
    public int X
    {
        get { return Tile.X; }
    }
    public int Y
    {
        get { return Tile.Y; }
    }

    // 获取类型
    public System.Type Type
    {
        get { return GetType(); }
    }

    // 获取分数
    public int Score 
    {
        get 
        { 
            _score = GlobalValue.NormalScoreByLevel[NextLevel - 1] * Special;
            return _score;
        }
    }

    // 设置父节点，并建立关系
    public void SetParent(Tile tile)
    {
        DetachParent();
        Tile = tile;
        transform.SetParent(tile.transform, false);
        _originPos = transform.position;
        tile.Unit = this;
    }

    // 解除与父节点的关系
    public void DetachParent()
    {
        if (Tile)
        {
            Tile.Unit = null;
            Tile = null;
        }
    }

    // 设置数据
    public void SetData(int level, int isSpecial = 1)
    {
        Level = level;
        Special = isSpecial;
        name = "Unit_" + (Y * 6 + X + 1);

        GetComponent<SpriteRenderer>().sprite = spriteList[Level - 1];
    }

    // 显示合体信息
    public void ShowCombineInfo()
    {
        //print("Level " + _level + " Unit");
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

    // 撤销操作
}
