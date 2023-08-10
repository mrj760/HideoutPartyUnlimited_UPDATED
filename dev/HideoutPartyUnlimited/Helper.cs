using System;
using System.Reflection;

namespace HideoutPartyUnlimited
{
    public static class Helper
    {
        public static void ReflectionSetFieldPropertyValue_Instance(object obj, string name, object data)
        {
            Type type = obj.GetType();
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod))
            {
                if (fieldInfo.Name == name)
                {
                    fieldInfo.SetValue(obj, data);
                }
            }
            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod))
            {
                if (propertyInfo.Name == name)
                {
                    propertyInfo.SetValue(obj, data);
                }
            }
        }

        public static object ReflectionGetField_Static(Type t, string fieldName)
        {
            return t.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod).GetValue(null);
        }

        public static object ReflectionGetField_Instance(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            PropertyInfo property = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            object result = null;
            if (field != null)
            {
                result = field.GetValue(obj);
            }
            else if (property != null)
            {
                result = property.GetValue(obj);
            }
            return result;
        }

        public static object ReflectionInvokeMethod_Instance(object obj, string methodName, object[] param)
        {
            return obj.GetType().InvokeMember(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, obj, param);
        }

        public static Delegate ReflectionCreateDelegate(object obj, string methodName, Type delegateType)
        {
            return obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod).CreateDelegate(delegateType, obj);
        }
    }
}
