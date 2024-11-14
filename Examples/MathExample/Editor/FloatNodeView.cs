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
using CZToolKit;
using CZToolKit.GraphProcessor.Editors;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomView(typeof(FloatNode))]
public class FloatNodeView : BaseNodeView
{
    public FloatField valueField;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var vm = ViewModel as FloatNodeProcessor;

        valueField = new FloatField();
        valueField.style.marginLeft = 3;
        valueField.style.marginRight = 3;
        // valueField.style.minWidth = 50;
        valueField.SetValueWithoutNotify(vm.Value);
        valueField.RegisterValueChangedCallback(OnEditedValue);
        controls.Add(valueField);
    }

    private void OnEditedValue(ChangeEvent<float> evt)
    {
        var vm = ViewModel as FloatNodeProcessor;
        vm.Value = evt.newValue;
    }

    protected override void OnBindingProperties()
    {
        base.OnBindingProperties();
        ViewModel.RegisterValueChanged<float>(nameof(FloatNode.num), OnFloatNumChanged);
    }

    protected override void OnUnBindingProperties()
    {
        base.OnUnBindingProperties();
        ViewModel.UnregisterValueChanged<float>(nameof(FloatNode.num), OnFloatNumChanged);
    }

    private void OnFloatNumChanged(float oldValue, float newValue)
    {
        valueField.SetValueWithoutNotify(newValue);
    }
}
#endif