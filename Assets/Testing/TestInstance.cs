using System.Collections;
using System.Collections.Generic;
using SVHandles.Displays.Handle;
using UnityEngine;

public class TestInstance : MonoBehaviour
{
    [SVHandle]
    private Vector3[] Testa = new Vector3[]
    {
        new Vector3(-2,0,2),
        new Vector3(0,0,2),
        new Vector3(2,0,2)
    };

    [SVHandle(typeof(Vector3ArrayHandleDisplay_Line))]
    private Vector3[] Testb = new Vector3[]
    {
        new Vector3(-2,0,-2),
        new Vector3(0,0,-2),
        new Vector3(2,0,-2)
    };

    [SVHandle] public Bounds MyBounds = new Bounds(Vector3.zero, Vector3.one);
}