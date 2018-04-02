using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ITypeHandleDisplay
{
    /// <summary>
    /// The type which this Display will work for.
    /// </summary>
    Type ExecutingType { get; }
    object Draw(SVArgs args);
}