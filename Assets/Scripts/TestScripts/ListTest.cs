using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ListTest : MonoBehaviour 
{
    List<int> _firstList = new List<int>()
    {
        1, 2, 3, 4, 5, 6
    };
    List<int> _secondList = new List<int>()
    {
        1, 3, 5, 7
    };

	void Start () 
    {
        foreach (var v in _firstList)
        {
            print(v);
        }

        _firstList = _firstList.Except(_secondList).ToList();

        print("事后...");
        foreach (var v in _firstList)
        {
            print(v);
        }
    }
}
