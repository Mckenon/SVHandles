using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Debug
{
    public class RayDebugDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Ray); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Ray? ray = value as Ray?;

            Handles.ArrowHandleCap(0, ray.Value.origin, Quaternion.LookRotation(ray.Value.direction), 1f, EventType.Repaint);
        }
    }
}