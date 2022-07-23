#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class FlowPoint : Manipulator
    {
        VisualElement point { get; set; }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target is Edge edge)
            {
                point = new VisualElement();
                point.AddToClassList("flow-point");
                point.style.left = 0;
                point.style.top = 0;
                target.Add(point);

                target.schedule.Execute(() =>
                {
                    UpdateCapPoint(edge, (float)(EditorApplication.timeSinceStartup % 3 / 3));
                }).Until(() => point == null);
            }
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (point != null)
            {
                target.Remove(point);
                point = null;
            }
        }

        public void UpdateCapPoint(Edge edgeView, float t)
        {
            Vector3 v = Lerp(edgeView.edgeControl.controlPoints, t);
            point.style.left = v.x;
            point.style.top = v.y;
        }

        Vector2 Lerp(Vector2[] points, float t)
        {
            t = Mathf.Clamp01(t);
            float totalLength = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                totalLength += Vector2.Distance(points[i], points[i + 1]);
            }

            float pointLength = Mathf.Lerp(0, totalLength, t);

            float tempLength = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                float d = Vector2.Distance(points[i], points[i + 1]);
                if (pointLength <= tempLength + d)
                    return Vector2.Lerp(points[i], points[i + 1], (pointLength - tempLength) / d);
                tempLength += d;
            }
            return points[0];
        }
    }
}