using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class Vector3IntArrayHandleDisplay : ITypeHandleDisplay
    {
        public Type ExecutingType
        {
            get { return typeof(Vector3Int[]); }
        }

        public object Draw(SVArgs args)
        {
            Vector3Int[] inPoints = args.Value as Vector3Int[];

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < inPoints.Length; i++)
            {
                if (SceneViewHandles.DoFade(inPoints[i]))
                    continue;
                inPoints[i] = Vector3Int.RoundToInt(Handles.DoPositionHandle(inPoints[i], Quaternion.identity));
            }

            if (EditorGUI.EndChangeCheck())
                return (object)inPoints;
            else
                return null;
        }
    }
}