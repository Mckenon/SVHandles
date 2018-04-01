using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITypeDebugDisplay
{
    /// <summary>
    /// The type which this Display will work for.
    /// </summary>
    Type ExecutingType { get; }
    void Draw(SVArgs args);
}