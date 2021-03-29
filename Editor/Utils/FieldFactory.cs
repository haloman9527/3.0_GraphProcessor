using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace GraphProcessor.Editors
{
    public static class FieldFactory
    {
        public delegate VisualElement DrawerCreator(Type fieldType, object value, Action<object> onValueChanged, string label);
        public delegate Type TypeProcessor(Type _originalType);

        public static readonly Dictionary<Type, Type> FieldDrawersCache = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, DrawerCreator> SpecialFieldDraweCreator = new Dictionary<Type, DrawerCreator>();
        static readonly Dictionary<Type, TypeProcessor> FieldTypeProcessorsCache = new Dictionary<Type, TypeProcessor>();

        static readonly MethodInfo createFieldMethod = typeof(FieldFactory).GetMethod("CreateFieldSpecific", BindingFlags.Static | BindingFlags.Public);

        static FieldFactory()
        {

            foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
            {
                var drawerAttribute = type.GetCustomAttributes(typeof(FieldDrawerAttribute), false).FirstOrDefault() as FieldDrawerAttribute;
                if (drawerAttribute == null) continue;
                AddDrawer(drawerAttribute.fieldType, type);
            }

            AddDrawer<bool, Toggle>();
            AddDrawer<int, IntegerField>();
            AddDrawer<long, LongField>();
            AddDrawer<float, FloatField>();
            AddDrawer<double, DoubleField>();
            AddDrawer<string, TextField>();
            AddDrawer<Bounds, BoundsField>();
            AddDrawer<Color, ColorField>();
            AddDrawer<Vector2, Vector2Field>();
            AddDrawer<Vector2Int, Vector2IntField>();
            AddDrawer<Vector3, Vector3Field>();
            AddDrawer<Vector3Int, Vector3IntField>();
            AddDrawer<Vector4, Vector4Field>();
            AddDrawer<AnimationCurve, CurveField>();
            AddDrawer<Enum, EnumField>();
            AddDrawer<Gradient, GradientField>();
            AddDrawer<UnityEngine.Object, ObjectField>();
            AddDrawer<Rect, RectField>();

            SpecialFieldDraweCreator[typeof(LayerMask)] = (fieldType, value, onValueChanged, label) =>
            {
                // LayerMasks inherit from INotifyValueChanged<int> instead of INotifyValueChanged<LayerMask>
                // so we can't register it inside our factory system :(
                var layerField = new LayerMaskField(label, ((LayerMask)value).value);
                layerField.RegisterValueChangedCallback(e =>
                {
                    onValueChanged(new LayerMask { value = e.newValue });
                });
                return layerField;
            };
        }

        static void AddDrawer(Type fieldType, Type drawerType)
        {
            var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(fieldType);

            if (!iNotifyType.IsAssignableFrom(drawerType))
            {
                Debug.LogWarning("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< " + fieldType + " >");
                return;
            }

            FieldDrawersCache[fieldType] = drawerType;
        }

        static void AddDrawer<F, D>()
        {
            Type fieldType = typeof(F);
            Type drawerType = typeof(D);

            var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(fieldType);
            if (!iNotifyType.IsAssignableFrom(drawerType))
            {
                Debug.LogWarning("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< " + fieldType + " >");
                return;
            }

            FieldDrawersCache[fieldType] = drawerType;
        }

        public static INotifyValueChanged<T> CreateField<T>(T value, string label = null)
        {
            return CreateField(value != null ? value.GetType() : typeof(T), label) as INotifyValueChanged<T>;
        }

        public static VisualElement CreateField(Type t, string label)
        {
            Type drawerType;

            FieldDrawersCache.TryGetValue(t, out drawerType);

            if (drawerType == null)
                drawerType = FieldDrawersCache.FirstOrDefault(kp => kp.Key.IsReallyAssignableFrom(t)).Value;

            if (drawerType == null)
            {
                Debug.LogWarning("Can't find field drawer for type: " + t);
                return null;
            }

            // Call the constructor that have a label
            object field;

            if (drawerType == typeof(EnumField))
            {
                field = new EnumField(label, Activator.CreateInstance(t) as Enum);
            }
            else
            {
                try
                {
                    field = Activator.CreateInstance(drawerType,
                        BindingFlags.CreateInstance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.OptionalParamBinding, null,
                        new object[] { label, Type.Missing }, CultureInfo.CurrentCulture);
                }
                catch
                {
                    field = Activator.CreateInstance(drawerType,
                        BindingFlags.CreateInstance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.OptionalParamBinding, null,
                        new object[] { label }, CultureInfo.CurrentCulture);
                }
            }

            // For mutiline
            switch (field)
            {
                case TextField textField:
                    textField.multiline = true;
                    break;
                case ObjectField objField:
                    objField.allowSceneObjects = true;
                    objField.objectType = typeof(UnityEngine.Object);
                    break;
            }

            return field as VisualElement;
        }

        public static INotifyValueChanged<T> CreateFieldSpecific<T>(T value, Action<object> onValueChanged, string label)
        {
            var fieldDrawer = CreateField<T>(value, label);

            if (fieldDrawer == null)
                return null;

            fieldDrawer.value = value;
            fieldDrawer.RegisterValueChangedCallback((e) =>
            {
                onValueChanged(e.newValue);
            });

            return fieldDrawer as INotifyValueChanged<T>;
        }

        public static VisualElement CreateField(Type fieldType, object value, Action<object> onValueChanged, string label)
        {
            if (typeof(Enum).IsAssignableFrom(fieldType))
                fieldType = typeof(Enum);

            VisualElement field = null;

            // Handle special cases here
            if (fieldType == typeof(LayerMask))
            {
                // LayerMasks inherit from INotifyValueChanged<int> instead of INotifyValueChanged<LayerMask>
                // so we can't register it inside our factory system :(
                var layerField = new LayerMaskField(label, ((LayerMask)value).value);
                layerField.RegisterValueChangedCallback(e =>
                {
                    onValueChanged(new LayerMask { value = e.newValue });
                });

                field = layerField;
            }
            else
            {
                try
                {

                    var createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(fieldType);
                    try
                    {
                        field = createFieldSpecificMethod.Invoke(null, new object[] { value, onValueChanged, label }) as VisualElement;
                    }
                    catch { }

                    // handle the Object field case
                    if (field == null && (value == null || value is UnityEngine.Object))
                    {
                        createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(typeof(UnityEngine.Object));
                        field = createFieldSpecificMethod.Invoke(null, new object[] { value, onValueChanged, label }) as VisualElement;
                        if (field is ObjectField objField)
                        {
                            objField.objectType = fieldType;
                            objField.value = value as UnityEngine.Object;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return field;
        }
    }
}