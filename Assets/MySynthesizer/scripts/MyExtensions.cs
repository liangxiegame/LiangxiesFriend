
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MySpace
{
    public static class MyMonoBehaviourExtensions
    {
#if UNITY_EDITOR
        private class MyRoutine
        {
            private bool finished;
            private uint lastTime;
            private IEnumerator enumerator;
            private object current;
            public uint LastTime
            {
                get
                {
                    return lastTime;
                }
            }
            public object Current
            {
                get
                {
                    return current;
                }
            }
            public MyRoutine(IEnumerator enumerator)
            {
                finished = false;
                lastTime = GetTime();
                current = null;
                this.enumerator = enumerator;
                NestedMoveNextExec(this.enumerator, out current);
            }
            private bool NestedMoveNextExec(IEnumerator ie, out object current)
            {
                if (ie.MoveNext())
                {
                    current = ie.Current;
                    var iex = current as IEnumerator;
                    if (iex != null)
                    {
                        NestedMoveNextExec(iex, out current);
                    }
                    return true;
                }
                current = ie.Current;
                return false;
            }
            private bool NestedMoveNext(IEnumerator ie, out object current)
            {
                var coie = ie.Current as IEnumerator;
                if (coie != null)
                {
                    if (NestedMoveNext(coie, out current))
                    {
                        return true;
                    }
                }
                return NestedMoveNextExec(ie, out current);
            }
            public void MoveNext(uint time)
            {
                if (finished)
                {
                    return;
                }
                if (lastTime == time)
                {
                    return;
                }
                lastTime = time;
                if (!NestedMoveNext(enumerator, out current))
                {
                    finished = true;
                }
                //Debug.Assert(!(current is Coroutine));
            }
        }
        private static uint GetTime()
        {
            return (uint)(DateTime.UtcNow.Ticks / 10000);  // 100nsec -> msec
        }
        private static int refCount = 0;
        private static Dictionary<IEnumerator, MyRoutine> enumeratorMyRoutineMap;
        private static Dictionary<IEnumerator, MyRoutine>.ValueCollection routines;
        private static void AddRef()
        {
            if (refCount++ == 0)
            {
                EditorApplication.update += Update;
                enumeratorMyRoutineMap = new Dictionary<IEnumerator, MyRoutine>();
                routines = enumeratorMyRoutineMap.Values;
            }
        }
        private static void Release()
        {
            if (--refCount == 0)
            {
                enumeratorMyRoutineMap = null;
                routines = null;
                EditorApplication.update -= Update;
            }
        }
        private static void Update()
        {
            uint time = GetTime();
            foreach (var routine in routines)
            {
                if (routine.Current is WaitForFixedUpdate)
                {
                    routine.MoveNext(time);
                }
            }
            foreach (var routine in routines)
            {
                if (routine.Current == null)
                {
                    routine.MoveNext(time);
                }
            }
            foreach (var routine in routines)
            {
                var wfs = routine.Current as WaitForSeconds;
                if (wfs != null)
                {
                    var fd = typeof(WaitForSeconds).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    //Debug.Assert(fd[0].Name == "m_Seconds");
                    //Debug.Assert(fd[0].FieldType == typeof(float));
                    float seconds = (float)fd[0].GetValue(wfs);
                    var past = (time - routine.LastTime) * 0.001f;
                    if (past >= seconds)
                    {
                        routine.MoveNext(time);
                    }
                }
            }
            foreach (var routine in routines)
            {
                if (routine.Current is WaitForEndOfFrame)
                {
                    routine.MoveNext(time);
                }
            }
        }
#endif
        /// <summary>
        /// Executable in edit mode
        /// </summary>
        /// <param name="mb"></param>
        /// <param name="ie"></param>
        public static void StartCoroutineEx(this MonoBehaviour mb, IEnumerator ie)
        {
            if (ie == null)
            {
                throw new ArgumentNullException();
            }
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                AddRef();
                enumeratorMyRoutineMap.Add(ie, new MyRoutine(ie));
                return;
            }
#endif
            mb.StartCoroutine(ie);
        }
        public static void StopCoroutineEx(this MonoBehaviour mb, IEnumerator ie)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                enumeratorMyRoutineMap.Remove(ie);
                Release();
                return;
            }
#endif
            mb.StopCoroutine(ie);
        }
    }

    public class MyReadOnlyAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MyReadOnlyAttribute))]
    public class MyReadOnlyDrawer : PropertyDrawer
    {
        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif // UNITY_EDITOR

    public class MyHexNumberAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MyHexNumberAttribute))]
    public class MyHexNumberDrawer : PropertyDrawer
    {
        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            unchecked
            {
                var value = (uint)property.intValue;
                var str = EditorGUI.TextField(position, label, value.ToString("X"));
                if (uint.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out value))
                {
#if false
                    property.intValue = (int)value;
#else
                    property.longValue = value;
#endif
                }
            }
        }
    }
