using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class Vector3ArrayHandleDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Vector3[]); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Vector3[] inPoints = value as Vector3[];

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < inPoints.Length; i++)
            {
                if (SceneViewHandles.DoFade(inPoints[i]))
                    continue;
                inPoints[i] = Handles.DoPositionHandle(inPoints[i], Quaternion.identity);
            }

            value = inPoints;
        }
    }
}