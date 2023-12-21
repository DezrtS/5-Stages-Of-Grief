using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    public static void PrintList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Debug.Log(item);
        }
    }
}