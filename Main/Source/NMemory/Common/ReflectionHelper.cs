﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Common
{
    internal class ReflectionHelper
    {
        //public static MethodInfo GetMethodInfo<T>(Expression<Func<T, object>> expression)
        //{
        //    var method =  expression.Body as MethodCallExpression;

        //    return method.Method;
        //}

        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }


        public static MethodInfo GetMethodInfo<TClass>(Expression<Action<TClass>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method;
        }

        public static MethodInfo GetStaticMethodInfo<TResult>(Expression<Func<TResult>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method;
        }


        public static PropertyInfo GetPropertyInfo<TClass, TResult>(Expression<Func<TClass, TResult>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static string GetMethodName<T>(Expression<Func<T, object>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method.Name;
        }


        public static Type GetUnderlyingIfNullable(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static bool IsAnonymousType(Type type)
        {
            return
                !typeof(IComparable).IsAssignableFrom(type) &&
                type.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false)
                    .Cast<DebuggerDisplayAttribute>()
                    .Any(m => m.Type == "<Anonymous Type>");
        }

        public static bool IsNullable(Type type)
        {
            return
                !type.IsValueType ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsTuple(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type t = type.GetGenericTypeDefinition();

            return
                t == typeof(Tuple<>) ||
                t == typeof(Tuple<,>) ||
                t == typeof(Tuple<,,>) ||
                t == typeof(Tuple<,,,>) ||
                t == typeof(Tuple<,,,,>) ||
                t == typeof(Tuple<,,,,,>) ||
                t == typeof(Tuple<,,,,,,>);
        }
    }
}
