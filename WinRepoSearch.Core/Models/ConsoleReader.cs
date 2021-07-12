using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WinRepoSearch.Core.Models
{

    // https://stackoverflow.com/questions/12355378/read-from-location-on-console-c-sharp
    public class ConsoleReader
    {
        public static IEnumerable<string> ReadFromBuffer(short x, short y, short width, short height)
        {
            IntPtr? bfr = Marshal.AllocHGlobal(width * height * Marshal.SizeOf(typeof(CHAR_INFO)));
            if (bfr is null || bfr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            IntPtr buffer = bfr.Value;

            try
            {
                COORD coord = new();
                SMALL_RECT rc = new();
                rc.Left = x;
                rc.Top = y;
                rc.Right = (short)(x + width - 1);
                rc.Bottom = (short)(y + height - 1);

                COORD size = new();
                size.X = width;
                size.Y = height;

                const int STD_OUTPUT_HANDLE = -11;
                if (!ReadConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), buffer, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                IntPtr ptr = buffer;
                for (int h = 0; h < height; h++)
                {
                    StringBuilder sb = new();
                    for (int w = 0; w < width; w++)
                    {
                        CHAR_INFO ci = (CHAR_INFO)Marshal.PtrToStructure(ptr, typeof(CHAR_INFO));
                        char[] chars = Console.OutputEncoding.GetChars(ci.charData);
                        sb.Append(chars[0]);
                        ptr += Marshal.SizeOf(typeof(CHAR_INFO));
                    }
                    yield return sb.ToString();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHAR_INFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] charData;
            public short attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, IntPtr lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
    }
}
