using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SVHandles.Displays.Handle
{
    public class BoundsHandleDisplay : SVHandleDisplay
    {
        public override Type ExecutingType
        {
            get { return typeof(Bounds); }
        }

        public override void Draw(SVArgs args, ref object value)
        {
            Transform t = args.MonoInstance.transform;

            Bounds inBounds = (value as Bounds?).GetValueOrDefault();
            Bounds outBounds = inBounds;

            if (SceneViewHandles.DoFade(t.TransformPoint(inBounds.center)))
                return;

            outBounds.center = t.InverseTransformPoint(Handles.DoPositionHandle(t.TransformPoint(inBounds.center), Quaternion.identity));
            //outBounds.size = Handles.DoScaleHandle(inBounds.size, t.TransformPoint(outBounds.center), Quaternion.identity, 1f);

            Vector3 center = t.TransformPoint(outBounds.center);
            Vector3 extents = outBounds.extents;

            // Draw our handles for dragging scale.
            Vector3 outSize = outBounds.size;

            outSize.x = Handles.ScaleSlider(outSize.x, center + (Vector3.right * outSize.x / 2f), Vector3.right, Quaternion.LookRotation(Vector3.right), HandleUtility.GetHandleSize(center + (Vector3.right * outSize.x / 2f)), 1f);
            outSize.x = Handles.ScaleSlider(outSize.x, center + (Vector3.left * outSize.x / 2f), Vector3.left, Quaternion.LookRotation(Vector3.left), HandleUtility.GetHandleSize(center + (Vector3.left * outSize.x / 2f)), 1f);

            outSize.y = Handles.ScaleSlider(outSize.y, center + (Vector3.up * outSize.y / 2f), Vector3.up, Quaternion.LookRotation(Vector3.up), HandleUtility.GetHandleSize(center + (Vector3.up * outSize.y / 2f)), 1f);
            outSize.y = Handles.ScaleSlider(outSize.y, center + (Vector3.down * outSize.y / 2f), Vector3.down, Quaternion.LookRotation(Vector3.down), HandleUtility.GetHandleSize(center + (Vector3.down * outSize.y / 2f)), 1f);

            outSize.z = Handles.ScaleSlider(outSize.z, center + (Vector3.forward * outSize.z / 2f), Vector3.forward, Quaternion.LookRotation(Vector3.forward), HandleUtility.GetHandleSize(center + (Vector3.forward * outSize.z / 2f)), 1f);
            outSize.z = Handles.ScaleSlider(outSize.z, center + (Vector3.back * outSize.z / 2f), Vector3.back, Quaternion.LookRotation(Vector3.back), HandleUtility.GetHandleSize(center + (Vector3.back * outSize.z / 2f)), 1f);

            outBounds.size = outSize;

            Handles.DrawWireCube(t.TransformPoint(outBounds.center), outBounds.size);

            value = outBounds;
        }
    }
}