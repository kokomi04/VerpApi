using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace VErp.Commons.Library
{
    public static class ObjectUtils
    {

        public static void UpdateIfAvaiable<T, TMember>(this T obj, Expression<Func<T, TMember>> member, IDictionary<string, TMember> dics, string key)
        {
            if (dics.ContainsKey(key))
            {
                obj.UpdateIfAvaiable(member, dics[key]);
            }
        }

        public static void UpdateIfAvaiable<T, TMember, TDic>(this T obj, Expression<Func<T, TMember>> member, IDictionary<string, TDic> dics, string key, Expression<Func<TDic, TMember>> divValue)
        {
            if (dics.ContainsKey(key))
            {
                var v = divValue.Compile().Invoke(dics[key]);
                obj.UpdateIfAvaiable(member, v);
            }
        }

        public static void UpdateIfAvaiable<T, TMember>(this T obj, Expression<Func<T, TMember>> member, TMember value)
        {
            if (value.IsNullOrEmptyObject())
            {
                return;
            }

            var props = new Stack<PropertyInfo>();
            MemberExpression me;
            switch (member.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = member.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = member.Body as MemberExpression;
                    break;
            }


            while (me != null)
            {
                props.Push(me.Member as PropertyInfo);
                me = me.Expression as MemberExpression;
            }

            object target = obj;
            while (props.Count > 0)
            {
                var p = props.Pop();

                if (props.Count == 0)
                {
                    p.SetValue(target, value);
                }
                else
                {
                    target = p.GetValue(target);
                }
            }
        }





        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }

        // public static T GetPropertyValue<T>(this object obj, string propertyName)
        // {
        //     return (T)obj.GetType().GetProperty(propertyName).GetValue(obj);
        // }



        public static T GetPropertyValue<T>(this object sourceObject, string propertyName)
        {
            if (sourceObject == null) throw new ArgumentNullException(nameof(sourceObject));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(nameof(propertyName));

            foreach (string currentPropertyName in propertyName.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(currentPropertyName)) throw new InvalidOperationException($"Invalid property '{propertyName}'");

                PropertyInfo propertyInfo = sourceObject.GetType().GetProperty(currentPropertyName);
                if (propertyInfo == null) throw new InvalidOperationException($"Property '{currentPropertyName}' not found");

                sourceObject = propertyInfo.GetValue(sourceObject);
            }

            return sourceObject is T result ? result : default;
        }

        public static bool IsNullOrEmptyObject(this object obj)
        {
            if (obj == null || obj == DBNull.Value) return true;

            if (obj.GetType() == typeof(string))
            {
                obj = (obj as string).Trim();

                if (string.Empty.Equals(obj)) return true;
            }

            return false;
        }

        public static TKey GetFirstValueNotNull<T, TKey>(this IList<T> lst, Func<T, TKey> func)
        {
            return lst.Where(x =>
            {
                var valid = func(x);
                return !valid.IsNullOrEmptyObject();
            }).Select(func)
            .FirstOrDefault();
        }

        public static T DeepClone<T>(this T a)
        {
            return a.JsonSerialize().JsonDeserialize<T>();
            // using (MemoryStream stream = new MemoryStream())
            // {
            //     BinaryFormatter formatter = new BinaryFormatter();
            //     formatter.Serialize(stream, a);
            //     stream.Position = 0;
            //     return (T)formatter.Deserialize(stream);
            // }
        }


        public static bool IsClass(this Type type)
        {
            bool isPrimitiveType = type.IsPrimitive || type.IsValueType || (type == typeof(string));

            return type.IsClass && !isPrimitiveType;
        }

        public static void CopyValuesFrom<T>(this T target, T source, bool appendList = true, bool copyObject = true)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                var targetValue = prop.GetValue(target, null);

                bool isPrimitiveType = prop.PropertyType.IsPrimitive || prop.PropertyType.IsValueType || (prop.PropertyType == typeof(string));

                if (appendList && !isPrimitiveType && (prop.PropertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)))
                {

                    if (targetValue.IsNullOrEmptyObject())
                    {
                        targetValue = Activator.CreateInstance(prop.PropertyType);
                    }

                    if (!value.IsNullOrEmptyObject())
                    {
                        var lst = new List<object>();
                        foreach (var v in targetValue as IEnumerable)
                        {
                            lst.Add(v);
                        }

                        foreach (var newVal in value as IEnumerable)
                        {
                            if (!lst.Contains(newVal))
                                lst.Add(newVal);
                        }

                        Type baseType;
                        if (prop.PropertyType.IsGenericType)
                        {
                            baseType = prop.PropertyType.GenericTypeArguments[0];
                        }
                        else
                        {
                            baseType = prop.PropertyType.GetElementType();
                        }

                        var newArray = Array.CreateInstance(baseType, lst.Count);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            newArray.SetValue(lst[i], i);
                        }
                        targetValue = newArray;
                    }

                    prop.SetValue(target, targetValue, null);

                }
                else if (!value.IsNullOrEmptyObject())
                {


                    if ((copyObject || isPrimitiveType) && targetValue != value)
                    {
                        prop.SetValue(target, value, null);
                    }

                }
            }
        }
        public static T MergeData<T>(this IList<T> items) where T : class
        {
            var result = Activator.CreateInstance<T>();

            foreach (var item in items)
            {
                result.CopyValuesFrom(item);
            }

            return result;
        }


        public static T InheritObject<T>(this T obj, T parent) where T : class
        {
            if (obj == null && parent == null) return null;

            var result = Activator.CreateInstance<T>();

            result.CopyValuesFrom(parent);

            return result;
        }

        public static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(decimal);
        }

        public static bool IsCollectionType(Type type)
        {
            return type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
