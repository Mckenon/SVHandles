using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct DrawDebugArgs
{
    public MonoBehaviour MonoInstance;
    public object Value;

    public DrawDebugArgs(object value, MonoBehaviour monoInstance)
    {
        Value = value;
        MonoInstance = monoInstance;
    }
}