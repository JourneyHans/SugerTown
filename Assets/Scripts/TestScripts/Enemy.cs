using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Humanoid
{
    public override void Yell()
    {
        base.Yell();
        Debug.Log("Enemy version of the Yell() method");
    }
}