#endif // UNITY_EDITOR


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MyConditionalAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        private Func<bool> isEnabled = null;
        private Func<bool> isVisible = null;
        private object target = null;
        private readonly Type type;
        private readonly string name;
        private readonly object value;
        private readonly bool hide;
        public bool IsEnabled(object obj)
        {
            if (isEnabled == null)
            {
                Setup(obj);
            }
            return isEnabled();
        }
        public bool IsVisible(object obj)
        {
            if (isVisible == null)
            {
                Setup(obj);
            }
            return isVisible();
        }
        private void Setup(object obj)
        {
            BindingFlags flags;
            if ((type == null) && (obj == null))
            {
                isVisible = () => true;
                isEnabled = () => false;
                return;
            }
            var targetType = type;
            if (targetType == null)
            {
                targetType = obj.GetType();
                flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                target = obj;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                target = type;
            }
            var valueType = (value == null) ? typeof(object) : value.GetType();
            var field = targetType.GetFields(flags).FirstOrDefault((f) => f.Name.Equals(name) && valueType.IsAssignableFrom(f.FieldType));
            if (field != null)
            {
                Func<bool> condition = () => Equals(value, field.GetValue(target));
                Func<bool> alwaysTrue = () => true;
                isVisible = hide ? condition : alwaysTrue;
                isEnabled = hide ? alwaysTrue : condition;
                return;
            }
            var property = targetType.GetProperties(flags).FirstOrDefault((p) => p.Name.Equals(name) && valueType.IsAssignableFrom(p.PropertyType));
            if (property != null)
            {
                Func<bool> condition = () => Equals(value, property.GetValue(target, null));
                Func<bool> alwaysTrue = () => true;
                isVisible = hide ? condition : alwaysTrue;
                isEnabled = hide ? alwaysTrue : condition;
                return;
            }
            var method = targetType.GetMethods(flags).FirstOrDefault((m) => m.Name.Equals(name) && valueType.IsAssignableFrom(m.ReturnType) && (m.GetParameters().Length == 0));
            if (method != null)
            {
                Func<bool> condition = () => Equals(value, method.Invoke(target, null));
                Func<bool> alwaysTrue = () => true;
                isVisible = hide ? condition : alwaysTrue;
                isEnabled = hide ? alwaysTrue : condition;
                return;
            }
            isVisible = () => true;
            isEnabled = () => false;
        }
#endif
        public MyConditionalAttribute(string name, object value, bool hide = false)
        {
#if UNITY_EDITOR
            this.type = null;
            this.name = name;
            this.value = value;
            this.hide = hide;
#endif
        }
        public MyConditionalAttribute(Type type, string name, object value, bool hide = false)
        {
#if UNITY_EDITOR
            this.type = type;
            this.name = name;
            this.value = value;
            this.hide = hide;
#endif
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MyConditionalAttribute))]
    public class MyConditionalDrawer : PropertyDrawer
    {
        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as MyConditionalAttribute;
            var visible = attr.IsVisible(property.serializedObject.targetObject);
            if (!visible)
            {
                return 0.0f;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as MyConditionalAttribute;
            var enabled = attr.IsEnabled(property.serializedObject.targetObject);
            var visible = attr.IsVisible(property.serializedObject.targetObject);
            if (visible)
            {
                EditorGUI.BeginDisabledGroup(!enabled);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
    public static class SerializedProperyExtensions
    {
        public static string GetTooltip(this SerializedProperty self)
        {
            var bindingFlags = BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var objectType = self.serializedObject.targetObject.GetType();
            var filedInfo = objectType.GetField(self.name, bindingFlags);
            var tooltip = Attribute.GetCustomAttribute(filedInfo, typeof(TooltipAttribute));
            if (tooltip == null)
            {
                return null;
            }
            var tooltipFiledInfo = tooltip.GetType().GetField("tooltip", bindingFlags);
            return (string)tooltipFiledInfo.GetValue(tooltip);
        }
        public static object GetTargetFieldValue(this SerializedProperty self)
        {
            var obj = (object)self.serializedObject.targetObject;
            var elements = self.propertyPath.Replace(".Array.data[", "[").Split('.');
            foreach (var element in elements)
            {
                int bp0 = element.IndexOf("[");
                int bp1 = element.IndexOf("]");
                if ((bp0 >= 0) && (bp1 >= 0))
                {
                    var elementName = element.Substring(0, bp0);
                    int index;
                    int.TryParse(element.Substring(bp0 + 1, bp1 - bp0 - 1), out index);
                    obj = getArrayValue(obj, elementName, index);
                }
                else
                {
                    obj = getFieldValue(obj, element);
                }
            }
            return obj;
        }
        private static object getFieldValue(object parent, string name)
        {
            if (parent == null)
            {
                return null;
            }
            var type = parent.GetType();
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                return null;
            }
            return field.GetValue(parent);
        }
        private static object getArrayValue(object source, string name, int index)
        {
            var obj = getFieldValue(source, name);
            var list = obj as IList;
            if (list != null)
            {
                return list[index];
            }
            var array = obj as Array;
            if (array != null)
            {
                return array.GetValue(index);
            }
            return null;
        }
    }
#endif // UNITY_EDITOR
}
