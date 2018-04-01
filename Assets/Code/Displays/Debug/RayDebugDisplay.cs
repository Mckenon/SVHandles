using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Debug
{
    public class RayDebugDisplay : ITypeDebugDisplay
    {
        public Type ExecutingType
        {
            get { return typeof(Ray); }
        }

        public void Draw(SVArgs args)
        {
            Ray? ray = args.Value as Ray?;

            if (ray == null)
                return;

            Handles.ArrowHandleCap(0, ray.Value.origin, Quaternion.LookRotation(ray.Value.direction), 1f, EventType.Repaint);
        }
    }
}