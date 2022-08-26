using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLMod
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"VLMod by Rai#3621";
            if(args.Length == 0)
            {
                Console.WriteLine($"Please Drag & Drop a video onto the executable");
                return;
            }
            string file = args[0];
            byte[] FileBytes = File.ReadAllBytes(file);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Input file: {file}");

            Console.ResetColor();
            Console.WriteLine(@"Select Option:
MP4 ONLY:
    [1] Custom video length
    [2] Infinite video length 
    [3] Negative-Infinite video length
    [4] 1 Second length

WEBM ONLY:
    [5] Growing Time Length
    [6] 0 Seconds
    [7] Custom Time (WEBM)
");

            void DebugWrite(object text, ConsoleColor color = ConsoleColor.DarkGray)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
            string DebugRead(string prefix="")
            {
                Console.Write(prefix);
                return Console.ReadLine();
            }

            Console.Write("Option: ");
            string opt = Console.ReadLine();
            Console.Clear();

            byte[] OverwriteMP4Time(byte[] Data, byte[] TimeBytes, bool OverwriteO3E8=false)
            {
                DebugWrite("Scanning for mvhd offset");
                int mvhd = Scanner.GetMVHDOffset(Data);
                if (mvhd == -1)
                {
                    DebugWrite("ERROR: Couldn't find mvhd offset in the given file, the file needs to be an mp4", ConsoleColor.Red);
                    Console.ReadLine();
                    return null;
                }
                DebugWrite($"mvhd: {mvhd}");

                DebugWrite("Scanning for o3e8 offset");
                int o3e8 = Scanner.Get03E8Offset(mvhd, Data);
                if (o3e8 == -1)
                {
                    DebugWrite("ERROR: Couldn't find o3e8 offset in the given file, the file needs to be an mp4", ConsoleColor.Red);
                    Console.ReadLine();
                    return null;
                }
                if (o3e8 > mvhd + 200)
                {
                    DebugWrite("WARNING: o3e8 offset was located, however its offset is way larger than usual, using approximate offset instead (video length might not be as requested)", ConsoleColor.Yellow);
                    o3e8 = mvhd + 18; // o3e8 (or its equivalent0 is usually located 18 bytes off from mvhd
                }

                DebugWrite($"o3e8: {o3e8}");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Over-writting: ");
                for (int i = 0; i < TimeBytes.Length; i++)
                {
                    Console.Write($"{Data[o3e8 + (OverwriteO3E8 ? 0 : 2 ) + i]} => {TimeBytes[i]}, ");
                    Data[o3e8 + (OverwriteO3E8 ? 0 : 2) + i] = TimeBytes[i];
                }
                Console.Write("\n");
                DebugWrite("Finished over-writting bytes", ConsoleColor.Green);
                return Data;
            }

            byte[] OverwriteWEBMTime(byte[] Data, byte[] TimeBytes)
            {
                DebugWrite("Scanning for time offset");
                int TimeOff = Scanner.GetWEBMTimeOffset(Data);
                if (TimeOff == -1)
                {
                    DebugWrite("ERROR: Couldn't locate TimeOffset for the provided file", ConsoleColor.Red);
                    Console.ReadLine();
                    return null;
                }

                for(int i=0; i < TimeBytes.Length; i++)
                    Data[TimeOff+i] = TimeBytes[i];

                return Data;
            }
            byte[] OverwriteWEBMTimeByte(byte[] Data, int Pos, byte b)
            {
                DebugWrite("Scanning for time offset");
                int TimeOff = Scanner.GetWEBMTimeOffset(Data);
                if (TimeOff == -1)
                {
                    DebugWrite("ERROR: Couldn't locate TimeOffset for the provided file", ConsoleColor.Red);
                    Console.ReadLine();
                    return null;
                }

                Data[TimeOff + Pos] = b;

                return Data;
            }


            if (opt == "1")
            {
                DebugWrite("Selected: Custom Video Length", ConsoleColor.Yellow);
                int MS = int.Parse(DebugRead("Length (in milliseconds): "));
                FileBytes = OverwriteMP4Time(FileBytes, BitConverter.GetBytes(MS));
            }
            else if(opt == "2")
            {
                DebugWrite("Selected: Infinite Time Length", ConsoleColor.Yellow);
                FileBytes = OverwriteMP4Time(FileBytes, new byte[] { 0, 1, 0, 255, 255, 255 }, true);
            }
            else if (opt == "3")
            {
                DebugWrite("Selected: Negative Infinite Time Length", ConsoleColor.Yellow);
                FileBytes = OverwriteMP4Time(FileBytes, new byte[] { 0, 1, 255, 0, 0, 0 }, true);
            }

            else if (opt == "4")
            {
                DebugWrite("Selected: 1 Second Length", ConsoleColor.Yellow);
                FileBytes = OverwriteMP4Time(FileBytes, new byte[] { 0, 1, 255, 255, 255, 255 }, true);
            }
            else if(opt == "5")
            {
                DebugWrite("Selected: Growing Time Length", ConsoleColor.Yellow);
                FileBytes = OverwriteWEBMTimeByte(FileBytes, 1, 0);

            }
            else if(opt == "6")
            {
                DebugWrite("Selected: Growing Time Length", ConsoleColor.Yellow);
                FileBytes = OverwriteWEBMTime(FileBytes, new byte[] {0,0,0,0,0,0,0,0});
            }
            else if(opt == "7")
            {
                DebugWrite("Selected: Custom Time (WEBM)", ConsoleColor.Yellow);
                int MS = int.Parse(DebugRead("Length (in milliseconds): "));
                FileBytes = OverwriteWEBMTime(FileBytes, BitConverter.GetBytes(MS));
            }

            string fileName = Path.GetFileNameWithoutExtension(file) + "-converted" + Path.GetExtension(file);
            File.WriteAllBytes(fileName, FileBytes);
            DebugWrite($"Saved file as {fileName}", ConsoleColor.DarkGreen);

            Console.ResetColor();
            Console.WriteLine("\nPress any button to exit.");
            Console.ReadKey();
        }
    }
}
