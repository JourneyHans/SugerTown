using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameMain : MonoBehaviour 
{
    // 地面
    public Ground ground;

    void Start()
    {
        // 初始化
        ground.InitUnits();

        // 产生物体
        DOVirtual.DelayedCall(0.1f, ground.GenerateUnit);
    }
}
