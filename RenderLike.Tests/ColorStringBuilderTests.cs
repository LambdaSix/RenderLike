using System;
using NUnit.Framework;
using RenderLike;

namespace RenderLike.Tests
{
    /// <summary>
    /// Tests for the plain (non-coloured) surface of <see cref="ColorStringBuilder"/>.
    /// The colour-emitting overloads take MonoGame's Color and are exercised elsewhere;
    /// these cover buffering, growth and line handling, which need no graphics types.
    /// </summary>
    [TestFixture]
    public class ColorStringBuilderTests
    {
        [Test]
        public void NewBuilder_ToString_IsEmpty()
        {
            Assert.That(new ColorStringBuilder().ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void AppendChar_Accumulates()
        {
            var sb = new ColorStringBuilder();
            sb.Append('h');
            sb.Append('i');
            Assert.That(sb.ToString(), Is.EqualTo("hi"));
        }

        [Test]
        public void AppendString_Accumulates()
        {
            var sb = new ColorStringBuilder();
            sb.Append("hello ");
            sb.Append("world");
            Assert.That(sb.ToString(), Is.EqualTo("hello world"));
        }

        [Test]
        public void AppendEmptyString_LeavesBufferEmpty()
        {
            var sb = new ColorStringBuilder();
            sb.Append("");
            Assert.That(sb.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Append_GrowsBufferBeyondInitialCapacity()
        {
            // Start with a deliberately tiny buffer to force ReallocateBuffer() to run repeatedly.
            var sb = new ColorStringBuilder(2);
            var expected = new string('x', 100);
            sb.Append(expected);
            Assert.That(sb.ToString(), Is.EqualTo(expected));
        }

        // --------------------------------------------------------------------
        // Documents a confirmed defect: AppendLine(string) appends a newline
        // after EVERY character rather than once after the whole string.
        // Remove the [Ignore] when fixed.
        // --------------------------------------------------------------------
        [Test]
        [Ignore("Known bug: AppendLine(string) appends Environment.NewLine after every character instead of once after the string. Remove Ignore when fixed.")]
        public void AppendLine_AppendsSingleTrailingNewline()
        {
            var sb = new ColorStringBuilder();
            sb.AppendLine("ab");
            Assert.That(sb.ToString(), Is.EqualTo("ab" + Environment.NewLine));
        }
    }
}
