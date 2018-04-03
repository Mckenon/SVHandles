using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class Vector3HandleDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Vector3); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Vector3 inPoint = (value as Vector3?).GetValueOrDefault();

            if (SceneViewHandles.DoFade(inPoint))
                return;

            value = Handles.DoPositionHandle(inPoint, Quaternion.identity);
        }
    }
}