using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ground : MonoBehaviour 
{
    public Transform tilePrefab;       // 地块
    public Transform unitPrefab;       // 物体

    private int _row = 6;
    private int _col = 6;
    private float _tileWidth;
    private float _tileHeight;
    private Unit _currentUnit;      // 当前物体
    private bool _isTouchBegan;     // 开始点击
    private bool _isCanceled;       // 取消
    private bool _touchEnabled;     // 是否开启触摸

    // 假设先初始化7个点
    private const int _spawnNum = 7;

    // 地块列表
    private List<Tile> _tileList = new List<Tile>();

    // 物体列表
    private List<Unit> _unitList = new List<Unit>();

    // 当前合并的物体列表
    private List<Unit> _combineUnitList = new List<Unit>();

    private void Awake()
    {
        _tileWidth = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        _tileHeight = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void Start()
    {
        // 初始化地块
        for (int c = 0; c < _col; c++)
        {
            for (int r = 0; r < _row; r++)
            {
                Transform tileTr = Object.Instantiate(tilePrefab);
                tileTr.SetParent(transform);
                tileTr.localPosition = new Vector3(r * _tileWidth, c * _tileHeight);
                tileTr.name = "Tile_" + (c * _col + r + 1);
                Tile tile = tileTr.GetComponent<Tile>();
                tile.SetPosition(r, c);
                _tileList.Add(tile);

                // 注册点击事件
                EventTriggerListener.Get(tileTr.gameObject).onDown = OnTouchBegan;
                EventTriggerListener.Get(tileTr.gameObject).onUp = OnTouchEnded;
                EventTriggerListener.Get(tileTr.gameObject).onEnter = OnMoveEnter;
                EventTriggerListener.Get(tileTr.gameObject).onExit = OnMoveOut;
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

    // 玩家手指按下操作
    private void OnTouchBegan(GameObject go, PointerEventData eventData)
    {
        _isTouchBegan = true;

        Tile tile = go.GetComponent<Tile>();
        if (tile.Unit == _currentUnit || tile.IsEmpty())
        {
            // 如果可以放置物体...
            CheckLinkSurround(tile);
            _currentUnit.SetParent(tile);
        }
        else
        {
            // 不能放置物体...
            tile.Unit.ShowCombineInfo();
        }
    }
    // 玩家手指移动入物体中
    private void OnMoveEnter(GameObject go, PointerEventData eventData)
    {
        if (_isTouchBegan)
        {
            Tile tile = go.GetComponent<Tile>();
            if (tile.Unit == _currentUnit || tile.IsEmpty())
            {
                // 如果可以放置物体...先将“取消”置为false
                _isCanceled = false;
                CheckLinkSurround(tile);
                GF.MyPrint("Now Tile is: " + tile.name);
                _currentUnit.SetParent(tile);
                GF.MyPrint("Current Parent is: " + _currentUnit.Tile.name);
            }
            else
            {
                // 不能放置物体...
                tile.Unit.ShowCombineInfo();
            }
        }
    }
    // 玩家手指移出物体
    private void OnMoveOut(GameObject go, PointerEventData eventData)
    {
        if (_isTouchBegan)
        {
            // 每次移出Tile都将“取消”置为True，如果玩家移入另一个物体，将置回false
            _isCanceled = true;
            foreach (var unit in _combineUnitList)
            {
                unit.ResetState();
            }
        }
    }
    // 玩家手指抬起
    private void OnTouchEnded(GameObject go, PointerEventData eventData)
    {
        _isTouchBegan = false;
        if (_isCanceled)
        {
            return;
        }

        Tile tile = go.GetComponent<Tile>();
        GF.MyPrint("OnTouchEnded: " + tile.name);
        // 如果是空地，放置当前物体
        // 如果不是空地，检测是否可以移除物体
        if (tile.Unit == _currentUnit || tile.IsEmpty())
        {
            PlaceUnit();

            // 产生物体
            DOVirtual.DelayedCall(0.1f, GenerateUnit);
        }
        else
        {
            EraseUnit(tile);
        }
    }

    // 放置物体
    private void PlaceUnit()
    {
        _currentUnit.Place();
        _unitList.Add(_currentUnit);
        if (_combineUnitList.Count >= 2)
        {
            // 合体！
            DoCombination();
        }
    }

    // 合体
    private void DoCombination()
    {
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
        _combineUnitList.Clear();
    }

    // 点击地块
    private void EraseUnit(Tile tile)
    {
        // 如果当前物体是消除操作，消除选中地块上的物体

    }

    // 检测可合并的物体
    private void CheckLinkSurround(Tile tile)
    {
        // 重置状态、清空合并列表
        foreach (var unit in _combineUnitList)
        {
            unit.ResetState();
        }
        _combineUnitList.Clear();

        // 合并过程
        LinkSurround(tile);

        // 合并完成，检查是否会合并
        if (_combineUnitList.Count >= 2)
        {
            GF.MyPrint(_combineUnitList.Count);
            foreach (var unit in _combineUnitList)
            {
                unit.ReadyToCombine(tile.transform.position);
            }
        }
    }

    // 链接可以合并的物体(递归)
    private void LinkSurround(Tile tile)
    {
        DoLink(tile);
    }

    private void DoLink(Tile tile)
    {
        int start_x = tile.X - 1;
        int end_x = tile.X + 1; 
        start_x = start_x < 0 ? tile.X : start_x;
        end_x = end_x > _row - 1 ? tile.X : end_x;
        GF.MyPrint("++++++++ Begin Link Surround");
        GF.MyPrint("Origin: (" + tile.X + ", " + tile.Y + ")");
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
                GF.MyPrint("Surround : (" + x + ", " + y + ")");
                Tile surroundTile = GetTile(x, y);
                if (surroundTile.IsEmpty())
                {
                    GF.MyPrint("(" + x + ", " + y + ") is Empty");
                    continue;       // 如果是空的，跳过
                }
                Unit unit = surroundTile.Unit;
                if (CheckIsInCombineList(unit) || unit == _currentUnit)
                {
                    GF.MyPrint("(" + x + ", " + y + ") is already added");
                    continue;       // 已加入列表，跳过
                }
                if (unit.Level != _currentUnit.Level)
                {
                    GF.MyPrint("(" + x + ", " + y + ") is not the same level. " + unit.name + " level: " + unit.Level);
                    continue; // 等级不一样，跳过
                }
                GF.MyPrint("(" + x + ", " + y + ") will be added");
                _combineUnitList.Add(unit);
                GF.MyPrint("++++++++ End Link Surround");
                LinkSurround(surroundTile);
            }
        }
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
            level = Random.Range(1, 4);
        }
        Transform unitTr = Instantiate(unitPrefab);
        Unit unit = unitTr.GetComponent<Unit>();
        unit.SetParent(tile);
        unit.SetData(level, tile.X, tile.Y);
        return unit;
    }
}
