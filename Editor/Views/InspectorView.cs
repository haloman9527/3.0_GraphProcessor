#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor
{
    public class InspectorView : VisualElement
    {
        public Resizer resizer;
        public VisualElement content;

        public InspectorView()
        {
            content = new VisualElement();
            content.StretchToParentSize();
            content.style.left = 10;
            content.style.flexGrow = 1;
            Add(content);
        }

        void OnResize()
        {
            
        }
    }
}
