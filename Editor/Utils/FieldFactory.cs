using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using CZToolKit.Core.Blackboards;

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


        public static Dictionary<Type, Func<Blackboard, string, IBlackboardPropertyGUID>> PropertyCreator = new Dictionary<Type, Func<Blackboard, string, IBlackboardPropertyGUID>>();

        static FieldFactory()
        {

            foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
            {
                var drawerAttribute = type.GetCustomAttributes(typeof(FieldDrawerAttribute), false).FirstOrDefault() as FieldDrawerAttribute;
                if (drawerAttribute == null) continue;
                AddDrawer(drawerAttribute.fieldType, type);
            }

            AddDrawer<bool, Toggle>(false);
            AddDrawer<int, IntegerField>(0);
            AddDrawer<long, LongField>(0);
            AddDrawer<float, FloatField>(0);
            AddDrawer<double, DoubleField>(0);
            AddDrawer<string, TextField>("");
            AddDrawer<Bounds, BoundsField>(new Bounds());
            AddDrawer<Color, ColorField>(new Color());
            AddDrawer<Vector2, Vector2Field>(new Vector2());
            AddDrawer<Vector2Int, Vector2IntField>(new Vector2Int());
            AddDrawer<Vector3, Vector3Field>(new Vector3());
            AddDrawer<Vector3Int, Vector3IntField>(new Vector3Int());
            AddDrawer<Vector4, Vector4Field>(new Vector4());
            AddDrawer<AnimationCurve, CurveField>(new AnimationCurve());
            //AddDrawer<Enum, EnumField>();
            AddDrawer<Gradient, GradientField>(new Gradient());
            AddDrawer<UnityEngine.Object, ObjectField>(new UnityEngine.Object());
            AddDrawer<Rect, RectField>(new Rect());

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

        static void AddDrawer<F, D>(F _defaultValue)
        {
            Type fieldType = typeof(F);
            Type drawerType = typeof(D);

            var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(fieldType);
            if (!iNotifyType.IsAssignableFrom(drawerType))
            {
                Debug.LogWarning("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< " + fieldType + " >");
                return;
            }

            PropertyCreator[typeof(F)] = (bb, name) =>
            {
                BlackboardPropertyGUID<F> property = new BlackboardPropertyGUID<F>();
                property.Name = name;
                property.TValue = _defaultValue;
                return property;
            };
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

            return fieldDrawer;
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