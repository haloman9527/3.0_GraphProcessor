using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

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

        private void Rename(Blackboard arg1, VisualElement arg2, string arg3)
        {
            if (string.IsNullOrEmpty(arg3)) return;
            BlackboardField blackboardField = arg2 as BlackboardField;
            string oldParamName = blackboardField.text;
            ExposedParameter param = GraphView.GraphData.GetExposedParameterFromName(oldParamName);
            GraphView.GraphData.RenameParameter(param, arg3);
            blackboardField.text = arg3;

            foreach (var item in GraphView.NodeViews.Values.OfType<ParameterNodeView>())
            {
                if ((item.NodeData as ParameterNode).paramGUID == param.GUID)
                {
                    item.title = param.Name;
                }
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

                    object value = null;
                    if (valueType.IsValueType)
                        value = Activator.CreateInstance(valueType);

                    string name = rawName;
                    int i = 0;
                    VisualElement view = null;
                    do
                    {
                        view = AddParam(name, valueType, value);
                        name = rawName + " " + i++;
                    } while (view == null);
                });
            }

            parameterType.ShowAsContext();
        }

        public VisualElement AddParam(string _name, Type _valueType, object _value)
        {
            ExposedParameter param = GraphView.GraphData.AddExposedParameter(_name, _valueType, _value);
            if (param == null) return null;
            return AddParamField(param);
        }

        public VisualElement AddParamField(ExposedParameter param)
        {
            VisualElement property = new VisualElement();
            BlackboardField blackboardField = new BlackboardField() { text = param.Name, typeText = param.ValueType.Name, userData = param };
            property.Add(blackboardField);

            VisualElement fieldDrawer = FieldFactory.CreateField(param.ValueType, param.Value, _newValue =>
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
            ExposedParameter param = blackboardField.userData as ExposedParameter;
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
            foreach (var param in GraphView.GraphData.GetParameters())
            {
                AddParamField(param);
            }
        }
    }
}