using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SVHandleDisplay
{
    public virtual int Priority
    {
        get { return -1;}
    }
    public abstract Type ExecutingType { get; }
    public abstract void Draw(SVArgs args, ref object val);
}