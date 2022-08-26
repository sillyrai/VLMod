using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLMod
{
    public static class Scanner
    {
        public static int GetMVHDOffset(byte[] data)
        {
            byte[] mvhd_aob = { 109, 118, 104, 100 }; // "mvhd"
            for (int i = 0; i < data.Length; i++)
                if (data.Skip(i).Take(mvhd_aob.Length).SequenceEqual(mvhd_aob))
                    return i;
            return -1;
        }
        public static int Get03E8Offset(int mvhd, byte[] data)
        {
            byte[] o3e8_aob = { 3, 232 }; // "03 E8"
            for (int i = mvhd; i < data.Length; i++)
            {
                if(i>200)
                    // Give up.
                    return mvhd+250; // return a number to trigger a warning that it was too large
                if (data.Skip(i).Take(o3e8_aob.Length).SequenceEqual(o3e8_aob))
                    return i;
            }

            return mvhd+18; // incase it cannot locate it, return an approximate (o3e8 is usually located 18 bytes off from mvhd)
        }

        private static void DebugWrite(object text, ConsoleColor color = ConsoleColor.DarkGray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        public static int GetWEBMTimeOffset(byte[] data)
        {
            // find 2a d7 b1 first
            // then 44 89 88
            // after 44 89 88, next 8 bytes is the video length, we need to return the offset of these 8 bytes

            byte[] _2ad7b1 = { 42, 215, 177 };
            int _2ad7b1_offset = -1;
            for(int i=0;i<data.Length; i++)
                if (data.Skip(i).Take(_2ad7b1.Length).SequenceEqual(_2ad7b1))
                {
                    _2ad7b1_offset = i;
                    break;
                }
            if (_2ad7b1_offset == -1) return -1;
            DebugWrite($"Found 2ad7b1 successfully");



            byte[] _448988 = {68,137,136};
            int _448988_offset = -1;
            for(int i=_2ad7b1_offset; i<data.Length; i++)
            {
                if (data.Skip(i).Take(_448988.Length).SequenceEqual(_448988))
                {
                    _448988_offset = i;
                    break;
                }
            }
            if (_448988_offset == -1) return -1;
            DebugWrite($"Found 448988 successfully");

            return _448988_offset + 3; // Time begins 3 bytes after this
        }
    }
}
