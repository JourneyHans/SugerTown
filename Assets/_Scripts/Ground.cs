using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ground : MonoBehaviour 
{
    public Transform tilePrefab;       // 地块
    public Transform unitPrefab;       // 物体

    public static float TileWidth;      // 地块宽
    public static float TileHeight;     // 地块高

    private int _row = 6;
    private int _col = 6;
    private Unit _currentUnit;      // 当前物体
    public Unit CurrentUnit
    {
        get { return _currentUnit; }
    }
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

        _currentUnit = _unitList[_unitList.Count - 1];
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

        Tile nextTile = CheckSurround(_currentUnit.Tile);
        Unit unit = CreateUnit(nextTile);
        _currentUnit = unit;
        _currentUnit.ReadyToPlace();
    }

    // 放置物体
    public void PlaceUnit()
    {
        _currentUnit.ResetState();
        _unitList.Add(_currentUnit);
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
        Vector2 des = _currentUnit.transform.position;
        foreach (var unit in _combineUnitList)
        {
            unit.StartCombine(des);
            _unitList.Remove(unit);
            if (unit == _combineUnitList[_combineUnitList.Count - 1])
            {
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
        _currentUnit.SetData(_currentUnit.NextLevel, _currentUnit.Special);

        // 更新分数
        ScoreManager.Instance.UpdateScore = true;

        // 产生下一个物体
        GenerateUnit();
    }

    // 移除地块
    public void EraseUnit(Tile tile)
    {
        // 如果当前物体是消除操作，消除选中地块上的物体
        GF.MyPrint("=========== EraseUnit Began: " + tile.name);
    }

    // 检测可合并的物体
    public void CheckLinkSurround(Tile tile)
    {
        // 重置状态、清空合并列表、清空得分列表
        ResetCombineList();
        _combineUnitList.Clear();
        ScoreManager.Instance.ClearScoreList();

        // 重置_currentUnit的NextLevel和Special
        _currentUnit.NextLevel = _currentUnit.Level;
        _currentUnit.Special = 1;

        // 合并过程
        GF.MyPrint("=========== CheckLinkSurround Began: " + tile.name);
        LinkSurround(tile);
        GF.MyPrint("=========== CheckLinkSurround Ended: " + tile.name);

        // 合并完成，检查是否会合并
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
            _currentUnit.NextLevel++;           // 等级提升
            _currentUnit.Special = (_singleCombineUnitList.Count > 2 ? 2 : 1);// 是否超过3个合体，变为Special物体

            // 将分数加入分数列表准备做分步运算
            ScoreManager.Instance.AddToScoreList(_currentUnit.Score);

            // 递归检测下一等级的Unit是否会合体
            LinkSurround(_currentUnit.Tile);
        }
        else
        {
            GF.MyPrint("Remove unit which there is only one...\t" + _singleCombineUnitList.Count);
            _combineUnitList = _combineUnitList.Except(_singleCombineUnitList).ToList();

            // 没有升过级，得分为本身的分数
            if (_currentUnit.NextLevel == _currentUnit.Level)
            {
                ScoreManager.Instance.AddToScoreList(_currentUnit.Score);
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
                if (CheckIsInCombineList(unit) || unit == _currentUnit)
                {
                    GF.MyPrint(unit.name + " is already added");
                    continue;       // 已加入列表，跳过
                }
                if (unit.Level != _currentUnit.NextLevel)
                {
                    GF.MyPrint(unit.name + "\'s level is not match to " + "Current Unit\'s next level " + _currentUnit.NextLevel);
                    continue; // 等级不一样，跳过
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
        if (level == -1)    // 未指定等级，就用随机等级
        {
            level = RandomController.Range(GlobalValue.LevelList, GlobalValue.ProbabilityList);
        }
        Transform unitTr = Instantiate(unitPrefab);
        Unit unit = unitTr.GetComponent<Unit>();
        unit.SetParent(tile);
        unit.SetData(level);
        return unit;
    }
}
