using CZToolKit.Core;
using CZToolKit.Core.Blackboards;
using CZToolKit.Core.Editors;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BlackboardView : Blackboard
    {
        Dictionary<string, BlackboardRow> fields = new Dictionary<string, BlackboardRow>();

        public BaseGraphView GraphView { get { return graphView as BaseGraphView; } }
        public Dictionary<string, BlackboardRow> Fields { get { return fields; } }

        public BlackboardView(GraphView associatedGraphView) : base(associatedGraphView)
        {
            title = "Blackboard";
            subTitle = "";
            scrollable = true;
            addItemRequested = OnAddClicked;
            editTextRequested = Rename;

            UpdateParameterList();

            GraphView.Model.RegisterValueChangedEvent<Rect>(nameof(GraphView.Model.BlackboardPosition), v =>
            {
                base.SetPosition(v);
            });
            GraphView.Model.RegisterValueChangedEvent<bool>(nameof(GraphView.Model.BlackboardVisible), v =>
            {
                style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
            });
            GraphView.Model.onBlackboardDataAdded += (name, data) =>
            {
                AddFieldView(name, data);
            };
            GraphView.Model.onBlackboardDataRemoved += name =>
            {
                RemoveFieldView(name);
            };
            GraphView.Model.onBlackboardDataRenamed += (oldName, newName) =>
            {
                BlackboardRow blackboardRow = fields[oldName];
                (blackboardRow.Q(className: "blackboardField") as BlackboardField).text = newName;
                fields.Remove(oldName);
                fields[newName] = blackboardRow;
                MarkDirtyRepaint();
            };
        }

        void Rename(Blackboard _blackboard, VisualElement _field, string _newName)
        {
            BlackboardField blackboardField = _field as BlackboardField;
            GraphView.Model.RenameData_BB(blackboardField.text, _newName);
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
                    string name = dataType.Name;

                    int i = 0;
                    while (GraphView.Model.ContainsName_BB(name))
                    {
                        name = dataType.Name + " " + i++;
                    }
                    GraphView.Model.AddData_BB(name, CZTypeFactory.TypeCreator[dataType]());
                });
            }

            menu.ShowAsContext();
        }

        public void AddFieldView(string _name, ICZType _data)
        {
            BlackboardField blackboardField = new BlackboardField() { text = _name, typeText = _data.ValueType.Name, userData = _data };
            blackboardField.RegisterCallback<MouseEnterEvent>(evt =>
            {
                GraphView.nodes.ForEach(node =>
                {
                    if (node is ParameterNodeView parameterNodeView && parameterNodeView.T_Model.Name == blackboardField.text)
                    {
                        parameterNodeView.HighlightOn();
                    }
                });
            });

            blackboardField.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                GraphView.nodes.ForEach(node =>
                {
                    if (node is ParameterNodeView parameterNodeView && parameterNodeView.T_Model.Name == blackboardField.text)
                    {
                        parameterNodeView.HighlightOff();
                    }
                });
            });
            VisualElement fieldDrawer = UIElementsFactory.CreateField("", _data.ValueType, _data.GetValue(), _newValue =>
             {
                 _data.SetValue(_newValue);
                 if (_data.GetValue() != null)
                     blackboardField.typeText = _data.ValueType.Name;
             });
            BlackboardRow blackboardRow = new BlackboardRow(blackboardField, fieldDrawer);
            contentContainer.Add(blackboardRow);
            fields[_name] = blackboardRow;
        }

        public void RemoveFieldView(string _name)
        {
            contentContainer.Remove(fields[_name]);
            fields.Remove(_name);
        }

        public override void UpdatePresenterPosition()
        {
            base.UpdatePresenterPosition();
            GraphView.Model.BlackboardPosition = GetPosition();
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();
            foreach (var kv in GraphView.Model.Blackboard.GUIDMap)
            {
                if (GraphView.Model.TryGetData_BB(kv.Key, out ICZType data))
                    AddFieldView(kv.Key, data);
            }
        }
    }
}