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
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using PortViewOrientation = UnityEditor.Experimental.GraphView.Orientation;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BetterEdgeControl : EdgeControl
    {
        private Edge edgeView;
        private bool pointsChanged;
        private Rect range;
        private List<Vector2> points = new List<Vector2>();

        public BetterEdgeControl(Edge connectionView)
        {
            this.edgeView = connectionView;
        }

        protected override void PointsChanged()
        {
            base.PointsChanged();
            pointsChanged = true;

            if (edgeView.input?.node == edgeView.output?.node)
            {
                ComputeControlPoints();
                var vector0 = controlPoints[0];
                var vector1 = controlPoints[1];
                var vector2 = controlPoints[2];
                var vector3 = controlPoints[3];

                points.Clear();
                points.Add(vector0);

                if (inputOrientation == outputOrientation && outputOrientation == PortViewOrientation.Horizontal)
                {
                    var y = Mathf.Max(vector1.y, vector2.y);
                    var c1 = new Vector2(vector1.x, y + 10);
                    var c2 = new Vector2(vector2.x, c1.y);

                    points.Add(vector1 + new Vector2(-1, 0));
                    points.Add(vector1 + new Vector2(-0.7f, 0.3f));
                    points.Add(vector1 + new Vector2(-0.5f, 0.5f));
                    points.Add(vector1 + new Vector2(-0.3f, 0.7f));
                    points.Add(vector1 + new Vector2(0, 1));

                    points.Add(c1 + new Vector2(0, -1));
                    points.Add(c1 + new Vector2(-0.3f, -0.7f));
                    points.Add(c1 + new Vector2(-0.5f, -0.5f));
                    points.Add(c1 + new Vector2(-0.7f, -0.3f));
                    points.Add(c1 + new Vector2(-1, 0));

                    points.Add(c2 + new Vector2(1, 0));
                    points.Add(c2 + new Vector2(0.7f, -0.3f));
                    points.Add(c2 + new Vector2(0.5f, -0.5f));
                    points.Add(c2 + new Vector2(0.3f, -0.7f));
                    points.Add(c2 + new Vector2(0, -1));

                    points.Add(vector2 + new Vector2(0, 1));
                    points.Add(vector2 + new Vector2(0.3f, 0.7f));
                    points.Add(vector2 + new Vector2(0.5f, 0.5f));
                    points.Add(vector2 + new Vector2(0.7f, 0.3f));
                    points.Add(vector2 + new Vector2(1, 0));
                }
                else if (inputOrientation == outputOrientation && outputOrientation == PortViewOrientation.Vertical)
                {
                    var x = Mathf.Max(vector1.x, vector2.x);
                    var c1 = new Vector2(x + 10, vector1.y);
                    var c2 = new Vector2(c1.x, vector2.y);

                    points.Add(vector1 + new Vector2(0, -1));
                    points.Add(vector1 + new Vector2(0.3f, -0.7f));
                    points.Add(vector1 + new Vector2(0.5f, -0.5f));
                    points.Add(vector1 + new Vector2(0.5f, -0.3f));
                    points.Add(vector1 + new Vector2(1, 0));

                    points.Add(c1 + new Vector2(-1, 0));
                    points.Add(c1 + new Vector2(-0.7f, -0.3f));
                    points.Add(c1 + new Vector2(-0.5f, -0.5f));
                    points.Add(c1 + new Vector2(-0.3f, -0.7f));
                    points.Add(c1 + new Vector2(0, -1));

                    points.Add(c2 + new Vector2(0, 1));
                    points.Add(c2 + new Vector2(-0.3f, 0.7f));
                    points.Add(c2 + new Vector2(-0.5f, 0.5f));
                    points.Add(c2 + new Vector2(-0.7f, 0.3f));
                    points.Add(c2 + new Vector2(-1, 0));

                    points.Add(vector2 + new Vector2(1, 0));
                    points.Add(vector2 + new Vector2(0.7f, 0.3f));
                    points.Add(vector2 + new Vector2(0.5f, 0.5f));
                    points.Add(vector2 + new Vector2(0.3f, 0.7f));
                    points.Add(vector2 + new Vector2(0, 1));
                }
                else if (inputOrientation != outputOrientation && outputOrientation == PortViewOrientation.Horizontal)
                {
                    var c = new Vector2(vector1.x, vector2.y);
                    points.Add(vector1);
                    points.Add(c);
                    points.Add(vector2);
                    points.Add(vector3);
                }
                else if (inputOrientation != outputOrientation && outputOrientation == PortViewOrientation.Vertical)
                {
                    var c = new Vector2(vector2.x, vector1.y);
                    points.Add(vector1);
                    points.Add(c);
                    points.Add(vector2);
                    points.Add(vector3);
                }

                points.Add(vector3);

                range.xMin = int.MaxValue;
                range.xMax = int.MinValue;
                range.yMin = int.MaxValue;
                range.yMax = int.MinValue;
                foreach (var point in points)
                {
                    range.xMin = Mathf.Min(point.x, range.xMin);
                    range.xMax = Mathf.Max(point.x, range.xMax);
                    range.yMin = Mathf.Min(point.y, range.yMin);
                    range.yMax = Mathf.Max(point.y, range.yMax);
                }
            }
        }

        protected override void UpdateRenderPoints()
        {
            if (edgeView.input?.node != edgeView.output?.node)
            {
                base.UpdateRenderPoints();
                return;
            }
            else
            {
                if (pointsChanged)
                {
                    pointsChanged = false;

                    var renderPoints = RenderPointsField.GetValue(this) as List<Vector2>;
                    renderPoints.Clear();
                    renderPoints.AddRange(points);

                    for (int i = 0; i < renderPoints.Count; i++)
                    {
                        renderPoints[i] -= layout.position;
                    }
                }
            }
        }

        public override void UpdateLayout()
        {
            if (edgeView.input?.node != edgeView.output?.node)
            {
                base.UpdateLayout();
                return;
            }

            var r = range;
            r.xMin -= 5;
            r.xMax += 5;
            r.yMin -= 5;
            r.yMax += 5;

            LayoutProperty.SetValue(this, r);
        }

        #region Static

        static FieldInfo RenderPointsDirtyField;
        static FieldInfo RenderPointsField;
        static PropertyInfo LayoutProperty;

        static BetterEdgeControl()
        {
            RenderPointsDirtyField = typeof(EdgeControl).GetField("m_RenderPointsDirty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            RenderPointsField = typeof(EdgeControl).GetField("m_RenderPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            LayoutProperty = typeof(EdgeControl).GetProperty("layout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        #endregion
    }
}
#endif