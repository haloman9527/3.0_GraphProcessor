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
using CZToolKit.Core.Editors;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseNode))]
    public partial class BaseNodeView
    {
        protected virtual void OnInitialized() { }

        protected virtual void OnBindingProperties() { }

        protected virtual void OnUnBindingProperties() { }

        protected virtual BasePortView NewPortView(BasePortVM port)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(port.ModelType), port, new EdgeConnectorListener()) as BasePortView;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(ViewModel.GetType()))
            {
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }
            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(ViewModel.Model.GetType()))
            {
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }
            evt.menu.AppendSeparator();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
        }

        public void HighlightOn()
        {
            nodeBorder.AddToClassList("highlight");
        }

        public void HighlightOff()
        {
            nodeBorder.RemoveFromClassList("highlight");
        }

        public void Flash()
        {
            HighlightOn();
            schedule.Execute(_ => { HighlightOff(); }).ExecuteLater(2000);
        }

        //public override void SetPosition(Rect newPos)
        //{
        //    if (GridSnap > 0)
        //    {
        //        var x = newPos.x % GridSnap;
        //        x = x < 0 ? GridSnap + x : x;
        //        newPos.x = newPos.x - x + (int)(x / GridSnap * 2) * GridSnap;

        //        var y = newPos.y % GridSnap;
        //        y = y < 0 ? GridSnap + y : y;
        //        newPos.y = newPos.y - y + (int)(y / GridSnap * 2) * GridSnap;
        //    }
        //    base.SetPosition(newPos);
        //}

        public void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.RightCenter);
        }

        public void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b =>
            {
                if (callback(b))
                {
                    b.Detach();
                    b.RemoveFromHierarchy();
                    return true;
                }
                return false;
            });
        }
    }

    public class BaseNodeView<M> : BaseNodeView where M : BaseNodeVM
    {
        public M T_ViewModel { get { return base.ViewModel as M; } }
    }
}
#endif