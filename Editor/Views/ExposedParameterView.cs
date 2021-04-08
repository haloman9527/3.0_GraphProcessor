using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;
using CZToolKit.Core.Blackboards;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace GraphProcessor.Editors
{
    public class ExposedParameterView : Blackboard
    {
        readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

        public BaseGraphView GraphView { get { return graphView as BaseGraphView; } }
        public Dictionary<string, VisualElement> fields = new Dictionary<string, VisualElement>();

        public ExposedParameterView(GraphView associatedGraphView) : base(associatedGraphView)
        {
            title = "Parameters";
            scrollable = true;
            styleSheets.Add(Resources.Load<StyleSheet>(exposedParameterViewStyle));
            this.addItemRequested += OnAddClicked;
            UpdateParameterList();
            this.editTextRequested = Rename;
            base.SetPosition(GraphView.GraphData.blackboardPosition);
        }

        private void Rename(Blackboard _blackboard, VisualElement _field, string _newName)
        {
            if (string.IsNullOrEmpty(_newName)) return;
            BlackboardField blackboardField = _field as BlackboardField;
            string oldParamName = blackboardField.text;
            if (!GraphView.GraphData.Blackboard.Rename(oldParamName, _newName))
                return;
            blackboardField.text = _newName;

            GraphView.GraphData.Blackboard.TryGetParam(_newName, out IBlackboardProperty property);
            IBlackboardPropertyGUID param = property as IBlackboardPropertyGUID;
            foreach (var item in GraphView.NodeViews.Values.OfType<ParameterNodeView>())
            {
                if ((item.NodeData as ParameterNode).paramGUID == param.GUID)
                    item.title = param.Name;
            }
        }

        protected virtual void OnAddClicked(Blackboard t)
        {
            var parameterType = new GenericMenu();

            foreach (var valueType in FieldFactory.FieldDrawersCache.Keys)
            {
                parameterType.AddItem(new GUIContent(valueType.Name), false, () =>
                {
                    string rawName = "New " + valueType.Name + "Param";
                    string name = rawName;

                    int i = 0;
                    while (GraphView.GraphData.Blackboard.Contains(name))
                    {
                        name = rawName + " " + i++;
                    }
                    AddParam(name, valueType);
                });
            }

            parameterType.ShowAsContext();
        }


        public void AddParam(string _name, Type _valueType)
        {
            if (FieldFactory.PropertyCreator.TryGetValue(_valueType, out Func<CZToolKit.Core.Blackboards.Blackboard, string, IBlackboardPropertyGUID> creator))
            {
                IBlackboardPropertyGUID property = creator(GraphView.GraphData.Blackboard, _name);
                GraphView.GraphData.Blackboard.AddProperty(property);
                AddParamField(property);
            }
        }

        public VisualElement AddParamField(IBlackboardPropertyGUID param)
        {
            VisualElement property = new VisualElement();
            BlackboardField blackboardField = new BlackboardField() { text = param.Name, typeText = param.PropertyType.Name, userData = param };
            property.Add(blackboardField);

            VisualElement fieldDrawer = FieldFactory.CreateField(param.PropertyType, param.Value, _newValue =>
            {
                param.Value = _newValue;
                if (param.Value != null)
                    blackboardField.typeText = param.Value.GetType().Name;
            }, "");
            BlackboardRow blackboardRow = new BlackboardRow(blackboardField, fieldDrawer);
            property.Add(blackboardRow);
            contentContainer.Add(property);
            fields[param.GUID] = property;
            return property;
        }

        public void RemoveField(BlackboardField blackboardField)
        {
            IBlackboardPropertyGUID param = blackboardField.userData as IBlackboardPropertyGUID;
            contentContainer.Remove(fields[param.GUID]);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            GraphView.GraphData.blackboardPosition = newPos;
            GraphView.RegisterCompleteObjectUndo("Modify ExposedParameterView");
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();
            foreach (var param in GraphView.GraphData.Blackboard.GetParams())
            {
                AddParamField(param as IBlackboardPropertyGUID);
            }
        }
    }
}