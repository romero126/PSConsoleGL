using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Xml;
using PSConsoleGL.Terminal.Drawing;

using    WORD = System.Int16;
using   SHORT = System.Int16;
using   DWORD = System.Int32;
using    BYTE = System.Char;
using    LONG = System.UInt32;
using   ULONG = System.UInt32;
using  LPVoid = System.IntPtr;
using  HANDLE = System.UInt64;
using COLOR16 = System.UInt16;

// DataStructure Implicit Structures
// https://github.com/jinhuca/Crystal.PInvoke/blob/e877d104fc6f7283760e702cabc6a0e93b76e6e3/PInvoke/Gdi32/WinGdi.Bitmap.cs
// https://github.com/jinhuca/Crystal.PInvoke/blob/e877d104fc6f7283760e702cabc6a0e93b76e6e3/PInvoke/Gdi32/WinGdi.BITMAPINFO.cs

namespace PSConsoleGL.Terminal.Drawing
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CIEXYZ
    {
        public int X;
        public int Y;
        public int Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CIEXYZTRIPLE
    {
        public CIEXYZ Red;
        public CIEXYZ Green;
        public CIEXYZ Blue;
    }

    // Bitmap File Header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapFileHeader
    {
        public WORD Type;
        public DWORD Size;
        public WORD Reserved1;
        public WORD Reserved2;
        public DWORD OffBits;
    }

    // Bitmap Info Header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInformationHeader
    {
        //                             // Type          | V1         | V4              | V5
        public DWORD Size;             //  DWORD        | bcSize     | biSize          | bV5Size
        public LONG Width;             //  LONG         | bcWidth    | biWidth         | bV5Width
        public LONG Height;            //  LONG         | bcHeight   | biHeight        | bV5Height
        public WORD Planes;            //  WORD         | bcPlanes   | biPlanes        | bV5Planes
        public WORD BitCount;          //  WORD         | bcBitCount | biBitCount      | bV5BitCount
        public DWORD Compression;      //  DWORD        |            | biCompression   | bV5Compression
        public DWORD SizeImage;        //  DWORD        |            | biSizeImage     | bV5SizeImage
        public LONG XPelsPerMeter;     //  LONG         |            | biXPelsPerMeter | bV5XPelsPerMeter
        public LONG YPelsPerMeter;     //  LONG         |            | biYPelsPerMeter | bV5YPelsPerMeter
        public DWORD ClrUsed;          //  DWORD        |            | biClrUsed       | bV5ClrUsed
        public DWORD ClrImportant;     //  DWORD        |            | biClrImportant  | bV5ClrImportant
        public DWORD RedMask;          //  DWORD        |            |                 | bV5RedMask
        public DWORD GreenMask;        //  DWORD        |            |                 | bV5GreenMask
        public DWORD BlueMask;         //  DWORD        |            |                 | bV5BlueMask
        public DWORD AlphaMask;        //  DWORD        |            |                 | bV5AlphaMask
        public DWORD CSType;           //  DWORD        |            |                 | bV5CSType
        public CIEXYZTRIPLE Endpoints; //  CIEXYZTRIPLE |            |                 | bV5Endpoints
        public DWORD GammaRed;         //  DWORD        |            |                 | bV5GammaRed
        public DWORD GammaGreen;       //  DWORD        |            |                 | bV5GammaGreen
        public DWORD GammaBlue;        //  DWORD        |            |                 | bV5GammaBlue
        public DWORD Intent;           //  DWORD        |            |                 | bV5Intent
        public DWORD ProfileData;      //  DWORD        |            |                 | bV5ProfileData
        public DWORD ProfileSize;      //  DWORD        |            |                 | bV5ProfileSize
        public DWORD Reserved;         //  DWORD        |            |                 | bV5Reserved
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public DWORD Blue;
        public DWORD Green;
        public DWORD Red;
        public DWORD Alpha;

        //public Color Color {
        //    get => Color(Alpha, Red, Green, Blue);
        //    set {
        //        Alpha = value.A; Blue = value.B; Green = value.G; Red = value.R;
        //    }
        //}

        public bool IsTransparent => Alpha == 0;

        //public static implicit operator RGBQUAD(Color c) => new() { Color = c };
        //public static implicit operator Color(RGBQUAD c) => c.Color;
    }


    public enum BitmapHeaderVersion
    {
        Core = 12,
        V1Header = 40,
        V2Header = 52,
        V3Header = 56,
        V4Header = 108,
        V5Header = 124
    }

    public enum BitmapHeaderCompression
    {
        // Notes: Unconfirmed data values
        // https://github.com/thomasxm/BOAZ_beta/blob/18aeff90af74bb4069672a16950b502bf7942c14/Include/10.0.22621.0/um/10.0.22621.0/um/wingdi.h#L684
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    public enum BitmapHeaderCSType
    {
        LCS_CALIBRATED_RGB = 0x00000000,
        LCS_sRGB = 0x73524742, // If were doing SRGB
        LCS_WINDOWS_COLOR_SPACE = 0x57696E20,
        // ??                       = 0x7FFC
        PROFILE_LINKED = 3,
        PROFILE_EMBEDDED = 4
    }

    public enum BitmapHeaderIntent
    {
        // Notes: Unconfirmed data values
        LCS_GM_ABS_COLORIMETRIC = (int)0x00000008L,
        LCS_GM_BUSINESS = (int)0x00000001L,
        LCS_GM_GRAPHICS = (int)0x00000002L,
        LCS_GM_IMAGES = (int)0x00000004L
    }

    public class Bitmap
    {
        public BitmapFileHeader bitmapFileHeader;
        public BitmapInformationHeader bitmapInformationHeader;
        internal bool isRGBA;
        internal RGBQUAD[] BitmapBuffer;

        public int Width
        {
            get
            {
                return (int)bitmapInformationHeader.Width;
            }
        }

        public int Height
        {
            get
            {
                return (int)bitmapInformationHeader.Height;
            }
        }

        internal T ReadBufferToStruct<T>(Stream stream, out byte[] buffer, int bufferSize)
        {
            buffer = new byte[Marshal.SizeOf(typeof(T))];
            stream.Read(buffer, 0, bufferSize);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var outputObject = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return outputObject;
        }

        internal T ReadBufferToStruct<T>(Stream stream, out byte[] buffer)
        {
            return ReadBufferToStruct<T>(stream, out buffer, Marshal.SizeOf(typeof(T)));
        }

        internal T ReadBufferToStruct<T>(Stream stream)
        {
            byte[] buffer;
            return ReadBufferToStruct<T>(stream, out buffer, Marshal.SizeOf(typeof(T)));
        }

        internal T ReadBufferToStruct<T>(Stream stream, int bufferSize)
        {
            byte[] buffer;
            return ReadBufferToStruct<T>(stream, out buffer, bufferSize);
        }

        internal int BitExtract(int value, int mask)
        {
            // If the mask is zero, return 0 (no bits are extracted).
            if (mask == 0)
                return 0;

            // Determine the number of trailing zeros in the mask.
            int maskBuffer = mask;
            int maskPadding = 0;

            // Shift the mask right until the first set bit is found.
            while ((maskBuffer & 1) == 0)
            {
                maskBuffer >>= 1;
                maskPadding++;
            }

            // Apply the mask to the value and shift right to align the extracted bits.
            return (int)((value & mask) >> maskPadding);
        }

        public void CopyToBuffer(FrameBuffer buffer)
        {
            // Lazy copy at this point
            for (int i = 0; i < BitmapBuffer.Length; i++)
            {
                buffer.SetPixel(
                    i,
                    (Int32)BitmapBuffer[i].Alpha,
                    (Int32)BitmapBuffer[i].Red,
                    (Int32)BitmapBuffer[i].Green,
                    (Int32)BitmapBuffer[i].Blue
                );
            }
        }
        public Bitmap(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // Read the fileheader
                bitmapFileHeader = ReadBufferToStruct<BitmapFileHeader>(fs);

                // BM~
                if (bitmapFileHeader.Type != 0x4D42)
                    throw new Exception("Unrecognized Bitmap Format");

                // Peek the buffer and step backwards
                var pos = fs.Position;
                int bitmapInformationHeaderSize = br.ReadInt32();
                fs.Seek(-4, SeekOrigin.Current);

                // Check to see if we are using valid bitmap header sizes
                if (!Enum.IsDefined(typeof(BitmapHeaderVersion), bitmapInformationHeaderSize))
                {
                    throw new Exception("Unsupported Bitmap format");
                }

                BitmapHeaderVersion HeaderVersion = (BitmapHeaderVersion)bitmapInformationHeaderSize;
                bitmapInformationHeader = ReadBufferToStruct<BitmapInformationHeader>(fs, bitmapInformationHeaderSize - 8);

                // Validate bit count
                if (bitmapInformationHeader.BitCount != 16 && bitmapInformationHeader.BitCount != 24 && bitmapInformationHeader.BitCount != 32)
                    throw new Exception("Unsupported Bitmap bit count got: " + bitmapInformationHeader.BitCount);

                // Validate compression levels
                if (bitmapInformationHeader.Compression != 0 && bitmapInformationHeader.Compression != 3)
                    throw new Exception("Unsupported Bitmap compression");

                if (HeaderVersion == BitmapHeaderVersion.V5Header || HeaderVersion == BitmapHeaderVersion.V4Header)
                {
                    // V4 and V5 defaults to RGBA file format.
                    isRGBA = true;
                }
                else
                {
                    // Switch to BGR
                    isRGBA = false;
                }

                BitmapBuffer = new RGBQUAD[bitmapInformationHeader.Height * bitmapInformationHeader.Width];

                // Define information on the bitmask.
                int colorsCount = bitmapInformationHeader.BitCount >> 3;
                if (colorsCount < 3)
                    colorsCount = 3;

                int bitsOnColor = bitmapInformationHeader.BitCount / colorsCount;
                int maskValue = (1 << bitsOnColor) - 1;

                if (
                        bitmapInformationHeader.RedMask == 0 ||
                        bitmapInformationHeader.GreenMask == 0 ||
                        bitmapInformationHeader.BlueMask == 0
                    )
                {
                    // Converts the mask into ARGB format
                    bitmapInformationHeader.RedMask = maskValue << (bitsOnColor * 2);
                    bitmapInformationHeader.GreenMask = maskValue << bitsOnColor;
                    bitmapInformationHeader.BlueMask = maskValue;
                }

                // We always define an Alpha Mask even if its not used.
                if (bitmapInformationHeaderSize < 56)
                {
                    bitmapInformationHeader.AlphaMask = maskValue << (bitsOnColor * 3);
                }

                fs.Seek(bitmapFileHeader.OffBits, SeekOrigin.Begin);

                for (int y = Height - 1; y > 0; y--)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;
                        byte[] buffer = new byte[4];
                        fs.Read(buffer, 0, (bitmapInformationHeader.BitCount / 8));

                        int bufferValue = 0;
                        if (isRGBA)
                        {
                            // Use RGBA Format and convert to ARGB for the mask
                            bufferValue = (buffer[0] << 16 | buffer[1] << 8 | buffer[2] | buffer[3] << 24);
                        }
                        else
                        {
                            // Use BGRA Format and convert to ARGB for the mask
                            bufferValue = (buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
                        }

                        BitmapBuffer[i].Red = BitExtract(bufferValue, bitmapInformationHeader.RedMask);
                        BitmapBuffer[i].Green = BitExtract(bufferValue, bitmapInformationHeader.GreenMask);
                        BitmapBuffer[i].Blue = BitExtract(bufferValue, bitmapInformationHeader.BlueMask);
                        BitmapBuffer[i].Alpha = BitExtract(bufferValue, bitmapInformationHeader.AlphaMask);
                    }
                }
            }
        }
    }
}