using System.Collections;
using System.Collections.Generic;
namespace Framework.Net
{
    /// <summary>
    /// Flush 最大值：1 << 8 *1000 + 1 << 8    256256
    /// </summary>
    public enum EFlush
    {
        TestC2S = (1<<8 + 1),
        TestS2C = (1<<8 + 2),
    }
   
}


