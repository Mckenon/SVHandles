using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class Vector3HandleDisplay : ITypeHandleDisplay
    {
        public Type ExecutingType
        {
            get { return typeof(Vector3); }
        }

        public void Draw(SVArgs args, out object obj)
        {
            Vector3 inPoint = (args.Value as Vector3?).GetValueOrDefault();

            Vector3 outPoint = Handles.DoPositionHandle(inPoint, Quaternion.identity);

            obj = (outPoint != inPoint) ? (object)outPoint : null;
        }
    }
}