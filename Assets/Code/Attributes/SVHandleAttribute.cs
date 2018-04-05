using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attribute used to draw debug information in scene view.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SVHandleAttribute : Attribute
{
    public Color Color;
    public Type Type = null;

    public bool MyBool = false;

    public SVHandleAttribute()
    {
        Color = Color.white;
    }

    public SVHandleAttribute(Type type)
    {
        Type = type;
    }

    public SVHandleAttribute(float r, float g, float b)
    {
        Color = new Color(r, g, b);
    }

    public SVHandleAttribute(float r, float g, float b, float a)
    {
        Color = new Color(r, g, b, a);
    }
}