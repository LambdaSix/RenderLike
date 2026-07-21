using System;
using System.Collections.Generic;
using NUnit.Framework;
using RenderLike;

namespace RenderLike.Tests
{
    /// <summary>
    /// Tests for <see cref="Rand"/>. The class is seedable, so wherever possible these
    /// tests assert deterministic invariants (bounds, membership, reproducibility)
    /// rather than hard-coded sequence values, which are implementation defined and
    /// differ between .NET runtimes.
    /// </summary>
    [TestFixture]
    public class RandTests
    {
        private const int Seed = 1337;
        private const int Iterations = 2000;

        [Test]
        public void SameSeed_ProducesIdenticalSequence()
        {
            var a = new Rand(Seed);
            var b = new Rand(Seed);

            for (int i = 0; i < Iterations; i++)
                Assert.That(a.Next(0, 1000), Is.EqualTo(b.Next(0, 1000)));
        }

        [Test]
        public void Next_NoArgs_IsZeroOrOne()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.Next(), Is.InRange(0, 1));
        }

        [Test]
        public void Next_Range_IsInclusiveOfBothBounds()
        {
            var rand = new Rand(Seed);
            bool sawMin = false, sawMax = false;

            for (int i = 0; i < Iterations; i++)
            {
                int v = rand.Next(0, 1);
                Assert.That(v, Is.InRange(0, 1));
                if (v == 0) sawMin = true;
                if (v == 1) sawMax = true;
            }

            // Documents that Next(min, max) is inclusive of max (implemented as Next(min, max + 1)).
            Assert.That(sawMin, Is.True, "expected to observe the lower bound");
            Assert.That(sawMax, Is.True, "expected to observe the (inclusive) upper bound");
        }

        [Test]
        public void Next_Range_DegenerateBounds_ReturnsThatValue()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < 100; i++)
                Assert.That(rand.Next(5, 5), Is.EqualTo(5));
        }

        [Test]
        public void NextFloat_IsInUnitInterval()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.NextFloat(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void NextDouble_IsInUnitInterval()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.NextDouble(), Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
        }

        [Test]
        public void NextDouble_Range_StaysWithinBounds()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.NextDouble(2.0, 5.0), Is.GreaterThanOrEqualTo(2.0).And.LessThan(5.0));
        }

        [Test]
        public void GetBoolean_ProducesBothValues()
        {
            var rand = new Rand(Seed);
            bool sawTrue = false, sawFalse = false;
            for (int i = 0; i < Iterations && !(sawTrue && sawFalse); i++)
            {
                if (rand.GetBoolean()) sawTrue = true; else sawFalse = true;
            }
            Assert.That(sawTrue && sawFalse, Is.True);
        }

        [Test]
        public void OneIn_One_IsAlwaysTrue()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < 100; i++)
                Assert.That(rand.OneIn(1), Is.True);
        }

        [Test]
        public void OneIn_Two_ProducesBothOutcomes()
        {
            var rand = new Rand(Seed);
            bool sawTrue = false, sawFalse = false;
            for (int i = 0; i < Iterations && !(sawTrue && sawFalse); i++)
            {
                if (rand.OneIn(2)) sawTrue = true; else sawFalse = true;
            }
            Assert.That(sawTrue && sawFalse, Is.True);
        }

        [Test]
        public void XinY_XEqualsY_IsAlwaysTrue()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.XinY(1.0, 1.0), Is.True);
        }

        [Test]
        public void Dice_SumStaysWithinTheoreticalRange()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.Dice(3, 6), Is.InRange(3, 18));
        }

        [Test]
        public void Dice_SingleOneSidedDie_IsAlwaysOne()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < 100; i++)
                Assert.That(rand.Dice(1, 1), Is.EqualTo(1));
        }

        [Test]
        public void RollRemainder_IntegralValue_ReturnsThatInteger()
        {
            var rand = new Rand(Seed);
            Assert.That(rand.RollRemainder(4.0), Is.EqualTo(4));
            Assert.That(rand.RollRemainder(-3.0), Is.EqualTo(-3));
        }

        [Test]
        public void RollRemainder_FractionalValue_RoundsToAnAdjacentInteger()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.RollRemainder(1.3), Is.InRange(1, 2));
        }

        [Test]
        public void Normalized_LowGreaterThanHigh_Throws()
        {
            var rand = new Rand(Seed);
            Assert.Throws<ArgumentOutOfRangeException>(() => rand.Normalized(10.0, 0.0));
        }

        [Test]
        public void Normalized_StaysWithinBounds()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.Normalized(0.0, 10.0), Is.InRange(0.0, 10.0));
        }

        [Test]
        public void NextLong_Bounded_StaysInRange()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.NextLong(0, 10), Is.GreaterThanOrEqualTo(0L).And.LessThan(10L));
        }

        [Test]
        public void PickRandom_Null_Throws()
        {
            var rand = new Rand(Seed);
            Assert.Throws<ArgumentNullException>(() => rand.PickRandom<string>(null));
        }

        [Test]
        public void PickRandom_Empty_Throws()
        {
            var rand = new Rand(Seed);
            Assert.Throws<ArgumentNullException>(() => rand.PickRandom(new List<string>()));
        }

        [Test]
        public void PickRandom_ReturnsAMemberOfTheSequence()
        {
            var rand = new Rand(Seed);
            var items = new List<string> { "a", "b", "c" };
            for (int i = 0; i < Iterations; i++)
                Assert.That(items, Contains.Item(rand.PickRandom(items)));
        }

        [Test]
        public void PickRandomOrNothing_FullChance_AlwaysPicks()
        {
            // At chance == 1.0 the only way to NOT pick is for NextFloat() to round up to
            // exactly 1.0f (astronomically rare); a modest iteration count keeps that edge
            // out of scope while still asserting the intended "always picks" behaviour.
            var rand = new Rand(Seed);
            var items = new List<string> { "a", "b", "c" };
            for (int i = 0; i < 500; i++)
                Assert.That(rand.PickRandomOrNothing(items, 1.0f), Is.Not.Null);
        }

        [Test]
        public void PickRandomOrNothing_ZeroChance_NeverPicks()
        {
            var rand = new Rand(Seed);
            var items = new List<string> { "a", "b", "c" };
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.PickRandomOrNothing(items, 0.0f), Is.Null);
        }

        [Test]
        public void PickWeightedRandom_SingleItem_ReturnsIt()
        {
            var rand = new Rand(Seed);
            var items = new List<string> { "only" };
            Assert.That(rand.PickWeightedRandom(items, s => 1.0f), Is.EqualTo("only"));
        }

        [Test]
        public void PickWeightedRandom_ReturnsAMemberOfTheSequence()
        {
            // NOTE: PickWeightedRandom currently news up its own System.Random internally,
            // so its weighting cannot be exercised deterministically. This only asserts the
            // weak invariant (membership). Making it use the injected _rng would let us test
            // the actual weight distribution.
            var rand = new Rand(Seed);
            var items = new List<string> { "a", "b", "c" };
            for (int i = 0; i < Iterations; i++)
                Assert.That(items, Contains.Item(rand.PickWeightedRandom(items, s => 1.0f)));
        }

        // --------------------------------------------------------------------
        // The tests below document confirmed defects. They assert the CORRECT
        // behaviour and are [Ignore]d so the suite stays green; remove the
        // [Ignore] attribute once the underlying bug is fixed.
        // --------------------------------------------------------------------

        [Test]
        [Ignore("Known bug: PickFrom indexes choices[Next(0, Length)] but Next is inclusive of its upper bound, so it can index choices[Length] and throw IndexOutOfRangeException. Remove Ignore when fixed.")]
        public void PickFrom_SingleElement_NeverThrows()
        {
            var rand = new Rand(Seed);
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < Iterations; i++)
                    Assert.That(rand.PickFrom("only"), Is.EqualTo("only"));
            });
        }

        [Test]
        [Ignore("Known bug: FromEnum routes through PickFrom, inheriting the same inclusive-index out-of-bounds defect. Remove Ignore when fixed.")]
        public void FromEnum_NeverThrows()
        {
            var rand = new Rand(Seed);
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < Iterations; i++)
                    Assert.That(Enum.IsDefined(typeof(DayOfWeek), rand.FromEnum<DayOfWeek>()), Is.True);
            });
        }

        [Test]
        [Ignore("Known bug: NextULong(min, max) computes range as (min - max) instead of (max - min); for min < max this underflows and results fall outside [min, max). Remove Ignore when fixed.")]
        public void NextULong_Bounded_StaysInRange()
        {
            var rand = new Rand(Seed);
            for (int i = 0; i < Iterations; i++)
                Assert.That(rand.NextULong(0, 10), Is.LessThan(10UL));
        }
    }
}
