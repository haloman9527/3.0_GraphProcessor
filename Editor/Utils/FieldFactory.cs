using CZToolKit.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.ComponentModel;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class FieldFactory
    {
        static readonly MethodInfo createFieldMethod = typeof(FieldFactory).GetMethod("CreateFieldSpecific", BindingFlags.Static | BindingFlags.Public);

        public static readonly Dictionary<Type, Func<string, Type, ExposedParameter>> PropertyCreatorMap = new Dictionary<Type, Func<string, Type, ExposedParameter>>();
        static readonly Dictionary<Type, Type> FieldDrawersCache = new Dictionary<Type, Type>();
        static readonly Dictionary<Type, Func<Type, VisualElement>> FieldDrawerCreatorMap = new Dictionary<Type, Func<Type, VisualElement>>();

        static FieldFactory()
        {
            //foreach (var drawerType in TypeCache.GetTypesWithAttribute<FieldDrawerAttribute>())
            //{
            //    if (Utility.TryGetTypeAttribute(drawerType, out FieldDrawerAttribute fieldDrawerAttribute))
            //        AddDrawer(fieldDrawerAttribute.fieldType, drawerType, _ => { return Activator.CreateInstance(drawerType) as VisualElement; });
            //}

            AddDrawer(typeof(Enum), typeof(EnumField), realType => { return new EnumField(Activator.CreateInstance(realType) as Enum); });

            AddDrawer<bool, Toggle>(_ => { return false; },
                _ => { return new Toggle(); });
            AddDrawer<int, IntegerField>(_ => { return 0; },
                _ => { return new IntegerField(); });
            AddDrawer<long, LongField>(_ => { return 0; },
                _ => { LongField drawer = new LongField(); drawer.style.width = 30; drawer.style.height = 16; return drawer; });
            AddDrawer<float, FloatField>(_ => { return 0; },
                _ => { return new FloatField(); });
            AddDrawer<double, DoubleField>(_ => { return 0; },
                _ => { DoubleField drawer = new DoubleField(); drawer.style.width = 30; drawer.style.paddingLeft = 0; drawer.style.paddingRight = 0; drawer.style.marginTop = 0; drawer.style.height = 18; return drawer; });
            AddDrawer<string, TextField>(_ => { return ""; },
                _ => { return new TextField() { multiline = true }; });

            AddDrawer<LayerMask, LayerMaskField>(_ => { return 0; },
                _ => { return new LayerMaskField(); });
            AddDrawer<Rect, RectField>(_ => { return new Rect(); },
                _ => { return new RectField(); });
            AddDrawer<Bounds, BoundsField>(_ => { return new Bounds(); },
                _ => { return new BoundsField(); });
            AddDrawer<BoundsInt, BoundsIntField>(_ => { return new BoundsInt(); },
                _ => { return new BoundsIntField(); });
            AddDrawer<Color, ColorField>(_ => { return new Color(); },
                _ => { return new ColorField(); });
            AddDrawer<Vector2, Vector2Field>(_ => { return new Vector2(); },
                _ => { return new Vector2Field(); });
            AddDrawer<Vector2Int, Vector2IntField>(_ => { return new Vector2Int(); },
                _ => { Vector2IntField drawer = new Vector2IntField(); drawer.style.width = 80; return drawer; });
            AddDrawer<Vector3, Vector3Field>(_ => { return new Vector3(); },
                _ => { return new Vector3Field(); });
            AddDrawer<Vector3Int, Vector3IntField>(_ => { return new Vector3Int(); },
                _ => { Vector3IntField drawer = new Vector3IntField(); drawer.style.width = 120; return drawer; });
            AddDrawer<Vector4, Vector4Field>(_ => { return new Vector4(); },
                _ => { Vector4Field drawer = new Vector4Field(); return drawer; });
            AddDrawer<AnimationCurve, CurveField>(_ => { return new AnimationCurve(); },
                _ => { return new CurveField(); });
            AddDrawer<Gradient, GradientField>(_ => { return new Gradient(); },
                _ => { return new GradientField(); });
            AddDrawer<UnityEngine.Object, ObjectField>(_ => { return null; },
                realType =>
                {
                    return new ObjectField() { allowSceneObjects = false, objectType = realType };
                });
        }

        static void AddDrawer(Type _fieldType, Type _drawerType, Func<Type, VisualElement> _fieldDrawerCreator)
        {
            var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(_fieldType);

            if (!iNotifyType.IsAssignableFrom(_drawerType))
            {
                Debug.LogWarning("The custom field drawer " + _drawerType + " does not implements INotifyValueChanged< " + _fieldType + " >");
                return;
            }

            FieldDrawersCache[_fieldType] = _drawerType;
            FieldDrawerCreatorMap[_fieldType] = _fieldDrawerCreator;
        }

        static void AddDrawer<F, D>(Func<Type, F> _defaultValueGetter, Func<Type, VisualElement> _fieldDrawerCreator) where D : VisualElement, new()
        {
            Type fieldType = typeof(F);
            Type drawerType = typeof(D);

            PropertyCreatorMap[fieldType] = (name, realType) =>
            {
                return new ExposedParameter(name, _defaultValueGetter(realType), fieldType);
            };

            FieldDrawersCache[fieldType] = drawerType;
            FieldDrawerCreatorMap[fieldType] = _fieldDrawerCreator;
        }

        public static INotifyValueChanged<F> CreateFieldSpecific<F>(string _label, F _value, Type _realFieldType, Action<object> _onValueChanged)
        {
            INotifyValueChanged<F> fieldDrawer = null;
            if (FieldDrawerCreatorMap.TryGetValue(typeof(F), out Func<Type, VisualElement> drawerCreator))
                fieldDrawer = drawerCreator(_realFieldType) as INotifyValueChanged<F>;

            if (fieldDrawer == null)
                return null;

            BaseField<F> tDrawer = fieldDrawer as BaseField<F>;
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

        public static VisualElement CreateField<T>(string _label, T _value, Action<object> _onValueChanged)
        {
            return CreateField(_label, typeof(T), _value, _onValueChanged);
        }

        public static VisualElement CreateField(string _label, Type _fieldType, object _value, Action<object> _onValueChanged)
        {
            Type realFieldType = _fieldType;

            if (!FieldDrawerCreatorMap.ContainsKey(_fieldType))
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(_fieldType))
                    _fieldType = typeof(UnityEngine.Object);
                else if (typeof(Enum).IsAssignableFrom(_fieldType) && !FieldDrawerCreatorMap.ContainsKey(_fieldType))
                    _fieldType = typeof(Enum);
            }

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
                    fieldDrawer = createFieldSpecificMethod.Invoke(null, new object[] { _label, _value, realFieldType, _onValueChanged }) as VisualElement;
                }
                catch { }
            }

            return fieldDrawer;
        }
    }
}