using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays
{
    public class Vector3DebugDisplay : ITypeDebugDisplay
    {
        public Type ExecutingType
        {
            get { return typeof(Vector3); }
        }

        public void Draw(SVArgs args)
        {
            Vector3? mVec = args.Value as Vector3?;

            if (mVec == null)
                return;

            Handles.SphereHandleCap(0, mVec.Value, Quaternion.identity, 0.25f, EventType.Repaint);
        }
    }
}