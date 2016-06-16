﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RenderLike.Namegen;

namespace RenderLike.Tests.Namegen
{
    [TestFixture]
    public class NameGeneratorTests
    {
        [Test]
        public void GeneratesOutput()
        {
            var path = TestContext.CurrentContext.TestDirectory;
            var nameGen = new NameGenerator(Path.Combine(path, @".\Data\Namegen"));

            string output = "";
            Assert.DoesNotThrow(() => output = nameGen.GenerateFromRule("Demon Female"));

            Assert.That(output, Is.Not.Empty);
            Console.WriteLine($"Output: {output}");

            var names = String.Join(",", Enumerable.Range(0, 10).Select(s => nameGen.GenerateFromRule("Demon Female")));
            Console.WriteLine($"Names: {names}");
        }
    }

    [TestFixture]
    public class NamegenTokenParserTests
    {
        [Test]
        public void CreatesTokenSequenceLiterals()
        {
            var parser = new NamegenTokenParser();
            var inputString = @"Hello World";

            var result = parser.ParseString(inputString).ToList();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.Cast<LiteralToken>().FirstOrDefault()?.Literal, Is.EqualTo("Hello World"));
        }

        [Test]
        public void CreatesTokenSequences()
        {
            var parser = new NamegenTokenParser();
            var inputString = @"$m'$50?$25v";

            var result = parser.ParseString(inputString).ToList();

            Assert.That(result.Count(), Is.EqualTo(4));

            var first = result.First();
            Assert.That(first.Type, Is.EqualTo(TokenType.MiddleSyllable));
            Assert.That(first.Chance, Is.EqualTo(0));

            var second = result.ElementAt(1);
            Assert.That(second.Type, Is.EqualTo(TokenType.Literal));
            Assert.That(second.Chance, Is.EqualTo(0));

            var secondLiteral = second as LiteralToken;
            Assert.That(secondLiteral?.Literal == "'");

            var third = result.ElementAt(2);
            Assert.That(third.Type, Is.EqualTo(TokenType.Phoneme));
            Assert.That(third.Chance, Is.EqualTo(50));

            var fourth = result.ElementAt(3);
            Assert.That(fourth.Type, Is.EqualTo(TokenType.Vocal));
            Assert.That(fourth.Chance, Is.EqualTo(25));
        }
    }
}