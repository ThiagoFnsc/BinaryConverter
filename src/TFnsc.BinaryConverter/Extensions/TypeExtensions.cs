using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TFnsc.BinaryConverter.Extensions
{
    internal static class TypeExtensions
    {
        public static ConstructorInfo GetParameterlessConstructor(this Type type) =>
            type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
    }
}
