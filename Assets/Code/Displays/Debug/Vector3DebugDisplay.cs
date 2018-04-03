using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays
{
    public class Vector3DebugDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Vector3); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Vector3? mVec = value as Vector3?;

            if (mVec == null)
                return;

            Handles.SphereHandleCap(0, mVec.Value, Quaternion.identity, 0.25f, EventType.Repaint);
        }
    }
}