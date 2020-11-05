using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFnsc.BinaryConverter.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static async IAsyncEnumerable<List<T>> ToListsAsync<T>(this IAsyncEnumerable<T> enumerable, int batchSize)
        {
            var list = new List<T>(batchSize);
            await foreach (var item in enumerable)
            {
                list.Add(item);
                if (list.Count == batchSize)
                {
                    yield return list;
                    list = new List<T>(batchSize);
                }
            }
            if (list.Count > 0)
                yield return list;
        }

        public static IEnumerable<List<T>> ToLists<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            var list = new List<T>(batchSize);
            foreach (var item in enumerable)
            {
                list.Add(item);
                if (list.Count == batchSize)
                {
                    yield return list;
                    list = new List<T>(batchSize);
                }
            }
            if (list.Count > 0)
                yield return list;
        }

        public static byte[] AggregateBytes(this IEnumerable<byte[]> bytes)
        {
            var list = bytes.ToArray();
            var endResult = new byte[list.Sum(b => b.Length)];
            for (int i = 0, offset = 0; i < list.Length; offset += list[i].Length, i++)
                list[i].CopyTo(endResult, offset);
            return endResult;
        }
    }
}
