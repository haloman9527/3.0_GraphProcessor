using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace Moyo.GraphProcessor.Editors
{
    public class GraphProcessorEditorSettings
    {
        public static PrefsVariable<bool> MiniMapActive { get; } = new PrefsVariable<bool>(GetKey());

        public static string GetKey([CallerMemberName] string memberName = null)
        {
            return $"{nameof(GraphProcessorEditorSettings)}.{memberName}";
        }

        public class PrefsVariable<T>
        {
            private string key;
            private T value;
            public event Action<T> onValueChanged;

            public PrefsVariable(string key)
            {
                this.key = key;
                this.LoadValue();
            }

            public T Value
            {
                get { return value; }
                set
                {
                    if (EqualityComparer<T>.Default.Equals(this.value, value))
                    {
                        return;
                    }

                    this.value = value;
                    this.SaveValue();
                    this.onValueChanged?.Invoke(this.value);
                }
            }

            private void LoadValue()
            {
                if (typeof(T) == typeof(int))
                {
                    ((this as PrefsVariable<int>)!).value = EditorPrefs.GetInt(key, 0);
                }
                else if (typeof(T) == typeof(float))
                {
                    ((this as PrefsVariable<float>)!).value = EditorPrefs.GetFloat(key, 0f);
                }
                else if (typeof(T) == typeof(bool))
                {
                    ((this as PrefsVariable<bool>)!).value = EditorPrefs.GetBool(key, false);
                }
                else if (typeof(T) == typeof(string))
                {
                    ((this as PrefsVariable<string>)!).value = EditorPrefs.GetString(key, string.Empty);
                }
                else
                {
                    throw new ArgumentException($"Unknown variable type: {typeof(T).Name}");
                }
            }

            private void SaveValue()
            {
                if (typeof(T) == typeof(int))
                {
                    EditorPrefs.SetInt(key, ((this as PrefsVariable<int>)!).value);
                }
                else if (typeof(T) == typeof(float))
                {
                    EditorPrefs.SetFloat(key, ((this as PrefsVariable<float>)!).value);
                }
                else if (typeof(T) == typeof(bool))
                {
                    EditorPrefs.SetBool(key, ((this as PrefsVariable<bool>)!).value);
                }
                else if (typeof(T) == typeof(string))
                {
                    EditorPrefs.SetString(key, (this as PrefsVariable<string>)?.value);
                }
                else
                {
                    throw new ArgumentException($"Unknown variable type: {typeof(T).Name}");
                }
            }
        }
    }
}