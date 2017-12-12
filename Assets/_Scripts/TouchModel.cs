using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchModel : MonoBehaviour 
{
    private Ground ground;

    private bool _touchEnabled = true;     // 是否开启触摸
    public bool TouchEnabled
    {
        get { return _touchEnabled; }
        set { _touchEnabled = value; }
    }
    private bool _isTouchBegan;     // 开始点击
    private bool _isCanceled;       // 取消

	void Start () 
    {
        ground = GetComponent<Ground>();
	}

    public void AddTouchEvent(GameObject target)
    {
        EventTriggerListener.Get(target).onDown = OnTouchBegan;
        EventTriggerListener.Get(target).onUp = OnTouchEnded;
        EventTriggerListener.Get(target).onEnter = OnMoveEnter;
        EventTriggerListener.Get(target).onExit = OnMoveOut;
    }

    // 玩家手指按下操作
    private void OnTouchBegan(GameObject go, PointerEventData eventData)
    {
        if (!_touchEnabled) { return; }

        _isTouchBegan = true;

        Tile tile = go.GetComponent<Tile>();
        if (tile.Unit == ground.CurrentUnit || tile.IsEmpty())
        {
            // 如果可以放置物体...
            ground.CurrentUnit.SetParent(tile);
            ground.CheckLinkSurround(tile);
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
            if (tile.Unit == ground.CurrentUnit || tile.IsEmpty())
            {
                // 如果可以放置物体...先将“取消”置为false
                _isCanceled = false;
                ground.CurrentUnit.SetParent(tile);
                ground.CheckLinkSurround(tile);
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
            ground.ResetCombineList();
        }
    }
    // 玩家手指抬起
    private void OnTouchEnded(GameObject go, PointerEventData eventData)
    {
        if (_isTouchBegan)
        {
            _isTouchBegan = false;
            if (_isCanceled)
            {
                return;
            }

            Tile tile = go.GetComponent<Tile>();
            // 如果是空地，放置当前物体
            // 如果不是空地，检测是否可以移除物体
            if (tile.Unit == ground.CurrentUnit || tile.IsEmpty())
            {
                ground.PlaceUnit();
            }
            else
            {
                ground.EraseUnit(tile);
            }
        }
    }
}
