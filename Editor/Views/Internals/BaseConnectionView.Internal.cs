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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using CZToolKit.Core;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseConnectionView : Edge, IBindableView<BaseConnection>
    {
        public Label IndexLabel { get; private set; }
        public BaseConnection Model { get; private set; }
        protected BaseGraphView Owner { get; private set; }

        public BaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
            IndexLabel = new Label();
            IndexLabel.style.display = DisplayStyle.None;
            IndexLabel.style.flexGrow = 1;
            IndexLabel.style.fontSize = 20;
            IndexLabel.style.color = new Color(1, 0.5f, 0, 1);
            IndexLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            edgeControl.Add(IndexLabel);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        public void SetUp(BaseConnection connection, BaseGraphView graphView)
        {
            Model = connection;
            Owner = graphView;
        }

        public virtual void UnBindingProperties()
        {

        }

        public void ShowIndex(int index)
        {
            IndexLabel.text = index.ToString();
            IndexLabel.style.display = DisplayStyle.Flex;
            BringToFront();
        }

        public void HideIndex()
        {
            IndexLabel.text = "";
            IndexLabel.style.display = DisplayStyle.None;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }
    }

    public class EdgeControl1 : EdgeControl
    {
        float controlDis = 25;
        float dis = 100;
        bool pointsChanged;
        Rect r;
        List<Vector2> points = new List<Vector2>();

        public BaseConnectionView connectionView;

        public EdgeControl1(BaseConnectionView connectionView)
        {
            this.connectionView = connectionView;
        }

        protected override void PointsChanged()
        {
            base.PointsChanged();
            ComputeControlPoints();
            pointsChanged = true;
            //Vector2 vector = base.parent.ChangeCoordinatesTo(this, controlPoints[0]);
            //Vector2 vector2 = base.parent.ChangeCoordinatesTo(this, controlPoints[1]);
            //Vector2 vector3 = base.parent.ChangeCoordinatesTo(this, controlPoints[2]);
            //Vector2 vector4 = base.parent.ChangeCoordinatesTo(this, controlPoints[3]);
            if (controlPoints == null)
            {
                return;
            }
            Vector2 vector = controlPoints[0];
            Vector2 vector2 = controlPoints[1];
            Vector2 vector3 = controlPoints[2];
            Vector2 vector4 = controlPoints[3];
            if (connectionView.input?.node == connectionView.output?.node && inputOrientation == outputOrientation)
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
                        if (absyDis <= dis && vector.x > vector4.x && xDis < dis * 2)
                        {
                            var per = absyDis <= a ? absyDis / a : 1 - (absyDis - a) / (dis - a) * (Mathf.Max(0, xDis) / dis);
                            fixedDis = per * new Vector3(0, dis * 3) * yN;
                        }
                        break;
                    case Orientation.Vertical:
                        var absxDis = xDis * xN;
                        if (absxDis <= dis && vector.y > vector4.y && yDis < dis * 2)
                        {
                            var per = absxDis <= a ? absxDis / a : 1 - (absxDis - a) / (dis - a) * (Mathf.Max(0, yDis) / dis);
                            fixedDis = per * new Vector3(dis * 3, 0) * xN;
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

            vector2 += inputDirection * controlDis * 2;
            vector3 += outputDirection * controlDis * 2;

            int max = 100;
            points.Clear();
            for (float i = 0; i < max; i++)
            {
                Vector2 v = GetPoint(vector, vector2, vector3, vector4, i / max);
                points.Add(v);
            }
        }

        protected override void UpdateRenderPoints()
        {
            if (connectionView.input?.node != connectionView.output?.node)
            {
                base.UpdateRenderPoints();
                return;
            }

            if (!(bool)(Util_Reflection.GetFieldInfo(typeof(EdgeControl), "m_RenderPointsDirty").GetValue(this)) && controlPoints != null)
            {
                return;
            }
            if (!pointsChanged)
            {
                return;
            }
            pointsChanged = false; 


            var renderPoints = Util_Reflection.GetFieldInfo(typeof(EdgeControl), "m_RenderPoints").GetValue(this) as List<Vector2>;
            renderPoints.Clear();
            renderPoints.AddRange(points);
        }

        public override void UpdateLayout()
        {
            if (connectionView.input?.node != connectionView.output?.node)
            {
                base.UpdateLayout();
                return;
            }

            var renderPoints = Util_Reflection.GetFieldInfo(typeof(EdgeControl), "m_RenderPoints").GetValue(this) as List<Vector2>;

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var point in points)
            {
                minX = Mathf.Min(point.x, minX);
                maxX = Mathf.Max(point.x, maxX);
                minY = Mathf.Min(point.y, minY);
                maxY = Mathf.Max(point.y, maxY);
            }

            r = new Rect(minX, minY, maxX - minX, maxY - minY);
            r.xMin -= 5;
            r.xMax += 10;
            r.yMin -= 5;
            r.yMax += 5;

            Util_Reflection.GetPropertyInfo(typeof(EdgeControl), "layout").SetValue(this, r);
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
    }
}
#endif