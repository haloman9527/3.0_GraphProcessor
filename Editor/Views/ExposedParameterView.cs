using CZToolKit.Core;
using CZToolKit.Core.Blackboards;
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
            string oldName = blackboardField.text;
            if (!GraphView.Graph.Blackboard.Rename(oldName, _newName))
                return;
            blackboardField.text = _newName;

            //GraphView.Graph.Blackboard.GUIDMap.TryGetValue(_newName, out string guid);
            foreach (var item in GraphView.NodeViews.Values.OfType<ParameterNodeView>())
            {
                if ((item.NodeData as ParameterNode).name == oldName)
                {
                    (item.NodeData as ParameterNode).name = _newName;
                    item.title = _newName;
                }
            }
        }

        protected virtual void OnAddClicked(Blackboard t)
        {
            GenericMenu menu = new GenericMenu();

            foreach (var dataType in CZTypeFactory.TypeCreator.Keys)
            {
                Type valueType = Utility_Refelection.GetFieldInfo(dataType, "value").FieldType;
                if (!typeof(UnityEngine.Object).IsAssignableFrom(valueType) && !FieldFactory.FieldDrawerCreatorMap.ContainsKey(valueType))
                    continue;
                menu.AddItem(new GUIContent(dataType.Name), false, () =>
                {
                    string rawName = "New " + dataType.Name + "Param";
                    string name = rawName;

                    int i = 0;
                    while (GraphView.Graph.Blackboard.TryGetData(name, out ICZType param))
                    {
                        name = rawName + " " + i++;
                    }
                    AddParam(name, dataType);
                });
            }

            menu.ShowAsContext();
        }


        public void AddParam(string _name, Type _dataType)
        {
            if (CZTypeFactory.TypeCreator.TryGetValue(_dataType, out Func<ICZType> creator))
            {
                ICZType data = creator();
                GraphView.Graph.Blackboard.SetData(_name, data);
                fields[_name] = AddParamField(_name, data);
            }
        }

        public VisualElement AddParamField(string _name, ICZType _data)
        {
            VisualElement property = new VisualElement();
            BlackboardField blackboardField = new BlackboardField() { text = _name, typeText = _data.ValueType.Name, userData = _data };
            property.Add(blackboardField);

            VisualElement fieldDrawer = FieldFactory.CreateField("", _data.ValueType, _data.GetValue(), _newValue =>
             {
                 _data.SetValue(_newValue);
                 if (_data.GetValue() != null)
                     blackboardField.typeText = _data.ValueType.Name;
             });
            BlackboardRow blackboardRow = new BlackboardRow(blackboardField, fieldDrawer);
            property.Add(blackboardRow);
            contentContainer.Add(property);
            return property;
        }

        public void RemoveField(BlackboardField blackboardField)
        {
            //ICZType param = blackboardField.userData as ExposedParameter;
            contentContainer.Remove(fields[blackboardField.text]);
            fields.Remove(blackboardField.text);
        }

        public override void UpdatePresenterPosition()
        {
            base.UpdatePresenterPosition();
            GraphView.RegisterCompleteObjectUndo("Modify ExposedParameterView");
            GraphView.Graph.BlackboardPosition = GetPosition();
            GraphView.SetDirty();
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();
            foreach (var kv in GraphView.Graph.Blackboard.GUIDMap)
            {
                if (GraphView.Graph.Blackboard.TryGetData(kv.Key, out ICZType data))
                    fields[kv.Key] = AddParamField(kv.Key, data);
            }
        }
    }
}