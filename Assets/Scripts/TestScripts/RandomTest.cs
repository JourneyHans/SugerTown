using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTest : MonoBehaviour 
{
    int _times = 10000;
	void Start () 
    {
        int[] targets = { 1, 2, 3, 4 };
        float[] probabilities = { 0.7f, 0.2f, 0.08f, 0.02f };

        int[] times = {0, 0, 0, 0};

        for (int i = 0; i < _times; i++)
        {
            int result = RandomController.Range(targets, probabilities);
            times[result - 1]++;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            print(targets[i] + " 出现了 " + times[i] + " 次" + ", 概率为 " + (float)times[i] / _times * 100 + "%");
        }
    }
}
