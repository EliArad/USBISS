using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxpPAApiLib
{
    public static class Helper
    {
        public static ushort GetShort(byte [] buf)
        {
            ushort x = 0;
            return x = (ushort)((buf[1] << 8) | buf[0]);
        }
    }
}
