using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TFnsc.BinaryConverter.Extensions;

namespace TFnsc.BinaryConverter
{
    

    public class BinaryConverter<T>
    {
        private readonly BackwardsConverter _backwardsConverter;
        private readonly ForwardsConverter _forwardsConverter;

        public BinaryConverter()
        {
            var type = typeof(T);
            var conversionFactory = new ObjectToBytesConverterFactory();
            var converters = conversionFactory.GetConverter(type);
            _backwardsConverter = async (obj, getBytes) => await converters.Backwards(obj, getBytes);
            _forwardsConverter = converters.Forwards;
        }

        public async Task WriteToStreamAsync(Stream stream, T item) =>
            await _forwardsConverter(item, stream);

        public async Task<byte[]> ConvertToByteArrayAsync(T item)
        {
            var ms = new MemoryStream();
            await WriteToStreamAsync(ms, item);
            return ms.ToArray();
        }

        public byte[] ConvertToByteArray(T item) =>
            ConvertToByteArrayAsync(item).Result;

        public T ConvertFromByteArray(byte[] input) =>
            ConvertFromStreamAsync(new MemoryStream(input)).Result;

        public async Task<T> ConvertFromStreamAsync(Stream input)
        {
            async Task<byte[]> getBytes(int bytes)
            {
                var arr = new byte[bytes];
                await input.ReadAsync(arr, 0, bytes);
                return arr;
            }
            return (T)await _backwardsConverter(null, getBytes);
        }
    }
}