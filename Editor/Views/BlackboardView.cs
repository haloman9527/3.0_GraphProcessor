using CZToolKit.Core;
using CZToolKit.Core.Blackboards;
using CZToolKit.Core.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BlackboardView : Blackboard
    {
        public BaseGraphView GraphView { get { return graphView as BaseGraphView; } }
        public Dictionary<string, VisualElement> fields = new Dictionary<string, VisualElement>();

        public BlackboardView(GraphView associatedGraphView) : base(associatedGraphView)
        {
            title = "Blackboard";
            subTitle = "";
            scrollable = true;
            addItemRequested = OnAddClicked;
            editTextRequested = Rename;
            base.SetPosition(GraphView.Graph.BlackboardPosition);
            style.display = GraphView.Graph.BlackboardVisible ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateParameterList();
        }

        private void Rename(Blackboard _blackboard, VisualElement _field, string _newName)
        {
            if (string.IsNullOrEmpty(_newName)) return;
            BlackboardField blackboardField = _field as BlackboardField;
            if (!GraphView.Graph.Blackboard.Rename(blackboardField.text, _newName)) return;

            string oldName = blackboardField.text;
            blackboardField.text = _newName;
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
                Type valueType = Utility_Reflection.GetFieldInfo(dataType, "value").FieldType;
                if (!typeof(UnityEngine.Object).IsAssignableFrom(valueType) && !UIElementsFactory.FieldDrawerCreatorMap.ContainsKey(valueType))
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
                fields[_name] = AddParamView(_name, data);
            }
        }

        public VisualElement AddParamView(string _name, ICZType _data)
        {
            VisualElement property = new VisualElement();
            BlackboardField blackboardField = new BlackboardField() { text = _name, typeText = _data.ValueType.Name, userData = _data };
            property.Add(blackboardField);

            VisualElement fieldDrawer = UIElementsFactory.CreateField("", _data.ValueType, _data.GetValue(), _newValue =>
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
                    fields[kv.Key] = AddParamView(kv.Key, data);
            }
        }
    }
}