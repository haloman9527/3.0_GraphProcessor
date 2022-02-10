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
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeControlA : EdgeControl
    {
        // 构成曲线的点的个数(分辨率)
        public int resolution = 100;
        // 起点终点突出偏移
        public float nearCapOffset = 30;
        // 控点偏移，控制曲线曲率
        public float controlOffset = 100;

        Edge edgeView;
        bool pointsChanged;
        CurveInfo curveInfo = new CurveInfo();

        public EdgeControlA(Edge connectionView)
        {
            this.edgeView = connectionView;
        }

        protected override void PointsChanged()
        {
            base.PointsChanged();
            ComputeControlPoints();
            pointsChanged = true;
            if (controlPoints == null)
                return;
            Vector2 vector = controlPoints[0];
            Vector2 vector2 = controlPoints[1];
            Vector2 vector3 = controlPoints[2];
            Vector2 vector4 = controlPoints[3];
            if (edgeView.input?.node == edgeView.output?.node && inputOrientation == outputOrientation)
            {
                Vector2 fixedDis = Vector2.zero;
                float xDis = vector2.x - vector3.x;
                float xN = xDis >= 0 ? 1 : -1;
                float yDis = vector2.y - vector3.y;
                float yN = yDis >= 0 ? 1 : -1;
                var a = 50;
                switch (inputOrientation)
                {
                    case Orientation.Horizontal:
                        var absyDis = yDis * yN;
                        if (absyDis <= controlOffset && vector.x > vector4.x && xDis < controlOffset * 2)
                        {
                            var per = absyDis <= a ? absyDis / a : 1 - (absyDis - a) / (controlOffset - a) * (Mathf.Max(0, xDis) / controlOffset);
                            fixedDis = per * new Vector3(0, controlOffset * 3) * yN;
                        }
                        break;
                    case Orientation.Vertical:
                        var absxDis = xDis * xN;
                        if (absxDis <= controlOffset && vector.y > vector4.y && yDis < controlOffset * 2)
                        {
                            var per = absxDis <= a ? absxDis / a : 1 - (absxDis - a) / (controlOffset - a) * (Mathf.Max(0, yDis) / controlOffset);
                            fixedDis = per * new Vector3(controlOffset * 3, 0) * xN;
                        }
                        break;
                    default:
                        break;
                }
                vector2 += fixedDis;
                vector3 += fixedDis;
            }

            Vector2 inputDirection = inputOrientation == Orientation.Horizontal ? Vector2.left : Vector2.up;
            Vector2 outputDirection = outputOrientation == Orientation.Horizontal ? Vector2.right : Vector2.down;

            vector2 += inputDirection * nearCapOffset * 2;
            vector3 += outputDirection * nearCapOffset * 2;

            curveInfo.points.Clear();
            for (float i = 0; i < resolution; i++)
            {
                curveInfo.points.Add(GetPoint(vector, vector2, vector3, vector4, i / resolution));
            }
            curveInfo.SetDirty();
        }

        protected override void UpdateRenderPoints()
        {
            if (edgeView.input?.node != edgeView.output?.node)
            {
                base.UpdateRenderPoints();
                return;
            }

            if (!(bool)(RenderPointsDirtyField.GetValue(this)) && controlPoints != null)
                return;
            if (!pointsChanged)
                return;
            pointsChanged = false;

            var renderPoints = RenderPointsField.GetValue(this) as List<Vector2>;
            renderPoints.Clear();
            renderPoints.AddRange(curveInfo.points);

            for (int i = 0; i < renderPoints.Count; i++)
            {
                renderPoints[i] -= layout.position;
            }
        }

        public override void UpdateLayout()
        {
            if (edgeView.input?.node != edgeView.output?.node)
            {
                base.UpdateLayout();
                return;
            }

            Rect r = new Rect(curveInfo.minX, curveInfo.minY, curveInfo.maxX - curveInfo.minX, curveInfo.maxY - curveInfo.minY);
            r.xMin -= 5;
            r.xMax += 5;
            r.yMin -= 5;
            r.yMax += 5;

            LayoutProperty.SetValue(this, r);
        }

        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        #region Static
        static FieldInfo RenderPointsDirtyField;
        static FieldInfo RenderPointsField;
        static PropertyInfo LayoutProperty;

        static EdgeControlA()
        {
            RenderPointsDirtyField = typeof(EdgeControl).GetField("m_RenderPointsDirty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            RenderPointsField = typeof(EdgeControl).GetField("m_RenderPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            LayoutProperty = typeof(EdgeControl).GetProperty("layout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        #endregion

        #region Define
        public class CurveInfo
        {
            public float minX = float.MaxValue, minY = float.MaxValue;
            public float maxX = float.MinValue, maxY = float.MinValue;
            public List<Vector2> points = new List<Vector2>();

            public void SetDirty()
            {
                minX = float.MaxValue;
                minY = float.MaxValue;
                maxX = float.MinValue;
                maxY = float.MinValue;
                foreach (var point in points)
                {
                    minX = Mathf.Min(point.x, minX);
                    maxX = Mathf.Max(point.x, maxX);
                    minY = Mathf.Min(point.y, minY);
                    maxY = Mathf.Max(point.y, maxY);
                }
            }
        }
        #endregion
    }
}
#endif