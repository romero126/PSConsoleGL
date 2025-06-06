using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PSConsoleGL.Terminal.Drawing;


namespace PSConsoleGL.Terminal.Drawing
{
    public class Render
    {
        public static void DrawSixelToScreen(FrameBuffer frameBuffer) {
            // Use Conout instead

            var buffer = Console.OpenStandardOutput();
            StreamWriter streamWriter = new StreamWriter(buffer);

            StringBuilder stringBuilder = new StringBuilder();
            Hashtable colorMap = new Hashtable();
            //Dictionary<int, int> colorMap = new Dictionary<int, int>();

            // escape sequence to start sixel graphics
            //stringBuilder.Append("\x1bP0;1q");
            streamWriter.Write("\x1bP0;1q");

            // Force 1:1 scale
            streamWriter.Write("\"1;1;{0};{1};", frameBuffer.Width, frameBuffer.Height);
            streamWriter.Write("#0;2;0;0;0");

            //stringBuilder.AppendFormat("\"1;1;{0};{1};", frameBuffer.Width, frameBuffer.Height);
            //stringBuilder.Append("#0;2;0;0;0");

            int count = 0;
            // Draw sixel graphics
            for (int i = 0; i < frameBuffer.Buffer.Length; i++) {
                Int32 argb = frameBuffer.Buffer[i];

                // Take ARGB and convert it to RGB32
                int r = (argb >> 16) & 0xFF;
                int g = (argb >> 8) & 0xFF;
                int b = argb & 0xFF;
                
                // For red and green, the 3 bit channels, divide source channel value by 32.
                // For blue, the 2 bit channel, divide source channel value by 64.
                if (r % 32 != 0) r = r - (r % 32);
                if (g % 32 != 0) g = g - (g % 32);
                if (b % 64 != 0) b = b - (b % 64);
                Int32 c = 255 << 24 | r << 16 | g << 8 | b;

                if (!colorMap.ContainsKey(c)) {
                    decimal rm = Math.Floor(((decimal)r / 255) * 100);
                    decimal gm = Math.Floor(((decimal)g / 255) * 100);
                    decimal bm = Math.Floor(((decimal)b / 255) * 100);
                                       
                    count = colorMap.Count + 1;
                    colorMap.Add(c, count);

                    streamWriter.Write("#{0};2;{1};{2};{3}", count, rm, gm, bm);
                    //stringBuilder.AppendFormat("#{0};2;{1};{2};{3}", count, rm, gm, bm);
                }

                int x = i % frameBuffer.Width;
                int y = i / frameBuffer.Width;
                int sixelPos = y % 6;

                // This is a transparent pixel check
                if (c == -16777216) {
                    streamWriter.Write("#0?");
                    //stringBuilder.Append("#0?");
                } else {
                    streamWriter.Write("#{0}{1}", colorMap[c], (char)(63 + Math.Pow(2, sixelPos)));
                    //stringBuilder.AppendFormat("#{0}{1}", colorMap[c], (char)(63 + Math.Pow(2, sixelPos)));

                }

                if (sixelPos == 5 && x == frameBuffer.Width-1) {
                    streamWriter.Write("-");
                    //stringBuilder.Append("-");
                } else if (x == frameBuffer.Width-1) {
                    streamWriter.Write("$");
                    //stringBuilder.Append("$");
                }

                streamWriter.Write("{0}{1}", colorMap[c], sixelPos);
                //stringBuilder.AppendFormat("{0}{1}", colorMap[c], sixelPos);
            }

            // escape sequence to end sixel graphics
            streamWriter.Write("\x1b\\");
            //stringBuilder.Append("\x1b\\");
            //Console.Write(stringBuilder.Replace("\x1b", "__ESC__").ToString());
            //Console.Write(stringBuilder.ToString());

            streamWriter.Flush();
            streamWriter.Dispose();
        }

    }    

}