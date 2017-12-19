using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour 
{
    // 简单单例
    public static ScoreManager Instance { get; private set; }

    // 总分数文本
    public Text scoreTxt;

    // 总分数
    private int _score;

    // 升级过程中的分数列表
    private Queue<int> _updateScoreList = new Queue<int>();

    // 开始更新分数
    [HideInInspector]
    public bool UpdateScore = false;
    private float refreshScoreInterval;       // 更新分数的间隔

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    void Start()
    {
        _score = 0;
        UpdateScoreTxt(_score);
	}

    // 添加到分数列表
    public void AddToScoreList(int score)
    {
        _updateScoreList.Enqueue(score);
    }

    // 清空分数列表
    public void ClearScoreList()
    {
        _updateScoreList.Clear();
    }

    // 更新函数
    private void Update()
    {
        if (UpdateScore)
        {
            if (_updateScoreList.Count > 0)
            {
                RefreshScore();
            }
            else
            {
                UpdateScore = false;
                refreshScoreInterval = 0;
            }
        }
    }

    // 更新分数
    private void RefreshScore()
    {
        if (refreshScoreInterval <= 0)
        {
            refreshScoreInterval = 0.3f;
            int score = _updateScoreList.Dequeue();
            AddScore(score);
        }
        else
        {
            refreshScoreInterval -= Time.deltaTime;
        }
    }

    private void AddScore(int score)
    {
        _score += score;
        UpdateScoreTxt(_score);
    }

    private void UpdateScoreTxt(int score)
    {
        scoreTxt.text = "Score: " + score;
    }
}
