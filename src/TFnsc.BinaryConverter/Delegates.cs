using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TFnsc.BinaryConverter
{
    public delegate Task<object> BackwardsConverter(object obj, BytesGetter getBytes);
    public delegate Task ForwardsConverter(object toConvert, Stream toWrite);
    public delegate Task<byte[]> BytesGetter(int nBytes);
}
