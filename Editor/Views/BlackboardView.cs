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
    public sealed class BlackboardView : Blackboard, IBindableView
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

            // ��
            BindingProperties();

            UpdateParameterList();
        }

        #region ���ݼ����ص�
        void OnPositionChanged(Rect _position)
        {
            base.SetPosition(_position);
            GraphView.SetDirty();
        }
        void OnVisibleChanged(bool _visible)
        {
            style.display = _visible ? DisplayStyle.Flex : DisplayStyle.None;
            GraphView.SetDirty();
        }
        void OnBlackboardDataAdded(string _dataName, ICZType _data)
        {
            AddFieldView(_dataName, _data);
            GraphView.SetDirty();
        }
        void OnBlackboardDataRemoved(string _dataName)
        {
            RemoveFieldView(_dataName);
            GraphView.SetDirty();
        }
        void OnBlackboardDataRenamed(string _oldName, string _newName)
        {
            BlackboardRow blackboardRow = fields[_oldName];
            (blackboardRow.Q(className: "blackboardField") as BlackboardField).text = _newName;
            fields.Remove(_oldName);
            fields[_newName] = blackboardRow;
            MarkDirtyRepaint();
        }
        void BindingProperties()
        {
            // ��ʼ��
            base.SetPosition(GraphView.Model.BlackboardPosition);
            style.display = GraphView.Model.BlackboardVisible ? DisplayStyle.Flex : DisplayStyle.None;

            GraphView.Model.BindingProperty<bool>(BaseGraph.BLACKBOARD_VISIBLE_NAME, OnVisibleChanged);
            GraphView.Model.BindingProperty<Rect>(BaseGraph.BLACKBOARD_POSITION_NAME, OnPositionChanged);
            GraphView.Model.onBlackboardDataAdded += OnBlackboardDataAdded;
            GraphView.Model.onBlackboardDataRemoved += OnBlackboardDataRemoved;
            GraphView.Model.onBlackboardDataRenamed += OnBlackboardDataRenamed;
        }
        public void UnBindingProperties()
        {
            GraphView.Model.UnBindingProperty<bool>(BaseGraph.BLACKBOARD_VISIBLE_NAME, OnVisibleChanged);
            GraphView.Model.UnBindingProperty<Rect>(BaseGraph.BLACKBOARD_POSITION_NAME, OnPositionChanged);
            GraphView.Model.onBlackboardDataAdded -= OnBlackboardDataAdded;
            GraphView.Model.onBlackboardDataRemoved -= OnBlackboardDataRemoved;
            GraphView.Model.onBlackboardDataRenamed -= OnBlackboardDataRenamed;
        }
        #endregion

        void Rename(Blackboard _blackboard, VisualElement _field, string _newName)
        {
            BlackboardField blackboardField = _field as BlackboardField;
            GraphView.Model.RenameData_BB(blackboardField.text, _newName);
        }

        void OnAddClicked(Blackboard t)
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
            BindableElement fieldDrawer = UIElementsFactory.CreateField("", _data.ValueType, _data.GetValue(), _newValue =>
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

        void UpdateParameterList()
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