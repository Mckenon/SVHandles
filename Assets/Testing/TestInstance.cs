using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInstance : MonoBehaviour
{
    [SVHandle]
    public Vector3 MyPoint = new Vector3(0, 10, 0);

    [SVDebug(1f, 0f, 0f)]
    public Vector3 TestDynamic = new Vector3(0, 10, 10);

    [SVDebug]
    public Ray TestRay = new Ray(new Vector3(0, 0, 0), Vector3.forward);

    private float t = 0f;
    private void Update()
    {
        t += Time.deltaTime;
        TestDynamic = new Vector3(Mathf.Cos(t) * 1f, 0, Mathf.Sin(t) * 1f);
        TestRay.direction = (TestDynamic).normalized;
    }
}