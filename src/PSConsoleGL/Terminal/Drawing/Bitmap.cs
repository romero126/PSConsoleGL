using System;
using System.IO;

namespace PSConsoleGL.Terminal.Drawing
{
    // Basic Bitmap functionality however does not work on Modern bitmaps
    // Only supports uncompressed bitmaps

    public class Bitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] PixelData { get; private set; }

        public Bitmap(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // Read BMP Header
                if (br.ReadByte() != 'B' || br.ReadByte() != 'M')
                    throw new Exception("Not a BMP file.");

                br.ReadInt32(); // File size
                br.ReadInt16(); // Reserved1
                br.ReadInt16(); // Reserved2
                int pixelArrayOffset = br.ReadInt32();

                // DIB Header
                int dibHeaderSize = br.ReadInt32();
                if (dibHeaderSize != 40)
                    throw new Exception("Only BITMAPINFOHEADER BMPs are supported.");

                Width = br.ReadInt32();
                Height = br.ReadInt32();
                br.ReadInt16(); // Planes
                int bitsPerPixel = br.ReadInt16();
                if (bitsPerPixel != 24 && bitsPerPixel != 32)
                    throw new Exception("Only 24-bit or 32-bit BMPs are supported.");

                int compression = br.ReadInt32();
                if (compression != 0)
                    throw new Exception("Compressed BMPs are not supported.");

                br.ReadInt32(); // Image size
                br.ReadInt32(); // X pixels per meter
                br.ReadInt32(); // Y pixels per meter
                br.ReadInt32(); // Colors in color table
                br.ReadInt32(); // Important color count

                // Move to pixel array
                fs.Seek(pixelArrayOffset, SeekOrigin.Begin);

                int bytesPerPixel = bitsPerPixel / 8;
                int rowSize = ((bitsPerPixel * Width + 31) / 32) * 4;
                PixelData = new byte[Width * Height * bytesPerPixel];

                for (int y = Height - 1; y >= 0; y--)
                {
                    int rowStart = y * Width * bytesPerPixel;
                    byte[] row = br.ReadBytes(rowSize);
                    Array.Copy(row, 0, PixelData, rowStart, Width * bytesPerPixel);
                }
            }
        }
    }
}