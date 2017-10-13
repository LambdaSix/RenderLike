using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RenderLike
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// var builder = new ColorStringBuilder(64);
    /// builder.AppendColor("hello world", Color.Red);
    /// builder.AppendColor("how are you", ConsoleColor.Green);
    /// builder.AppendColor("I'm good", ConsoleColor.Red);
    /// RichConsole.Write(builder.ToString());
    /// </example>
    public class ColorStringBuilder
    {
        private char[] _buffer;
        private int _index;
        private int _curLength;

        public ColorStringBuilder(int length = 0)
        {
            _curLength = length == 0 ? 128 : length;
            _buffer = new char[_curLength];
        }

        public void Append(char c)
        {
            if (_index + 1 > _curLength)
                ReallocateBuffer();

            _buffer[_index++] = c;
        }

        public void Append(string str)
        {
            foreach (var c in str)
            {
                Append(c);
            }
        }

        public void AppendLine(string str)
        {
            foreach (var c in str)
            {
                Append(c);
                Append(Environment.NewLine);
            }
        }

        public void AppendLine(string str, Color foreColor, Color backColor, Color defaultFore, Color defaultBack)
        {
            Append(str, foreColor, backColor, defaultFore, defaultBack);
            Append(Environment.NewLine);
        }

        /// <summary>
        /// Write a section of coloured text then reset back to the white on black after.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        public void Append(string str, Color foreColor, Color backColor, Color defaultFore, Color defaultBack)
        {
            string Format(Color c) => $"{c.R};{c.G};{c.B}";

            var resetColor = $"{ColorTags.COLOR_ESCAPE}[{Format(defaultFore)}#{Format(defaultBack)}m";
            Append($"{ColorTags.COLOR_ESCAPE}[{Format(foreColor)}#{Format(backColor)}m{str}{resetColor}");
        }

        /// <inheritdoc />
        public override string ToString() => new String(_buffer.TakeWhile(s => s != '\0').ToArray());

        private void ReallocateBuffer()
        {
            // Increase the size of the buffer
            _curLength = (int)Math.Ceiling(_curLength * 1.2);
            char[] tempBuff = new char[_curLength];
            Array.Copy(_buffer, tempBuff, _buffer.Length);
            _buffer = tempBuff;
        }
    }
}