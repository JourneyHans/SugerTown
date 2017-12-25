using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarBand : MonoBehaviour 
{
	void Start () 
    {
        //Humanoid human = new Humanoid();
        Enemy enemy = new Enemy();
        //Orc orc = new Orc();

        //human.Yell();
        //print("-----------");
        enemy.Yell();
        //print("-----------");
        //orc.Yell();
	}
}
