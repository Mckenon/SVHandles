using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attribute used to draw debug information in scene view.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SVHandle : Attribute
{
    public Color Color;

    public SVHandle()
    {
        Color = Color.white;
    }

    public SVHandle(float r, float g, float b)
    {
        Color = new Color(r, g, b);
    }

    public SVHandle(float r, float g, float b, float a)
    {
        Color = new Color(r, g, b, a);
    }
}