using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RenderLike {

    /// <summary>
    /// Used by the ColourStringBuilder & Surface::WriteString
    /// </summary>
    internal static class ColorTags
    {
        internal static char COLOR_ESCAPE { get; } = '\x1B';
    }

    /// <summary>
    /// Horizontal alignment used by various string printing methods
    /// </summary>
    public enum HorizontalAligment {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Vertical alignment used by various string printing methods
    /// </summary>
    public enum VerticalAlignment {
        Top,
        Center,
        Bottom
    }

    /// <summary>
    /// Wrapping mode used by various string printing methods
    /// </summary>
    public enum WrappingType {
        /// <summary>
        /// No wrapping is performed - characters will be trimmed if too long to fit
        /// </summary>
        None,

        /// <summary>
        /// String is wrapped to new line if too long to fit
        /// </summary>
        Character,

        /// <summary>
        /// String is wrapped to new line if too long to fit, respecting word boundaries (spaces)
        /// </summary>
        Word
    }

    internal struct Cell {
        public Color Back;
        public char Char;
        public Color Fore;

        public override string ToString() {
            return $"Tile: {Char}";
        }
    }

    public class Surface {
        internal readonly Cell[] Cells;
        internal bool[] DirtyCells;
        internal Font Font;
        internal readonly RLConsole ParentConsole;

        internal Surface(int width, int height, Font font, RLConsole parent, int index) {
            Width = width;
            Height = height;
            Index = index;

            Font = font;
            ParentConsole = parent;

            Rect = new Rectangle(0, 0, width, height);

            Cells = new Cell[width*height];
            DirtyCells = new bool[width*height];

            DefaultBackground = Color.Black;
            DefaultForeground = Color.White;

            for (int i = 0; i < width*height; i++) {
                Cells[i].Back = DefaultBackground;
                Cells[i].Char = ' ';
                Cells[i].Fore = DefaultForeground;
            }
        }

        /// <summary>
        ///     Width of the surface in number of character cells
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        ///     Height of surface in number of character cells
        /// </summary>
        public int Height { get; private set; }

        public int Index { get; set; }

        public Rectangle Rect { get; private set; }

        /// <summary>
        ///     The default background color used by drawing methods when not specified
        /// </summary>
        public Color DefaultBackground { get; set; }

        /// <summary>
        ///     The default foreground color used by drawing methods when not specified
        /// </summary>
        public Color DefaultForeground { get; set; }

        /// <summary>
        ///     Returns the character at the given position in cell coordinates.
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public char GetChar(int x, int y) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            return Cells[x + y*Width].Char;
        }

        /// <summary>
        ///     Returns the foreground color at the given position in cell coordinates.
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetForeground(int x, int y) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            return Cells[x + y*Width].Fore;
        }

        /// <summary>
        ///     Returns the background color at the given position in cell coordinates.
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetBackground(int x, int y) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            return Cells[x + y*Width].Back;
        }

        /// <summary>
        ///     Changes the foreground color of the specified cell without affecting the character or background
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fore"></param>
        public void SetForeground(int x, int y, Color fore) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            SetCell(x, y, null, fore, null);
        }

        /// <summary>
        ///     Changes the background color of the specified cell without affecting the character or foreground
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="back"></param>
        public void SetBackground(int x, int y, Color back) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            SetCell(x, y, null, null, back);
        }

        /// <summary>
        ///     Changes the character at the specified cell without affecting the foreground or background
        ///     Position must be within the surface boundaries or this method will throw an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public void SetChar(int x, int y, char c) {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            SetCell(x, y, c, null, null);
        }

        /// <summary>
        ///     Prints the specified character using specified foreground and background colors.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void PrintChar(int x, int y, char c, Color fore, Color back) {
            SetCell(x, y, c, fore, back);
        }

        /// <summary>
        ///     Prints the character using the specified foreground and default background colors
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        public void PrintChar(int x, int y, char c, Color fore) {
            SetCell(x, y, c, fore, DefaultBackground);
        }

        /// <summary>
        ///     Prints the character using the default background and foreground colors
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public void PrintChar(int x, int y, char c) {
            SetCell(x, y, c, DefaultForeground, DefaultBackground);
        }

        /// <summary>
        ///     Prints the string using the specified foreground and background colors
        ///     If the string contains colour escaping values, those colours will be used
        ///     for those sections and the foreground & background colours passed as params
        ///     will be otherwise used.
        /// </summary>
        /// <param name="x">X position to start drawing at</param>
        /// <param name="y">Y position to start drawing at</param>
        /// <param name="str">The string to print, optionally with colour escape sequences</param>
        /// <param name="fore">Foreground colour to use for text without colour instructions</param>
        /// <param name="back">Background colour to use for text without colour instructions</param>
        public void PrintString(int x, int y, string str, Color fore, Color back) {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            
            // int f(string,int)
            int ExtractColor(string s, ref int idx, out bool terminatorFound, out bool backgroundFound)
            {
                terminatorFound = false;
                backgroundFound = false;

                var buff = new StringBuilder(3);
                while (s[idx] != ';')
                {
                    // Might be a foreground specification only, so check for the end of the entire section
                    if (s[idx] == 'm')
                    {
                        terminatorFound = true;
                        break;
                    }

                    if (s[idx] == '#')
                    {
                        backgroundFound = true;
                        break;
                    }

                    buff.Append(str[idx]);
                    idx++;
                }
                idx++; // Skip the ;
                return Int32.Parse(buff.ToString());
            }

            Color overrideForeColor = fore;
            Color overrideBackColor = back;

            int i = 0;
            int xI = 0; // for printable text.

            // Expected format: '\x1B[255;0;0#0;0;0mRed Text\x1B[0;0;0#0;0;0m'
            // '\x1B[FORE_RED;FORE_GREEN;FORE_BLUE#BACK_RED;BACK_GREEN;BACK_BLUEm' -- specify fore/back colours
            // '\x1B[FORE_RED;FORE_GREEN;FORE_BLUEm' -- default to black background or whatever 'back' is passed as
            // '\x1B[0m' -- reset colour to the values passed in fore/back
            while (true)
            {
                if (i >= str.Length)
                    break;

                if (str[i] == ColorTags.COLOR_ESCAPE)
                {
                    i++; // Move over the COLOR_ESCAPE

                    // Early check for the reset instruction
                    if (str.Substring(i, 3) == "[0m")
                    {
                        overrideForeColor = fore;
                        overrideBackColor = back;
                        i += 3;
                        continue;
                    }

                    if (str[i] == '[')
                    {
                        bool terminatorFound;
                        bool backgroundFound;

                        i++; // skip the [
                        // Extract the R
                        var redForeValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);
                        var greenForeValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);
                        var blueForeValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);

                        overrideForeColor = new Color(redForeValue, greenForeValue, blueForeValue);

                        if (terminatorFound)
                            continue;

                        if (backgroundFound)
                        {
                            // Extract the background colour now.
                            var redBackValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);
                            var greenBackValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);
                            var blueBackValue = ExtractColor(str, ref i, out terminatorFound, out backgroundFound);

                            overrideBackColor = new Color(redBackValue, greenBackValue, blueBackValue);
                        }
                    }
                }
                else
                {
                    PrintChar(x + xI, y, str[i], overrideForeColor, overrideBackColor);
                    i++; // Increment the str indexer
                    xI++; // Increment the visual printing indexer.
                }
            }
        }

        /// <summary>
        ///     Prints a string using the default foreground and background colors
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        public void PrintString(int x, int y, string str) {
            PrintString(x, y, str, DefaultForeground, DefaultBackground);
        }

        /// <summary>
        ///     Prints a string using the specified foreground and default background colors
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        /// <param name="fore"></param>
        public void PrintString(int x, int y, string str, Color fore) {
            PrintString(x, y, str, fore, DefaultBackground);
        }

        /// <summary>
        ///     Prints the string within the specified rectangle, with the specified alignments and wrapping
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="str"></param>
        /// <param name="hAlign"></param>
        /// <param name="vAlign"></param>
        /// <param name="wrapping"></param>
        public void PrintStringRect(Rectangle rect, string str, HorizontalAligment hAlign, VerticalAlignment vAlign,
            WrappingType wrapping) {
            if (str == null)
                throw new ArgumentNullException("str");

            switch (wrapping) {
                case WrappingType.None:
                    if (str.Length > rect.Width) {
                        str = str.Substring(0, rect.Width);
                    }

                    int nx = rect.X + GetHorizontalDelta(str.Length, rect.Width, hAlign);
                    int ny = rect.Y + GetVerticalDelta(1, rect.Height, vAlign);

                    PrintString(nx, ny, str);
                    break;

                case WrappingType.Character:
                    string[] stringlist = GetCharWrappedLines(str, rect.Width);

                    for (int i = 0; i < stringlist.Length; i++) {
                        str = stringlist[i];

                        nx = rect.X + GetHorizontalDelta(str.Length, rect.Width, hAlign);
                        ny = i + rect.Y + GetVerticalDelta(stringlist.Length, rect.Height, vAlign);

                        // note XNA.Rectangle.Bottom is EXCLUSIVE!
                        if (ny >= rect.Top && ny < rect.Bottom) {
                            PrintString(nx, ny, str);
                        }
                    }
                    break;

                case WrappingType.Word:
                    stringlist = GetWordWrappedLines(str, rect.Width);

                    for (int i = 0; i < stringlist.Length; i++) {
                        str = stringlist[i];

                        nx = rect.X + GetHorizontalDelta(str.Length, rect.Width, hAlign);
                        ny = i + rect.Y + GetVerticalDelta(stringlist.Length, rect.Height, vAlign);

                        // note XNA.Rectangle.Bottom is EXCLUSIVE!
                        if (ny >= rect.Top && ny < rect.Bottom) {
                            PrintString(nx, ny, str);
                        }
                    }

                    break;
            }
        }

        /// <summary>
        ///     Prints the string within the specified rectangle, with the specified alignments.  No wrapping is performed.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="str"></param>
        /// <param name="hAlign"></param>
        /// <param name="vAlign"></param>
        public void PrintStringRect(Rectangle rect, string str, HorizontalAligment hAlign,
            VerticalAlignment vAlign = VerticalAlignment.Top) {
            PrintStringRect(rect, str, hAlign, vAlign, WrappingType.None);
        }

        /// <summary>
        ///     Clears the surface, setting each cell character to space and each cell to the default foreground and background
        ///     colors.
        /// </summary>
        public void Clear() {
            ClearSurface();
        }

        /// <summary>
        ///     Fills the specified rectangle with the character and colors specified.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void Fill(Rectangle rect, char c, Color fore, Color back) {
            for (int y = rect.Top; y < rect.Bottom; y++) {
                for (int x = rect.Left; x < rect.Right; x++) {
                    SetCell(x, y, c, fore, back);
                }
            }
        }

        /// <summary>
        ///     Fills a rectangle with the specified character using the default foreground and background colors.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        public void Fill(Rectangle rect, char c) {
            Fill(rect, c, DefaultForeground, DefaultBackground);
        }

        /// <summary>
        ///     Fills a rectangle with the specified character and foreground color and the default background color.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        public void Fill(Rectangle rect, char c, Color fore) {
            Fill(rect, c, fore, DefaultBackground);
        }

        /// <summary>
        ///     Fills a rectangle with the provided colors.  Does not change the characters within the rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void Fill(Rectangle rect, Color fore, Color back) {
            for (int y = rect.Top; y < rect.Bottom; y++) {
                for (int x = rect.Left; x < rect.Right; x++) {
                    SetCell(x, y, null, fore, back);
                }
            }
        }

        /// <summary>
        ///     Draw a horizontal line using the specified character
        /// </summary>
        /// <param name="leftX"></param>
        /// <param name="leftY"></param>
        /// <param name="length"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void DrawHorizontalLine(int leftX, int leftY, int length, char c, Color fore, Color back) {
            for (int i = 0; i < length; i++) {
                PrintChar(leftX + i, leftY, c, fore, back);
            }
        }

        /// <summary>
        ///     Draw a horizontal line using special characters, assuming one of the default font layouts (or similar) is being
        ///     used.
        /// </summary>
        /// <param name="leftX"></param>
        /// <param name="leftY"></param>
        /// <param name="length"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void DrawHorizontalLine(int leftX, int leftY, int length, Color fore, Color back) {
            DrawHorizontalLine(leftX, leftY, length, (char) SpecialChar.HorizontalLine, fore, back);
        }

        /// <summary>
        ///     Draw a horizontal line using special characters, assuming one of the default font layouts (or similar) is being
        ///     used.
        ///     The default background is used.
        /// </summary>
        /// <param name="leftX"></param>
        /// <param name="leftY"></param>
        /// <param name="length"></param>
        /// <param name="fore"></param>
        public void DrawHorizontalLine(int leftX, int leftY, int length, Color fore) {
            DrawHorizontalLine(leftX, leftY, length, fore, DefaultBackground);
        }

        /// <summary>
        ///     Draw a horizontal line using special characters, assuming one of the default font layouts (or similar) is being
        ///     used.
        ///     The default foreground and background colors are used.
        /// </summary>
        /// <param name="leftX"></param>
        /// <param name="leftY"></param>
        /// <param name="length"></param>
        public void DrawHorizontalLine(int leftX, int leftY, int length) {
            DrawHorizontalLine(leftX, leftY, length, DefaultForeground, DefaultBackground);
        }

        /// <summary>
        ///     Draw a vertical line using the specified character
        /// </summary>
        /// <param name="topX"></param>
        /// <param name="topY"></param>
        /// <param name="length"></param>
        /// <param name="c"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void DrawVerticalLine(int topX, int topY, int length, char c, Color fore, Color back) {
            for (int i = 0; i < length; i++) {
                PrintChar(topX, topY + i, c, fore, back);
            }
        }

        /// <summary>
        ///     Draw a vertical line using special characters, assuming one of the default font layouts (or similar) is being used.
        /// </summary>
        /// <param name="topX"></param>
        /// <param name="topY"></param>
        /// <param name="length"></param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void DrawVerticalLine(int topX, int topY, int length, Color fore, Color back) {
            DrawVerticalLine(topX, topY, length, (char) SpecialChar.VerticalLine, fore, back);
        }

        /// <summary>
        ///     Draw a vertical line using special characters, assuming one of the default font layouts (or similar) is being used.
        ///     The default background color is used.
        /// </summary>
        /// <param name="topX"></param>
        /// <param name="topY"></param>
        /// <param name="length"></param>
        /// <param name="fore"></param>
        public void DrawVerticalLine(int topX, int topY, int length, Color fore) {
            DrawVerticalLine(topX, topY, length, fore, DefaultBackground);
        }

        /// <summary>
        ///     Draw a vertical line using special characters, assuming one of the default font layouts (or similar) is being used.
        ///     The default foreground and background colors are used.
        /// </summary>
        /// <param name="topX"></param>
        /// <param name="topY"></param>
        /// <param name="length"></param>
        public void DrawVerticalLine(int topX, int topY, int length) {
            DrawVerticalLine(topX, topY, length, DefaultForeground, DefaultBackground);
        }

        /// <summary>
        ///     Draws a frame using special characters, assuming one of the default font layouts (or similar) is being used.
        ///     If title is not null or empty, then this string is printed a the top left corner of the frame.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="title"></param>
        /// <param name="clear">If true, clears the region inside the frame with the given back color</param>
        /// <param name="fore"></param>
        /// <param name="back"></param>
        public void DrawFrame(Rectangle rect, string title, bool clear, Color fore, Color back) {
            if (clear) {
                Fill(rect, ' ', fore, back);
            }

            DrawHorizontalLine(rect.Left, rect.Top, rect.Width - 1, fore, back);
            DrawHorizontalLine(rect.Left, rect.Bottom - 1, rect.Width - 1, fore, back);

            DrawVerticalLine(rect.Left, rect.Top, rect.Height - 1, fore, back);
            DrawVerticalLine(rect.Right - 1, rect.Top, rect.Height - 1, fore, back);

            PrintChar(rect.Left, rect.Top, (char) SpecialChar.NorthWestLine, fore, back);
            PrintChar(rect.Right - 1, rect.Top, (char) SpecialChar.NorthEastLine, fore, back);
            PrintChar(rect.Left, rect.Bottom - 1, (char) SpecialChar.SouthWestLine, fore, back);
            PrintChar(rect.Right - 1, rect.Bottom - 1, (char) SpecialChar.SouthEastLine, fore, back);

            if (!string.IsNullOrEmpty(title)) {
                PrintString(rect.Left + 1, rect.Top, title, back, fore);
            }
        }

        /// <summary>
        ///     Draws a frame using special characters, assuming one of the default font layouts (or similar) is being used.
        ///     If title is not null or empty, then this string is printed a the top left corner of the frame.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="title"></param>
        /// <param name="clear">If true, clears the region inside the frame with the default back color</param>
        public void DrawFrame(Rectangle rect, string title = null, bool clear = false) {
            DrawFrame(rect, title, clear, DefaultForeground, DefaultBackground);
        }

        private int GetHorizontalDelta(int strLength, int width, HorizontalAligment hAlign) {
            int dx;

            switch (hAlign) {
                case HorizontalAligment.Left:
                    dx = 0;
                    break;

                case HorizontalAligment.Center:
                    dx = (width - strLength)/2;
                    break;

                case HorizontalAligment.Right:
                default:
                    dx = (width - strLength);
                    break;
            }

            return dx;
        }

        private int GetVerticalDelta(int numLines, int height, VerticalAlignment vAlign) {
            int dy;

            switch (vAlign) {
                case VerticalAlignment.Top:
                    dy = 0;
                    break;

                case VerticalAlignment.Center:
                    dy = (height - numLines)/2;
                    break;

                case VerticalAlignment.Bottom:
                default:
                    dy = height - numLines;
                    break;
            }

            return dy;
        }

        private string[] GetCharWrappedLines(string str, int maxWidth) {
            var stringlist = new List<string>();

            int count = 0;
            var builder = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                builder.Append(str[i]);
                count++;
                if (count >= maxWidth) {
                    count = 0;
                    stringlist.Add(builder.ToString());
                    builder.Clear();
                }
            }
            stringlist.Add(builder.ToString());

            return stringlist.ToArray();
        }

        private string[] GetWordWrappedLines(string str, int width) {
            var lines = new List<string>();

            string[] words = Explode(str);
            int currlength = 0;
            var currentLine = new StringBuilder();

            for (int i = 0; i < words.Length; i++) {
                if (words[i].Length + currlength > width) {
                    if (currlength > 0) {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currlength = 0;
                    }

                    if (words[i].Length > width) {
                        string[] cwlines = GetCharWrappedLines(words[i], width);
                        foreach (string s in cwlines) {
                            lines.Add(s);
                        }

                        currlength = 0;
                    }
                    else {
                        currentLine.Append(words[i] + ' ');
                        currlength += words[i].Length + 1;
                    }
                }
                else {
                    currentLine.Append(words[i] + ' ');
                    currlength += words[i].Length + 1;
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines.ToArray();
        }

        private string[] Explode(string str) {
            var stringList = new List<string>();
            int currIndex = 0;
            var builder = new StringBuilder();

            while (true) {
                while (currIndex < str.Length && char.IsWhiteSpace(str[currIndex])) {
                    currIndex++;
                }

                while (currIndex < str.Length && !char.IsWhiteSpace(str[currIndex])) {
                    builder.Append(str[currIndex]);
                    currIndex++;
                }
                stringList.Add(builder.ToString());
                builder.Clear();

                if (currIndex >= str.Length)
                    break;
            }

            return stringList.ToArray();
        }

        internal virtual void SetCell(int x, int y, char? c, Color? fore, Color? back) {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            if (back.HasValue)
                Cells[x + y*Width].Back = back.Value;
            if (fore.HasValue)
                Cells[x + y*Width].Fore = fore.Value;
            if (c.HasValue)
                Cells[x + y*Width].Char = c.Value;

            // At least something chnaged, mark the cell up as dirty.
            if (back.HasValue || fore.HasValue || c.HasValue)
                DirtyCells[x + y*Width] = true;
        }

        /// <summary>
        /// Render this surface into a RenderTarget2D
        /// </summary>
        /// <returns>A drawn RenderTarget2D with this surfaces data on it</returns>
        internal virtual RenderTarget2D RenderSurface(RenderTarget2D renderTarget) {
            ParentConsole.Graphics.SetRenderTarget(renderTarget);
            ParentConsole.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            var fontSrc = new Rectangle(0, 0, 10, 10);

            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    if (DirtyCells[x + y*Width]) {
                        // Background
                        Font.SetFontSourceRect(ref fontSrc, (char) Font.SolidChar);
                        ParentConsole.SpriteBatch.Draw(Font.Texture,
                            new Vector2(x*Font.CharacterWidth, y*Font.CharacterWidth),
                            fontSrc,
                            Cells[x + y*Width].Back);

                        // Foreground
                        Font.SetFontSourceRect(ref fontSrc, Cells[x + y*Width].Char);
                        ParentConsole.SpriteBatch.Draw(Font.Texture,
                            new Vector2(x*Font.CharacterWidth, y*Font.CharacterHeight),
                            fontSrc,
                            Cells[x + y*Width].Fore);
                    }
                }
            }
            ParentConsole.SpriteBatch.End();
            ParentConsole.Graphics.SetRenderTarget(null);
            return renderTarget;
        }

        internal virtual void ClearSurface() {
            for (int i = 0; i < Width*Height; i++) {
                Cells[i].Back = DefaultBackground;
                Cells[i].Char = ' ';
                Cells[i].Fore = DefaultForeground;
            }

            Array.Clear(DirtyCells, 0, DirtyCells.Length);
        }
    }
}