using NUnit.Framework;
using RenderLike;

namespace RenderLike.Tests
{
    /// <summary>
    /// Boundary tests for the <see cref="CharExtensions"/> ctype-style predicates.
    /// A couple of the assertions are characterization tests that pin down where the
    /// implementation deliberately (or incidentally) diverges from the C++ &lt;cctype&gt;
    /// functions it claims to mirror; those are called out inline.
    /// </summary>
    [TestFixture]
    public class CharExtensionsTests
    {
        [TestCase('\0')]
        [TestCase('\t')]
        [TestCase('\n')]
        [TestCase('\r')]
        [TestCase((char)0x1F)]
        public void IsControl_True_ForC0Codes(char c)
        {
            Assert.That(c.isControl(), Is.True);
        }

        [TestCase(' ')]
        [TestCase('A')]
        [TestCase('~')]
        public void IsControl_False_ForPrintable(char c)
        {
            Assert.That(c.isControl(), Is.False);
        }

        [Test]
        public void IsControl_Del_ReturnsFalse_DivergesFromStdIsCntrl()
        {
            // std::iscntrl treats 0x7F (DEL) as a control character; this implementation does not.
            Assert.That(((char)0x7F).isControl(), Is.False);
        }

        [TestCase(' ')]
        [TestCase('A')]
        [TestCase('0')]
        [TestCase('~')]
        public void IsPrintable_True_ForVisibleAsciiAndSpace(char c)
        {
            Assert.That(c.isPrintable(), Is.True);
        }

        [TestCase((char)0x1F)]
        [TestCase((char)0x7F)]
        public void IsPrintable_False_OutsidePrintableRange(char c)
        {
            Assert.That(c.isPrintable(), Is.False);
        }

        [Test]
        public void IsGraph_Space_ReturnsTrue_DivergesFromStdIsGraph()
        {
            // std::isgraph excludes space; here isGraph is an alias of isPrintable, which includes it.
            Assert.That(' '.isGraph(), Is.True);
        }

        [TestCase(' ', true)]
        [TestCase('\n', true)]
        [TestCase('\t', true)]
        [TestCase('\r', true)]
        [TestCase('\f', true)]
        [TestCase('\v', true)]
        [TestCase('A', false)]
        [TestCase('0', false)]
        public void IsSpace(char c, bool expected)
        {
            Assert.That(c.isSpace(), Is.EqualTo(expected));
        }

        [TestCase(' ', true)]
        [TestCase('\t', true)]
        [TestCase('\n', false)]
        [TestCase('A', false)]
        public void IsBlank_OnlySpaceAndHorizontalTab(char c, bool expected)
        {
            Assert.That(c.isBlank(), Is.EqualTo(expected));
        }

        [TestCase('!', true)]
        [TestCase('/', true)]
        [TestCase(':', true)]
        [TestCase('@', true)]
        [TestCase('[', true)]
        [TestCase('`', true)]
        [TestCase('~', true)]
        [TestCase('A', false)]
        [TestCase('0', false)]
        [TestCase(' ', false)]
        public void IsPunctuation(char c, bool expected)
        {
            Assert.That(c.isPunctuation(), Is.EqualTo(expected));
        }

        [TestCase('A', true)]
        [TestCase('z', true)]
        [TestCase('5', true)]
        [TestCase('!', false)]
        [TestCase(' ', false)]
        public void IsAlphaNumeric(char c, bool expected)
        {
            Assert.That(c.isAlphaNumeric(), Is.EqualTo(expected));
        }

        [TestCase('A', true)]
        [TestCase('Z', true)]
        [TestCase('a', true)]
        [TestCase('z', true)]
        [TestCase('0', false)]
        [TestCase('@', false)]
        [TestCase('[', false)]
        public void IsAlpha(char c, bool expected)
        {
            Assert.That(c.isAlpha(), Is.EqualTo(expected));
        }

        [TestCase('A', true)]
        [TestCase('Z', true)]
        [TestCase('a', false)]
        [TestCase('@', false)]
        public void IsUpper_BoundariesAreExclusiveOfNeighbours(char c, bool expected)
        {
            Assert.That(c.isUpper(), Is.EqualTo(expected));
        }

        [TestCase('a', true)]
        [TestCase('z', true)]
        [TestCase('A', false)]
        [TestCase('{', false)]
        public void IsLower_BoundariesAreExclusiveOfNeighbours(char c, bool expected)
        {
            Assert.That(c.isLower(), Is.EqualTo(expected));
        }

        [TestCase('0', true)]
        [TestCase('9', true)]
        [TestCase('/', false)]
        [TestCase(':', false)]
        [TestCase('a', false)]
        public void IsDigit_BoundariesAreExclusiveOfNeighbours(char c, bool expected)
        {
            Assert.That(c.isDigit(), Is.EqualTo(expected));
        }

        [TestCase('0', true)]
        [TestCase('9', true)]
        [TestCase('a', true)]
        [TestCase('f', true)]
        [TestCase('A', true)]
        [TestCase('F', true)]
        [TestCase('g', false)]
        [TestCase('G', false)]
        public void IsXDigit(char c, bool expected)
        {
            Assert.That(c.isXDigit(), Is.EqualTo(expected));
        }
    }
}
