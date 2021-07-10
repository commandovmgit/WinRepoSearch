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

    public class WinRepoPSHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
        public override Size BufferSize
        {
            get => new(Console.BufferWidth, Console.BufferHeight);
            set
            {
                var (width, height) = (value.Width, value.Height);

                Console.BufferHeight = height;
                Console.BufferWidth = width;
            }
        }
        public override Coordinates CursorPosition
        {
            get => new(Console.CursorLeft, Console.CursorTop);
            set
            {
                var (left, top) = (value.X, value.Y);

                Console.CursorLeft = left;
                Console.CursorTop = top;
            }
        }
        public override int CursorSize { get => Console.CursorSize; set => Console.CursorSize = value; }
        public override ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }

        public override bool KeyAvailable => Console.KeyAvailable;

        public override Size MaxPhysicalWindowSize => new(Console.LargestWindowWidth, Console.LargestWindowHeight);

        public override Size MaxWindowSize => MaxPhysicalWindowSize;

        public override Coordinates WindowPosition
        {
            get => new(Console.WindowLeft, Console.WindowTop);
            set
            {
                var (left, top) = (value.X, value.Y);

                Console.WindowLeft = left;
                Console.WindowTop = top;
            }
        }
        public override Size WindowSize
        {
            get => new(Console.WindowWidth, Console.WindowHeight);
            set
            {
                var (width, height) = (value.Width, value.Height);

                Console.WindowHeight = height;
                Console.WindowWidth = width;
            }
        }
        public override string WindowTitle { get => Console.Title ?? "<untitled>"; set => Console.Title = value; }

        public override void FlushInputBuffer()
        {
            Console.In.ReadToEnd();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            var text = ConsoleReader
                .ReadFromBuffer((short)rectangle.Left,
                    (short)rectangle.Top,
                    (short)(rectangle.Right - rectangle.Left),
                    (short)(rectangle.Top - rectangle.Bottom))
                .SelectMany(s => s.ToCharArray())
                .ToImmutableArray();

            var result = new BufferCell[rectangle.Right - rectangle.Left, rectangle.Top - rectangle.Bottom];

            for (int i = 0; i < text.Length; ++i)
            {
                var y = i / (rectangle.Top - rectangle.Bottom);
                var x = i % (rectangle.Top - rectangle.Bottom);

                result[x, y] = new BufferCell()
                {
                    Character = text[i],
                    ForegroundColor = ForegroundColor,
                    BackgroundColor = BackgroundColor
                };
            }

            return result;
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            var key = Console.ReadKey();
            return new KeyInfo(key.KeyChar, key.KeyChar, CalculateKeyState(key), false);
        }

        private ControlKeyStates CalculateKeyState(ConsoleKeyInfo key)
        {
            ControlKeyStates result = default;

            if ((key.Modifiers | ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            {
                result |= ControlKeyStates.RightAltPressed;
                result |= ControlKeyStates.LeftAltPressed;
            }

            if ((key.Modifiers | ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                result |= ControlKeyStates.LeftCtrlPressed;
                result |= ControlKeyStates.RightCtrlPressed;
            }

            if ((key.Modifiers | ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            {
                result |= ControlKeyStates.ShiftPressed;
            }

            return result;
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }
    }
}
