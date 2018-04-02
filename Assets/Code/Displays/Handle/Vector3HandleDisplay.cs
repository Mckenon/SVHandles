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

        public object Draw(SVArgs args)
        {
            Vector3 inPoint = (args.Value as Vector3?).GetValueOrDefault();

            if (SceneViewHandles.DoFade(inPoint))
                return null;

            Vector3 outPoint = Handles.DoPositionHandle(inPoint, Quaternion.identity);

            return (outPoint != inPoint) ? (object)outPoint : null;
        }
    }
}