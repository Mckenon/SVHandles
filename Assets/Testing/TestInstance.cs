using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInstance : MonoBehaviour
{
    [SVHandle]
    private Vector3Int[] MyPoints = new Vector3Int[]
    {
        new Vector3Int(0,0,0),
        new Vector3Int(-2,0,0),
        new Vector3Int(2,0,0)
    };

    [SVHandle] public Bounds MyBounds = new Bounds(Vector3.zero, Vector3.one);
}