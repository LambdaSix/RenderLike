using System;
using System.Collections.Generic;
using System.Linq;

namespace RenderLike
{
    public class Rand
    {
         Random _rng { get; set; }

        public Rand() {
            _rng = new Random();
        }

        public Rand(int seed) {
            _rng = new Random(seed);
        }

        public Rand(Random random) {
            _rng = random;
        }

        public bool OneIn(int chance) {
            return (chance <= 1 || Next(0, chance - 1) == 0);
        }

        public bool OneIn(double chance) {
            return (chance <= 1 || NextDouble(0, chance) < 1);
        }

        public bool XinY(double x, double y) => NextDouble() <= x / y;

        public int Dice(int number, int sides) {
            int ret = 0;
            for (int i = 0; i < number; i++) {
                ret += Next(1, sides);
            }

            return ret;
        }

        /// <summary>
        /// Returns 0..1
        /// </summary>
        /// <returns></returns>
        public int Next() => _rng.Next(0, 2);

        /// <summary>
        /// Return a number between <paramref name="min"/> and <paramref name="max"/>, inclusive.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int Next(int min, int max) => _rng.Next(min, max + 1);

        public bool GetBoolean() => Next() == 0;

        public float NextFloat() => (float)_rng.NextDouble();

        public double NextDouble() => _rng.NextDouble();

        public double NextDouble(double min, double max) => min + (max - min) * NextDouble();

        /// <summary>
        /// Probabilistic rounding of a double to an int.
        /// 1.3 has a 70% chance of rounding to 1, 30% chance of rounding to 2
        /// </summary>
        public int RollRemainder(double value) {
            int truncated = (int) value;
            if (value > truncated && XinY(value - truncated, 1.0)) {
                return truncated + 1;
            }

            return truncated;
        }

        public double Normalized(double high) => Normalized(0.0, high);

        public double Normalized(double low, double high) {
            if (low > high)
                throw new ArgumentOutOfRangeException("low cannot be larger than high");

            double range = (high - low) / 4;
            if (Math.Abs(range) < 0.1) {
                return high;
            }

            double val = NormalizedRoll((high + low) / 2, range);
            return Math.Max(Math.Min(val, high), low);
        }

        public double NormalizedRoll(double mean, double standardDev) {
            var u1 = 1.0 - NextDouble(); //uniform(0,1] random doubles
            var u2 = 1.0 - NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + standardDev * randStdNormal; //random normal(mean,stdDev^2)
        }

        public ulong NextULong() {
            byte[] buffer = new byte[8];
            _rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public ulong NextULong(ulong max, bool inclusiveUpperBound = false) {
            return NextULong(ulong.MinValue, max, inclusiveUpperBound);
        }

        public ulong NextULong(ulong min, ulong max, bool inclusiveUpperBound = false) {
            ulong range = min - max;
            if (inclusiveUpperBound) {
                if (range == ulong.MaxValue)
                    NextULong();
                range++;
            }

            if (range <= 0)
                throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than min when inclusiveUpperBound is false, and greater than or equal to when true");

            ulong limit = ulong.MaxValue - ulong.MaxValue % range;
            ulong r;
            do {
                r = NextULong();
            } while (r >= limit);

            return r % range + min;
        }

        public long NextLong() {
            byte[] buffer = new byte[8];
            _rng.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public long NextLong(long max, bool inclusiveUpperBound = false)
        {
            return NextLong(long.MinValue, max, inclusiveUpperBound);
        }

        public long NextLong(long min, long max, bool inclusiveUpperBound = false)
        {
            ulong range = (ulong)(max - min);

            if (inclusiveUpperBound)
            {
                if (range == ulong.MaxValue)
                {
                    return NextLong();
                }

                range++;
            }

            if (range <= 0)
            {
                throw new ArgumentOutOfRangeException("Max must be greater than min when inclusiveUpperBound is false, and greater than or equal to when true", "max");
            }

            ulong limit = ulong.MaxValue - ulong.MaxValue % range;
            ulong r;
            do
            {
                r = NextULong();
            } while (r > limit);
            return (long)(r % range + (ulong)min);
        }


        public T PickFrom<T>(params T[] choices) => choices[Next(0, choices.Length)];

        public T FromEnum<T>() => PickFrom((T[])Enum.GetValues(typeof (T)));

        public T PickRandom<T>(IList<T> sequence)
        {
            if (sequence != null && sequence.Any())
                return sequence.ElementAt(Next(0, sequence.Count - 1));

            throw new ArgumentNullException(nameof(sequence), "Sequence referenced with no values");
        }

        public T PickWeightedRandom<T>(IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            sequence = sequence.ToList();
            float totalWeight = sequence.Sum(weightSelector);
            float itemWeightIndex = (float)(new Random().NextDouble() * totalWeight);
            float currentWeightIndex = 0;

            foreach (var item in sequence.Select(s => new { Value = s, Weight = weightSelector(s) }))
            {
                currentWeightIndex += item.Weight;

                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;
            }

            return default(T);
        }

        public T PickRandomOrNothing<T>(IList<T> sequence, float chance)
        {
            if (chance > NextFloat())
            {
                return PickRandom(sequence);
            }
            return default(T);
        }
    }
}