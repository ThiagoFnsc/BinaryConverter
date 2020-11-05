using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TFnsc.BinaryConverter
{
    public class ObjectToBytesConverter
    {
        public ForwardsConverter Forwards { get; }
        public BackwardsConverter Backwards { get; }

        public ObjectToBytesConverter(ForwardsConverter forwards, BackwardsConverter backwards)
        {
            Forwards = forwards;
            Backwards = backwards;
        }

        public static ObjectToBytesConverter New<T>(Func<T, Stream, Task> forwards, Func<BytesGetter, Task<T>> backwards) =>
            new ObjectToBytesConverter((part, stream) => forwards((T)part, stream), async (obj,ret) => await backwards(ret));

        public static ObjectToBytesConverter Simple<T>(Func<T, byte[]> forwards, Func<BytesGetter, Task<T>> backwards) =>
            New((obj, writer) => writer.WriteAsync(forwards(obj)).AsTask(), backwards);

        public static ObjectToBytesConverter DynamicLength<T>(Func<T, byte[]> forwards, Func<byte[], T> backwards)=>
            new ObjectToBytesConverter(async (value, stream) =>
            {
                var bytes = forwards((T)value);
                await stream.WriteAsync(BitConverter.GetBytes(bytes.Length));
                await stream.WriteAsync(bytes);
            },
                async (obj, getBytes) => {
                    int length = BitConverter.ToInt32(await getBytes(sizeof(int)));
                    var bytes = await getBytes(length);
                    return backwards(bytes);
                });

        public static ObjectToBytesConverter DynamicLength(Func<object, byte[]> forwards, Func<byte[], object> backwards) =>
            DynamicLength<object>(forwards, backwards);
    }
}