using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomController
{
    public static int Range(int[] targets, float[] probabilities)
    {
        float minus = Random.Range(0f, 1f);
        int index = 0;
        do
        {
            minus -= probabilities[index];
            ++index;
        } while (minus > 0);
        return targets[--index];
    }
}
