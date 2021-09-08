using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32.System.Console;

using static Windows.Win32.PInvoke;

namespace KuCoinConsole
{
    public class ConsoleEmulator
    {
        /// <summary>
        /// Represents a system indexed color map.
        /// </summary>
        public class ColorTableColor
        {
            public Color Color { get; set; }

            public int Index { get; set; }

            public string Name { get; set; }

            public ColorTableColor(int color, int index, string name)
            {
                Index = index;
                Color = Color.FromArgb(color);
                Name = name;
            }

            public ColorTableColor()
            {
            }

        }

        public List<ColorTableColor> Colors { get; private set; }

        /// <summary>
        /// Writes the current value of the <see cref="Colors"/> collection to the system console properties.
        /// </summary>
        public void UpdateSystemColors()
        {
            var h = GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
            var lpInfo = new CONSOLE_SCREEN_BUFFER_INFOEX();
            lpInfo.cbSize = (uint)Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFOEX>();

            var handle = new SafeFileHandle(h, false);
            GetConsoleScreenBufferInfoEx(handle, ref lpInfo);

            foreach (var color in Colors)
            {
                lpInfo.ColorTable[color.Index] = (uint)color.Color.ToArgb();
            }

            SetConsoleScreenBufferInfoEx(handle, lpInfo);
        }

        /// <summary>
        /// Refresh the color values from the system.
        /// </summary>
        public void RefreshColors()
        {
            var h = GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
            var lpInfo = new CONSOLE_SCREEN_BUFFER_INFOEX();
            lpInfo.cbSize = (uint)Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFOEX>();

            var handle = new SafeFileHandle(h, false);
            GetConsoleScreenBufferInfoEx(handle, ref lpInfo);
            Colors = new List<ColorTableColor>();
            //
            // Summary:
            //     The color black.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[0], 0, "Black"));
            //
            // Summary:
            //     The color dark blue.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[1], 1, "DarkBlue"));
            //
            // Summary:
            //     The color dark green.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[2], 2, "DarkGreen"));
            //
            // Summary:
            //     The color dark cyan (dark blue-green).
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[3], 3, "DarkCyan"));
            //
            // Summary:
            //     The color dark red.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[4], 4, "DarkRed"));
            //
            // Summary:
            //     The color dark magenta (dark purplish-red).
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[5], 5, "DarkMagenta"));
            //
            // Summary:
            //     The color dark yellow (ochre).
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[6], 6, "DarkYellow"));
            //
            // Summary:
            //     The color gray.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[7], 7, "Gray"));
            //
            // Summary:
            //     The color dark gray.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[8], 8, "DarkGray"));
            //
            // Summary:
            //     The color blue.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[9], 9, "Blue"));
            //
            // Summary:
            //     The color green.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[10], 10, "Green"));
            //
            // Summary:
            //     The color cyan (blue-green).
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[11], 11, "Cyan"));
            //
            // Summary:
            //     The color red.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[12], 12, "Red"));
            //
            // Summary:
            //     The color magenta (purplish-red).
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[13], 13, "Magenta"));
            //
            // Summary:
            //     The color yellow.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[14], 14, "Yellow"));
            //
            // Summary:
            //     The color white.
            Colors.Add(new ColorTableColor((int)lpInfo.ColorTable[15], 15, "White"));

        }

        public ConsoleEmulator()
        {
            RefreshColors();
        }

    }
}
