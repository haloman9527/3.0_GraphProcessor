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
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using System;
using Atom.UnityEditors;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public abstract partial class BaseNodeView
    {
        protected virtual BasePortView NewPortView(PortProcessor port)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(port.ModelType), port, new EdgeConnectorListener()) as BasePortView;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(ViewModel.GetType()))
            {
                if (script.GetClass() == null)
                {
                    continue;
                }
                if (!script.GetClass().IsSubclassOf(typeof(BaseNodeProcessor)))
                {
                    continue;
                }
                
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }

            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(ViewModel.Model.GetType()))
            {
                if (script.GetClass() == null)
                {
                    continue;
                }
                if (!script.GetClass().IsSubclassOf(typeof(BaseNode)))
                {
                    continue;
                }
                
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }

            evt.menu.AppendSeparator();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
        }

        public virtual bool CheckDrawControls()
        {
            return controls.childCount > 0;
        }

        protected virtual void DoInit()
        {
        }

        protected virtual void DoUnInit()
        {
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

        public void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);
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

    public class BaseNodeView<T> : BaseNodeView where T : BaseNodeProcessor
    {
        public T T_ViewModel
        {
            get { return base.ViewModel as T; }
        }
    }
}
#endif