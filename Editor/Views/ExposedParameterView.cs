using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace CZToolKit.GraphProcessor.Editors
{
    public class ExposedParameterView : Blackboard
    {
        public BaseGraphView GraphView { get { return graphView as BaseGraphView; } }
        public Dictionary<string, VisualElement> fields = new Dictionary<string, VisualElement>();

        public ExposedParameterView(GraphView associatedGraphView) : base(associatedGraphView)
        {
            title = "Parameters";
            scrollable = true;
            UpdateParameterList();
            addItemRequested += OnAddClicked;
            editTextRequested = Rename;
        }

        private void Rename(Blackboard _blackboard, VisualElement _field, string _newName)
        {
            if (string.IsNullOrEmpty(_newName)) return;
            BlackboardField blackboardField = _field as BlackboardField;
            string oldParamName = blackboardField.text;
            if (!GraphView.GraphData.RenameExposeParameter(oldParamName, _newName))
                return;
            blackboardField.text = _newName;

            GraphView.GraphData.TryGetExposedParameterFromName(_newName, out ExposedParameter param);
            foreach (var item in GraphView.NodeViews.Values.OfType<ParameterNodeView>())
            {
                if ((item.NodeData as ParameterNode).paramGUID == param.GUID)
                    item.title = param.Name;
            }
        }

        protected virtual void OnAddClicked(Blackboard t)
        {
            GenericMenu parameterTypes = new GenericMenu();

            foreach (var valueType in FieldFactory.PropertyCreatorMap.Keys)
            {
                parameterTypes.AddItem(new GUIContent(valueType.Name), false, () =>
                {
                    string rawName = "New " + valueType.Name + "Param";
                    string name = rawName;

                    int i = 0;
                    while (GraphView.GraphData.TryGetExposedParameterFromName(name, out ExposedParameter param))
                    {
                        name = rawName + " " + i++;
                    }
                    AddParam(name, valueType);
                });
            }

            parameterTypes.ShowAsContext();
        }


        public void AddParam(string _name, Type _valueType)
        {
            if (FieldFactory.PropertyCreatorMap.TryGetValue(_valueType, out Func<string, Type, ExposedParameter> creator))
            {
                ExposedParameter property = creator(_name, _valueType);
                GraphView.GraphData.AddExposedParameter(property);
                AddParamField(property);
            }
        }

        public VisualElement AddParamField(ExposedParameter _param)
        {
            VisualElement property = new VisualElement();
            BlackboardField blackboardField = new BlackboardField() { text = _param.Name, typeText = _param.ValueType.Name, userData = _param };
            property.Add(blackboardField);

            VisualElement fieldDrawer = FieldFactory.CreateField("", _param.ValueType, _param.Value, _newValue =>
             {
                 _param.Value = _newValue;
                 if (_param.Value != null)
                     blackboardField.typeText = _param.Value.GetType().Name;
             });
            BlackboardRow blackboardRow = new BlackboardRow(blackboardField, fieldDrawer);
            property.Add(blackboardRow);
            contentContainer.Add(property);
            fields[_param.GUID] = property;
            return property;
        }

        public void RemoveField(BlackboardField blackboardField)
        {
            ExposedParameter param = blackboardField.userData as ExposedParameter;
            contentContainer.Remove(fields[param.GUID]);
        }

        public override void UpdatePresenterPosition()
        {
            base.UpdatePresenterPosition();
            GraphView.RegisterCompleteObjectUndo("Modify ExposedParameterView");
            GraphView.GraphData.blackboardPosition = GetPosition();
            GraphView.SetDirty();
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();
            foreach (var param in GraphView.GraphData.Blackboard)
            {
                AddParamField(param.Value);
            }
        }
    }
}