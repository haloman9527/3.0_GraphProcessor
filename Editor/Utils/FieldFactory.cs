using CZToolKit.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace GraphProcessor.Editors
{
    public static class FieldFactory
    {
        static readonly MethodInfo createFieldMethod = typeof(FieldFactory).GetMethod("CreateFieldSpecific", BindingFlags.Static | BindingFlags.Public);

        public static readonly Dictionary<Type, Type> FieldDrawersCache = new Dictionary<Type, Type>();
        public static readonly Dictionary<Type, Func<string, ExposedParameter>> PropertyCreatorMap = new Dictionary<Type, Func<string, ExposedParameter>>();
        public static readonly Dictionary<Type, Func<VisualElement>> FieldDrawerCreatorMap = new Dictionary<Type, Func<VisualElement>>();

        static FieldFactory()
        {
            foreach (var drawerType in TypeCache.GetTypesWithAttribute<FieldDrawerAttribute>())
            {
                if (AttributeCache.TryGetTypeAttribute(drawerType, out FieldDrawerAttribute fieldDrawerAttribute))
                    AddDrawer(fieldDrawerAttribute.fieldType, drawerType);
            }

            AddDrawer(typeof(Enum), typeof(EnumField));
            AddDrawer<bool, Toggle>(() => { return false; },
                () => { return new Toggle(); });
            AddDrawer<int, IntegerField>(() => { return 0; },
                () => { return new IntegerField(); });
            AddDrawer<long, LongField>(() => { return 0; },
                () => { return new LongField(); });
            AddDrawer<float, FloatField>(() => { return 0; },
                () => { return new FloatField(); });
            AddDrawer<double, DoubleField>(() => { return 0; },
                () => { return new DoubleField(); });
            AddDrawer<string, TextField>(() => { return ""; },
                () => { return new TextField() { multiline = true }; });

            AddDrawer<LayerMask, LayerMaskField>(() => { return 0; },
                () => { return new LayerMaskField(); });
            AddDrawer<Rect, RectField>(() => { return new Rect(); },
                () => { return new RectField(); });
            AddDrawer<Bounds, BoundsField>(() => { return new Bounds(); },
                () => { return new BoundsField(); });
            AddDrawer<BoundsInt, BoundsIntField>(() => { return new BoundsInt(); },
                () => { return new BoundsIntField(); });
            AddDrawer<Color, ColorField>(() => { return new Color(); },
                () => { return new ColorField(); });
            AddDrawer<Vector2, Vector2Field>(() => { return new Vector2(); },
                () => { return new Vector2Field(); });
            AddDrawer<Vector2Int, Vector2IntField>(() => { return new Vector2Int(); },
                () => { return new Vector2IntField(); });
            AddDrawer<Vector3, Vector3Field>(() => { return new Vector3(); },
                () => { return new Vector3Field(); });
            AddDrawer<Vector3Int, Vector3IntField>(() => { return new Vector3Int(); },
                () => { return new Vector3IntField(); });
            AddDrawer<Vector4, Vector4Field>(() => { return new Vector4(); },
                () => { return new Vector4Field(); });
            AddDrawer<AnimationCurve, CurveField>(() => { return new AnimationCurve(); },
                () => { return new CurveField(); });
            AddDrawer<Gradient, GradientField>(() => { return new Gradient(); },
                () => { return new GradientField(); });
            AddDrawer<UnityEngine.Object, ObjectField>(() => { return new UnityEngine.Object(); },
                () =>
                {
                    return new ObjectField() { allowSceneObjects = true, objectType = typeof(UnityEngine.Object) };
                });
        }

        static void AddDrawer(Type _fieldType, Type _drawerType)
        {
            var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(_fieldType);

            if (!iNotifyType.IsAssignableFrom(_drawerType))
            {
                Debug.LogWarning("The custom field drawer " + _drawerType + " does not implements INotifyValueChanged< " + _fieldType + " >");
                return;
            }

            FieldDrawersCache[_fieldType] = _drawerType;
            FieldDrawerCreatorMap[_fieldType] = () => { return Activator.CreateInstance(_drawerType) as VisualElement; };
        }

        static void AddDrawer<F, D>(Func<F> _defaultValueGetter, Func<VisualElement> _fieldDrawerCreator) where D : VisualElement, new()
        {
            Type fieldType = typeof(F);
            Type drawerType = typeof(D);

            PropertyCreatorMap[fieldType] = (name) =>
            {
                return new ExposedParameter(name, _defaultValueGetter());
            };

            FieldDrawersCache[fieldType] = drawerType;
            FieldDrawerCreatorMap[fieldType] = _fieldDrawerCreator;
        }

        public static VisualElement CreateField(Type _fieldType, string _label)
        {
            object fieldDrawer = null;
            if (_fieldType == typeof(Enum))
                fieldDrawer = new EnumField(_label, Activator.CreateInstance(_fieldType) as Enum);
            else if (FieldDrawerCreatorMap.TryGetValue(_fieldType, out Func<VisualElement> drawerCreator))
            {
                fieldDrawer = drawerCreator();
            }
            return fieldDrawer as VisualElement;
        }

        public static INotifyValueChanged<T> CreateField<T>(T value, string label = null)
        {
            return CreateField(typeof(T), label) as INotifyValueChanged<T>;
        }

        public static INotifyValueChanged<T> CreateFieldSpecific<T>(T _value, Action<object> _onValueChanged, string _label)
        {
            var fieldDrawer = CreateField(_value, _label);
            if (fieldDrawer == null)
                return null;
            BaseField<T> tDrawer = fieldDrawer as BaseField<T>;
            tDrawer.labelElement.style.minWidth = 48;
            tDrawer.labelElement.style.width = 54;
            tDrawer.label = _label;
            fieldDrawer.value = _value;
            fieldDrawer.RegisterValueChangedCallback((e) =>
            {
                _onValueChanged(e.newValue);
            });
            return fieldDrawer;
        }

        public static VisualElement CreateField(Type _fieldType, object _value, Action<object> _onValueChanged, string _label)
        {
            if (typeof(Enum).IsAssignableFrom(_fieldType))
                _fieldType = typeof(Enum);

            VisualElement fieldDrawer = null;

            if (_fieldType == typeof(LayerMask))
            {
                var layerField = new LayerMaskField(_label, ((LayerMask)_value).value);
                layerField.RegisterValueChangedCallback(e =>
                {
                    _onValueChanged(new LayerMask { value = e.newValue });
                });
                layerField.labelElement.style.minWidth = 48;
                layerField.labelElement.style.width = 54;
                fieldDrawer = layerField;
            }
            else
            {
                var createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(_fieldType);
                try
                {
                    fieldDrawer = createFieldSpecificMethod.Invoke(null, new object[] { _value, _onValueChanged, _label }) as VisualElement;
                }
                catch { }
            }

            return fieldDrawer;
        }
    }
}