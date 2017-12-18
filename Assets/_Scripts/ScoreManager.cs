using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour 
{
    public Text scoreTxt;
    private int score;

    public static ScoreManager Instance { get; private set; }

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
        score = 0;
        UpdateScoreTxt(score);
	}

    public void AddScore(int value)
    {
        score += value;
        UpdateScoreTxt(score);
    }

    private void UpdateScoreTxt(int value)
    {
        scoreTxt.text = "Score: " + value;
    }
}
