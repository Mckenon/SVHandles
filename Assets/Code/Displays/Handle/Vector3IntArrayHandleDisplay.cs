using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class Vector3IntArrayHandleDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Vector3Int[]); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Vector3Int[] inPoints = value as Vector3Int[];

            for (int i = 0; i < inPoints.Length; i++)
            {
                if (SceneViewHandles.DoFade(inPoints[i]))
                    continue;
                inPoints[i] = Vector3Int.RoundToInt(Handles.DoPositionHandle(inPoints[i], Quaternion.identity));
            }

            value = inPoints;
        }
    }
}