using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TFnsc.BinaryConverter.Extensions;

namespace TFnsc.BinaryConverter
{
    public class ObjectToBytesConverterFactory
    {
        private static Dictionary<Type, ObjectToBytesConverter> _converters;

        public ObjectToBytesConverterFactory()
        {
            _converters = new Dictionary<Type, ObjectToBytesConverter>();
        }

        public ObjectToBytesConverter GetConverter(Type type)
        {
            if (_converters.TryGetValue(type, out ObjectToBytesConverter converter))
                return converter;
            var newConverter = CreateConverter(type);
            _converters[type] = newConverter;
            return newConverter;
        }

        private ObjectToBytesConverter CreateConverter(Type type)
        {
            if (type == typeof(byte))
                return ObjectToBytesConverter.Simple(b => new byte[] { b }, async ret => (await ret(1))[0]);
            if (type == typeof(bool))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToBoolean(await ret(sizeof(bool))));
            if (type == typeof(long))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToInt64(await ret(sizeof(long))));
            if (type == typeof(int))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToInt32(await ret(sizeof(int))));
            if (type == typeof(short))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToInt16(await ret(sizeof(short))));
            if (type == typeof(float))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToSingle(await ret(sizeof(float))));
            if (type == typeof(double))
                return ObjectToBytesConverter.Simple(BitConverter.GetBytes, async ret => BitConverter.ToDouble(await ret(sizeof(double))));
            if (type == typeof(DateTime))
                return ObjectToBytesConverter.Simple(dt => BitConverter.GetBytes(dt.Ticks), async ret => new DateTime(BitConverter.ToInt64(await ret(sizeof(long)))));
            if (type == typeof(TimeSpan))
                return ObjectToBytesConverter.Simple(dt => BitConverter.GetBytes(dt.Ticks), async ret => new TimeSpan(BitConverter.ToInt64(await ret(sizeof(long)))));
            if (type.IsEnum)
            {
                var underlyingConverter = GetConverter(Enum.GetUnderlyingType(type));
                return new ObjectToBytesConverter(underlyingConverter.Forwards, underlyingConverter.Backwards);
            }
            
            if (type == typeof(string))
                return ObjectToBytesConverter.DynamicLength(Encoding.UTF8.GetBytes, Encoding.UTF8.GetString);

            var enumerableType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableType!=null)
            {
                var itemType = enumerableType.GetGenericArguments().Single();
                var itemConverter = GetConverter(itemType);
                
                return ObjectToBytesConverter.New(async (i, stream) =>
                {
                    foreach(var item in i)
                    {
                        await stream.WriteAsync(new byte[] { 1 });
                        await itemConverter.Forwards(item, stream);
                    }
                    stream.Write(new byte[] { 0 });
                }, async getBytes => {
                    var listType = typeof(List<>).MakeGenericType(itemType);
                    var list = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod(nameof(List<object>.Add), new Type[] { itemType });
                    void add(object item) => addMethod.Invoke(list, new object[] { item });
                    while ((await getBytes(1))[0] == 1)
                        add(await itemConverter.Backwards(null, getBytes));
                    if (type.IsArray)
                        return listType.GetMethod(nameof(List<object>.ToArray), new Type[] { }).Invoke(list, new object[] { }) as IEnumerable;
                    return list as IEnumerable;
                });
            }

            var parameterlessConstructor = type.GetParameterlessConstructor();
            if (parameterlessConstructor != null)
            {
                var properties = type.GetProperties()
                    .Where(p => p.CanWrite)
                    .ToList();

                var _backwardsConverters = properties
                .Select(p =>
                {
                    var converter = GetConverter(p.PropertyType).Backwards;
                    return (BackwardsConverter)(async (instance, get) =>
                    {
                        p.SetValue(instance, await converter(instance, get));
                        return instance;
                    });
                })
                .ToList();

                var _forwardsConverters = properties
                    .Select(p =>
                    {
                        var converter = GetConverter(p.PropertyType).Forwards;
                        return (ForwardsConverter)((obj, stream) => converter(p.GetValue(obj), stream));
                    })
                    .ToList();

                async Task<object> backwardsConverter(object obj, BytesGetter bytesGetter)
                {
                    var instance = Activator.CreateInstance(type);

                    foreach (var converter in _backwardsConverters)
                        await converter(instance, bytesGetter);
                    return instance;
                }

                async Task forwardsConverter(object toConvert, Stream stream)
                {
                    foreach (var converter in _forwardsConverters)
                        await converter(toConvert, stream);
                }

                return new ObjectToBytesConverter(forwardsConverter, backwardsConverter);
            }
            throw new NotImplementedException(type.FullName);
        }
    }
}