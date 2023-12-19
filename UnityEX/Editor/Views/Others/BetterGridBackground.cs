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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BetterGridBackground : GridBackground
    {
        private static CustomStyleProperty<Color> s_CenterLineColorProperty = new CustomStyleProperty<Color>("--center-line-color");

        private static readonly Color s_DefaultCenterLineColor = new Color(0f, 0.91f, 1f);

        private Color m_CenterLineColor = BetterGridBackground.s_DefaultCenterLineColor;
        private Color centerLineColor => this.m_CenterLineColor;

        private VisualElement m_Container;

        public BetterGridBackground() : base()
        {
            this.RegisterCallback<CustomStyleResolvedEvent>(new EventCallback<CustomStyleResolvedEvent>(this.OnCustomStyleResolved1));
        }

        private void OnCustomStyleResolved1(CustomStyleResolvedEvent e)
        {
            ICustomStyle customStyle = e.customStyle;
            if (customStyle.TryGetValue(BetterGridBackground.s_CenterLineColorProperty, out var centerLineColor))
                this.m_CenterLineColor = centerLineColor;
        }

        protected override void ImmediateRepaint()
        {
            base.ImmediateRepaint();

            this.m_Container = this.parent is UnityEditor.Experimental.GraphView.GraphView parent ? parent.contentViewContainer : throw new InvalidOperationException("GridBackground can only be added to a GraphView");
            Rect layout1 = m_Container.layout;
            layout1.x = 0;
            layout1.y = 0;

            var matrix = m_Container.transform.matrix;

            Vector2 hMin = new Vector3(layout1.x, layout1.y + layout1.height / 2);
            Vector2 hMax = new Vector3(layout1.xMax, layout1.y + layout1.height / 2);
            
            hMin.y += layout1.y * hMin.y;
            hMin = m_Container.WorldToLocal(hMin);
            hMax = m_Container.WorldToLocal(hMax);
            hMin.y = 0;
            hMax.y = 0;
            hMin = m_Container.LocalToWorld(hMin);
            hMax = m_Container.LocalToWorld(hMax);
            
            if (hMin.y >= layout1.yMin && hMax.y <= layout.yMax)
            {
                GL.Begin(1);
                GL.Color(centerLineColor);
                GL.Vertex(hMin);
                GL.Vertex(hMax);
                GL.End();
            }
        }
    }
}