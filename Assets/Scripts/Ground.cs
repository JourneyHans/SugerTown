﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ground : MonoBehaviour 
{
    public Transform tilePrefab;        // 地块
    public Transform unitPrefab;        // 物体
    public Transform moveUnitPrefab;    // 可移动物体

    public static float TileWidth;      // 地块宽
    public static float TileHeight;     // 地块高

    private int _row = 6;
    private int _col = 6;

    public Unit CurrentUnit { get; set; }   // 当前物体
    private TouchModel _touchModel; // 点击事件模块

    // 假设先初始化7个点
    private const int _spawnNum = 7;

    // 地块列表
    private List<Tile> _tileList = new List<Tile>();

    // 物体列表
    private List<Unit> _unitList = new List<Unit>();

    // 单个合并物体的列表
    private List<Unit> _singleCombineUnitList = new List<Unit>();

    // 当前合并的物体列表
    private List<Unit> _combineUnitList = new List<Unit>();

    private void Awake()
    {
        TileWidth = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        TileHeight = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void Start()
    {
        _touchModel = GetComponent<TouchModel>();
        // 初始化地块
        for (int c = 0; c < _col; c++)
        {
            for (int r = 0; r < _row; r++)
            {
                Transform tileTr = Object.Instantiate(tilePrefab);
                tileTr.SetParent(transform);
                tileTr.localPosition = new Vector3(r * TileWidth, c * TileHeight);
                tileTr.name = "Tile_" + (c * _col + r + 1);
                Tile tile = tileTr.GetComponent<Tile>();
                tile.SetPosition(r, c);
                _tileList.Add(tile);

                // 注册点击事件
                _touchModel.AddTouchEvent(tileTr.gameObject);
            }
        }
	}

    // 初始化物体
    public void InitUnits()
    {
        while (_unitList.Count < _spawnNum)
        {
            int x = Random.Range(0, _col - 1);
            int y = Random.Range(0, _row - 1);
            Tile tile = GetTile(x, y);
            if (tile.IsEmpty())      // 该位置为空
            {
                Unit unit = CreateUnit(tile);
                _unitList.Add(unit);
            }
        }

        CurrentUnit = _unitList[_unitList.Count - 1];
    }

    // 生成物体
    public void GenerateUnit()
    {
        if (IsFull())
        {
            // 如果已经满了，且当前物体不是具有消除功能的物体，则游戏结束
            GameOver();
            return;
        }

        Tile nextTile = CheckSurround(CurrentUnit.Tile);
        Unit unit = CreateUnit(nextTile);
        CurrentUnit = unit;
        CurrentUnit.ReadyToPlace();
    }

    // 放置物体
    public void PlaceUnit()
    {
        CurrentUnit.ResetState();
        _unitList.Add(CurrentUnit);
        if (_combineUnitList.Count >= 2)
        {
            // 合体！
            DoCombination();
        }
        else
        {
            // 未合体，直接调用完成合体
            FinishCombination();
        }
    }

    // 合体
    private void DoCombination()
    {
        _touchModel.TouchEnabled = false;   // 禁止触摸
        Vector2 des = CurrentUnit.transform.position;
        foreach (var unit in _combineUnitList)
        {
            // 开始合并的动画
            unit.StartCombine(des);
            _unitList.Remove(unit);
            if (unit == _combineUnitList[_combineUnitList.Count - 1])
            {   // 完成合并的回调
                unit.StartCombine(des, FinishCombination);
            }
        }
    }

    // 合体完成
    private void FinishCombination()
    {
        _combineUnitList.Clear();       // 清空合体列表
        _touchModel.TouchEnabled = true;    // 开启触摸

        // Current升级
        CurrentUnit.SetData(CurrentUnit.NextLevel, CurrentUnit.Special);

        // 更新分数
        ScoreManager.Instance.UpdateScore = true;

        // 产生下一个物体
        GenerateUnit();

        // 检测移动物体是否移动
        foreach (var unit in _unitList)
        {
            MoveUnit moveUnit = unit.GetComponent<MoveUnit>();
            if (moveUnit != null && moveUnit.IsAlive)
            {
                bool isSurrounded = WanderAround(moveUnit);
                if (isSurrounded)
                {
                    moveUnit.SetAlive(false);
                }
            }
        }
    }

    // 移除地块
    public void EraseUnit(Tile tile)
    {
        // 如果当前物体是消除操作，消除选中地块上的物体
        GF.MyPrint("=========== EraseUnit Began: " + tile.name);
    }

    // 移动物体
    private bool WanderAround(MoveUnit moveUnit)
    {
        List<Tile> emptyList = new List<Tile>();
        bool isSurrounded = true;
        Tile currentTile = moveUnit.Tile;

        // 检测上下左右是否有空位可以移动
        int start_x = currentTile.X - 1;
        int end_x = currentTile.X + 1;
        start_x = start_x < 0 ? currentTile.X : start_x;
        end_x = end_x > _row - 1 ? currentTile.X : end_x;
        for (int x = start_x; x <= end_x; x++)
        {
            int offset_y = 1 - Mathf.Abs(x - currentTile.X);
            int start_y = currentTile.Y - offset_y;
            int end_y = currentTile.Y + offset_y;
            start_y = start_y < 0 ? currentTile.Y : start_y;
            end_y = end_y > _col - 1 ? currentTile.Y : end_y;

            for (int y = start_y; y <= end_y; y++)
            {
                if (x == currentTile.X && y == currentTile.Y) continue;   // 不检测自身
                Tile surroundTile = GetTile(x, y);
                if (surroundTile.IsEmpty())
                {
                    // 如果有一个是空的，那么MoveUnit就不会被围死
                    isSurrounded = false;
                    emptyList.Add(surroundTile);
                }
            }
        }

        // 如果有空位，随机选一个作为移动的目标
        if (emptyList.Count > 0)
        {
            int pickOne = Random.Range(0, emptyList.Count);
            Tile target = emptyList[pickOne];
            moveUnit.MoveToTile(target);
        }

        return isSurrounded;
    }

    // 检测可合并的物体
    public void CheckLinkSurround(Tile tile)
    {
        // 重置状态、清空合并列表、清空得分列表
        ResetCombineList();
        _combineUnitList.Clear();
        ScoreManager.Instance.ClearScoreList();

        // 重置CurrentUnit的NextLevel和Special
        CurrentUnit.NextLevel = CurrentUnit.Level;
        CurrentUnit.Special = 1;

        // 合并过程
        GF.MyPrint("=========== CheckLinkSurround Began: " + tile.name);
        LinkSurround(tile);
        GF.MyPrint("=========== CheckLinkSurround Ended: " + tile.name);

        // 连接完成，检查是否会合并
        if (_combineUnitList.Count >= 2)
        {
            foreach (var unit in _combineUnitList)
            {
                unit.ReadyToCombine(tile.transform.position);
            }
        }
    }

    // 链接可以合并的物体(递归)
    private void LinkSurround(Tile tile)
    {
        GF.MyPrint("----------- LinkSurround Began: " + tile.name);
        // 清空单次合并列表
        _singleCombineUnitList.Clear();

        // 开始单次合并
        DoLink(tile);

        GF.MyPrint("Single Combine contains: " + _singleCombineUnitList.Count);

        /** 计算分数，分数规则为：
        /* 1. 未合体：得到物体本身的分数
        /* 2. 合体：得到合体过程中每一级物体的分数（但不包含初始的物体）*/

        // 单次合并如果成功后，继续查找下一等级的物体
        if (_singleCombineUnitList.Count >= 2)
        {
            CurrentUnit.NextLevel++;           // 等级提升
            CurrentUnit.Special = (_singleCombineUnitList.Count > 2 ? 2 : 1);// 是否超过3个合体，变为Special物体

            // 将分数加入分数列表准备做分步运算
            ScoreManager.Instance.AddToScoreList(CurrentUnit.Score);

            // 递归检测下一等级的Unit是否会合体
            LinkSurround(CurrentUnit.Tile);
        }
        else
        {
            GF.MyPrint("Remove unit which there is only one...\t" + _singleCombineUnitList.Count);
            _combineUnitList = _combineUnitList.Except(_singleCombineUnitList).ToList();

            // 没有升过级，得分为本身的分数
            if (CurrentUnit.NextLevel == CurrentUnit.Level)
            {
                ScoreManager.Instance.AddToScoreList(CurrentUnit.Score);
            }
        }
        GF.MyPrint("----------- LinkSurround Ended");
    }

    // 单次合并（递归）
    private void DoLink(Tile tile)
    {
        GF.MyPrint("----------- DoLink Began");
        int start_x = tile.X - 1;
        int end_x = tile.X + 1; 
        start_x = start_x < 0 ? tile.X : start_x;
        end_x = end_x > _row - 1 ? tile.X : end_x;
        for (int x = start_x; x <= end_x; x++)
        {
            int offset_y = 1 - Mathf.Abs(x - tile.X);
            int start_y = tile.Y - offset_y;
            int end_y = tile.Y + offset_y;
            start_y = start_y < 0 ? tile.Y : start_y;
            end_y = end_y > _col - 1 ? tile.Y : end_y;

            for (int y = start_y; y <= end_y; y++)
            {
                if (x == tile.X && y == tile.Y) continue;   // 不检测自身
                Tile surroundTile = GetTile(x, y);
                if (surroundTile.IsEmpty())
                {
                    GF.MyPrint(surroundTile.name + " is Empty");
                    continue;       // 如果是空的，跳过
                }
                Unit unit = surroundTile.Unit;
                if (CheckIsInCombineList(unit) || unit == CurrentUnit)
                {
                    GF.MyPrint(unit.name + " is already added");
                    continue;       // 已加入列表，跳过
                }
                if (unit.Level != CurrentUnit.NextLevel)
                {
                    GF.MyPrint(string.Format("Level not match: {0}/Current : {1}/{2}", unit.name, unit.Level, CurrentUnit.NextLevel));
                    continue;   // 等级不一样，跳过
                }
                if (unit.Type != CurrentUnit.Type)
                {
                    GF.MyPrint(string.Format("Type not match: {0}/Current : {1}/{2}", unit.name, unit.Type, CurrentUnit.Type));
                    continue;   // 类型不一样，跳过
                }
                MoveUnit moveUnit = unit.GetComponent<MoveUnit>();
                if (moveUnit != null && moveUnit.IsAlive)
                {
                    GF.MyPrint(string.Format("{0} is still alive", moveUnit.name));
                    continue;   // 移动物体还活着
                }
                GF.MyPrint(unit.name + " will be added");
                _combineUnitList.Add(unit);
                _singleCombineUnitList.Add(unit);
                // 递归检测下一个邻位
                DoLink(surroundTile);
            }
        }
        GF.MyPrint("----------- DoLink Ended");
    }

    // 检测是否已经加入到合成列表
    private bool CheckIsInCombineList(Unit target)
    {
        foreach (var unit in _combineUnitList)
        {
            if (target.X == unit.X && target.Y == unit.Y)
            {
                return true;
            }
        }
        return false;
    }

    // 根据x、y索引地块
    public Tile GetTile(int x, int y)
    {
        int idx = y * _col + x;
        return _tileList[idx];
    }

    // 重置 Combine Unit List
    public void ResetCombineList()
    {
        foreach (var unit in _combineUnitList)
        {
            unit.ResetState();
        }
    }

    // 检测是否已经满了
    private bool IsFull()
    {
        return _unitList.Count == _row * _col;
    }

    // 检测目标周围的空位
    private Tile CheckSurround(Tile target)
    {
        Tile ret = null;
        foreach (var tile in _tileList)
        {
            if (tile.IsEmpty())
            {
                if (ret == null)
                {
                    ret = tile;
                }
                else
                {
                    if (tile.GetDistance(target) < ret.GetDistance(target))
                    {
                        ret = tile;
                    }
                }
            }
        }
        return ret;
    }

    // 游戏结束
    private void GameOver()
    {
        GF.MyPrint("Game Over!");
    }

    // 创建Unit
    private Unit CreateUnit(Tile tile, int level = -1)
    {
        int type = RandomController.Range(GlobalValue.UnitClassList, GlobalValue.UnitSpawnProbList);
        Unit unit = null;
        switch (type)
        {
            case 1:
                unit = CreateNormalUnit(tile, level);
                break;
            case 2:
                unit = CreateMovableUnit(tile);
                break;
            default:
                print("Wrong type: " + type);
                break;
        }
        return unit;
    }

    // 创建普通Unit
    private Unit CreateNormalUnit(Tile tile, int level = -1)
    {
        if (level == -1)    // 未指定等级，就用随机等级
        {
            level = RandomController.Range(GlobalValue.NormalLevelList, GlobalValue.NormalProbList);
        }
        Transform unitTr = Instantiate(unitPrefab);
        Unit unit = unitTr.GetComponent<Unit>();
        unit.SetParent(tile);
        unit.SetData(level);
        return unit;
    }

    // 创建移动Unit
    private MoveUnit CreateMovableUnit(Tile tile)
    {
        Transform moveUnitTr = Instantiate(moveUnitPrefab);
        MoveUnit moveUnit = moveUnitTr.GetComponent<MoveUnit>();
        moveUnit.SetParent(tile);
        moveUnit.SetData(1);
        moveUnit.SetAlive(true);
        return moveUnit;
    }
}
