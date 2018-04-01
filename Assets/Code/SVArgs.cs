using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct SVArgs
{
    public MonoBehaviour MonoInstance;
    public object Value;

    public SVArgs(object value, MonoBehaviour monoInstance)
    {
        Value = value;
        MonoInstance = monoInstance;
    }
}