using System;
using UnityEngine;

/// <summary>
/// Attribute used to draw debug information in scene view.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DrawDebug : Attribute
{
    public Color Color;

    public DrawDebug()
    {
        Color = Color.white;
    }

    public DrawDebug(float r, float g, float b)
    {
        Color = new Color(r, g, b);
    }

    public DrawDebug(float r, float g, float b, float a)
    {
        Color = new Color(r, g, b, a);
    }
}