﻿using System;

namespace CSDS.Utilities
{
    public interface Randomness
    {
        /// <summary>
        /// Gets 64 random bits as a long, probably to be consumed by RNG.
        /// </summary>
        /// <returns>a pseudo-random long that can be positive or negative</returns>
        long Next64();
        /// <summary>
        /// Gets 32 random bits as an int, probably to be consumed by RNG.
        /// </summary>
        /// <returns>a pseudo-random int that can be positive or negative</returns>
        int Next32();
        /// <summary>
        /// Gets a byte array that can be saved or used to restore the state of this Randomness (using FromSnapshot() ).
        /// </summary>
        /// <returns>a byte array that contains enough data to restore the state of this Randomness</returns>
        byte[] GetSnapshot();
        /// <summary>
        /// Sets this Randomness' state so it matches the data stored in snapshot, which should have been
        /// obtained from an object of the same class as this one using GetSnapshot() .
        /// </summary>
        /// <param name="snapshot">a byte array produced by GetSnapshot() on an object with the same class as this</param>
        void FromSnapshot(byte[] snapshot);
        /// <summary>
        /// Returns a copy of this Randomness type with a copied state.
        /// </summary>
        /// <returns>a copy of this Randomness type with a copied state</returns>
        Randomness Copy();
    }

    public class RNG : Random
    {
        public static Random GlobalRandom = new Random();
        public Randomness Rand { get; set; }
        /// <summary>
        /// Constructs an RNG with a SplitMixRandomness, randomly seeded, as its Randomness.
        /// </summary>
        public RNG()
        {
            Rand = new SplitMixRandomness();
        }

        public RNG(long seed)
        {
            Rand = new SplitMixRandomness(seed);
        }

        /// <summary>
        /// Constructs an RNG with randomSource used as its Randomness.
        /// </summary>
        public RNG(Randomness randomSource)
        {
            Rand = randomSource;
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return Rand.Next64();
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 0 or less, this simply returns 0).
        /// </summary>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if(maxValue <= 0) return 0;
            long threshold = (0x7fffffffffffffffL - maxValue + 1) % maxValue;
            for(;;)
            {
                long bits = Rand.Next64() & 0x7fffffffffffffffL;
                if(bits >= threshold)
                    return bits % maxValue;
            }
        }
        /// <summary>
        /// Gets a random long that is between minValue (inclusive) and maxValue (exclusive);
        /// both should be positive and minValue should be less than maxValue.
        /// </summary>
        /// <param name="minValue">the lower bound as a long, inclusive</param>
        /// <param name="maxValue">the upper bound as a long, exclusive</param>
        /// <returns></returns>
        public long NextLong(long minValue, long maxValue)
        {
            return NextLong(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            return Rand.Next32();
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return Rand.Next32() & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * (Rand.Next64() & 0x7FFFFFFFL)) >> 31);
        }
        /// <summary>
        /// Gets a random int that is between minValue (inclusive) and maxValue (exclusive); both can be positive or negative.
        /// </summary>
        /// <param name="minValue">the inner bound as an int, inclusive</param>
        /// <param name="maxValue">the outer bound as an int, exclusive</param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            return Next(maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Fills buffer with random values, from its start to its end.
        /// </summary>
        /// <remarks>
        /// Based on reference code in the documentation for java.util.Random, but modified
        /// to work with 8 random bytes at a time instead of 4.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
                for(long r = Rand.Next64(), n = Math.Min(buffer.Length - i, 8); n-- > 0; r >>= 8)
                    buffer[i++] = (byte)r;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (Rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (Rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Returns a new RNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this RNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this RNG</returns>
        public RNG Copy()
        {
            return new RNG(Rand.Copy());
        }
        /// <summary>
        /// Gets a snapshot of the current state as a byte array. This snapshot can be used to restore the current state.
        /// </summary>
        /// <remarks>
        /// Normally, you get a byte array by calling this method on this RNG, and later call FromSnapshot() on this RNG and
        /// give it the earlier byte array. This can be useful for saving state, but it only works if the Randomness implementations
        /// are the same. The default Randomness is SplitMixRandomness, so if you didn't specify a different one, then the snapshots
        /// from and to those default RNGs will be compatible.
        /// </remarks>
        /// <returns>a snapshot of the current state as a byte array</returns>
        public byte[] GetSnapshot()
        {
            return Rand.GetSnapshot();
        }
        /// <summary>
        /// Restores the state this uses internally to the one stored in snapshot, a byte array.
        /// </summary>
        /// <param name="snapshot">a byte array normally produced by GetSnapshot() called on this RNG or its Randomness</param>
        public void FromSnapshot(byte[] snapshot)
        {
            Rand.FromSnapshot(snapshot);
        }
    }

    public class SplitMixRandomness : Randomness
    {
        public ulong State;

        public SplitMixRandomness()
        {
            State = (ulong)RNG.GlobalRandom.Next() >> 5 ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42;
        }

        public SplitMixRandomness(ulong state)
        {
            State = state;
        }

        public SplitMixRandomness(long state)
        {
            State = (ulong)state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 8)
                State = (ulong)(-1L - snapshot.LongLength * 421L);
            else
                State = BitConverter.ToUInt64(snapshot, 0);
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }

        public Randomness Copy()
        {
            return new SplitMixRandomness(State);
        }

        public int Next32()
        {
            ulong z = (State += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return (int)(z ^ (z >> 31));
        }

        public long Next64()
        {
            ulong z = (State += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return (long)(z ^ (z >> 31));
        }
    }

    public class RushRandomness : Randomness
    {
        public long State0, State1;

        public RushRandomness() : this(RNG.GlobalRandom.Next() << 15 ^ RNG.GlobalRandom.Next(),
            RNG.GlobalRandom.Next() << 14 ^ RNG.GlobalRandom.Next(), RNG.GlobalRandom.Next() << 16 ^ RNG.GlobalRandom.Next())
        {
        }
        public RushRandomness(long seed)
        {
            State0 = seed * -0x3943D8696D4A3B7DL - 0x7CD6391461952C1DL;
            State1 = seed * -0x7CD6391461952C1DL + 0x3943D8696D4A3B7DL;
        }

        public RushRandomness(long state0, long state1)
        {
            State0 = state0;
            State1 = state1;
        }

        public RushRandomness(int seed0, int seed1, int seed2)
        {
            State0 = (seed0 * 0xBFL + seed1 * seed2 << 24) ^ -0x7CD6391461952C1DL;
            State1 = (seed1 * -0x7CD6391461952C1DL ^ seed2 - -0x3943D8696D4A3B7DL) - seed0 * 0x61C8864680B583EBL;
        }


        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 16)
            {
                State0 = (-0x3943D8696D4A3B7DL - snapshot.LongLength * 0x7CD6391461952C1DL);
                State1 = (0x7CD6391461952C1DL + snapshot.LongLength * 0x3943D8696D4A3B7DL);
            }
            else
            {
                State0 = BitConverter.ToInt64(snapshot, 0);
                State1 = BitConverter.ToInt64(snapshot, 8);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[16];
            Buffer.BlockCopy(new long[] { State0, State1 }, 0, snap, 0, 16);
            return snap;
        }

        public Randomness Copy()
        {
            return new RushRandomness(State0, State1);
        }

        public int Next32()
        {
            return (int)(State1 += ((State0 -= 0x61C8864680B583EBL) >> 24) * 0x632AE59B69B3C209L);
        }

        public long Next64()
        {
            return State1 += ((State0 -= 0x61C8864680B583EBL) >> 24) * 0x632AE59B69B3C209L;
        }
    }
    public class HerdRandomness : Randomness
    {
        public uint choice = 0U;
        public uint[] state = new uint[16];

        public HerdRandomness()
        {
            for(int i = 0; i < 16; i++)
            {
                choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
            }
        }
        public HerdRandomness(int seed)
        {
            uint seed2 = (uint)seed, p;
            seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
            for(int i = 0; i < 16; i++)
            {
                p = (seed2 += 0x9E3779B9U);
                p ^= p >> (4 + (int)(p >> 28));
                choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
            }
        }

        public HerdRandomness(int[] seed)
        {
            uint sum = 0U, temp, p;
            if(seed == null)
            {
                temp = 0x13A5BA1DU;
                for(int i = 0; i< 16; i++)
                {
                    p = (temp += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
                }
}
            else
            {
                temp = 0U;
                for(int s = 0; s < seed.Length; s++)
                {
                    sum += (uint)seed[s];
                    temp += ((sum >> 19 | sum << 13) ^ 0x13A5BA1DU);
                    for(int i = 0; i< 16; i++)
                    {
                        p = (temp += 0x9E3779B9U);
                        p ^= p >> (4 + (int)(p >> 28));
                        choice += (state[i] ^= ((p *= 277803737) >> 22) ^ p);
                    }
                }
            }
        }
        public HerdRandomness(uint[] stateSeed, uint choiceSeed)
        {
            if(stateSeed == null || stateSeed.Length != 16)
            {
                for(int i = 0; i < 16; i++)
                {
                    choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
                }
            }
            else
            {
                Buffer.BlockCopy(stateSeed, 0, state, 0, 64);
                choice = choiceSeed;
            }
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 68)
            {
                uint seed2 = (uint)snapshot.Length, p;
                seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
                for(int i = 0; i < 16; i++)
                {
                    p = (seed2 += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    state[i] = ((p *= 277803737) >> 22) ^ p;
                }
                p = (seed2 += 0x8D265FCDU);
                p ^= p >> (4 + (int)(p >> 28));
                choice = ((p *= 277803737) >> 22) ^ p;

            }
            else
            {
                Buffer.BlockCopy(snapshot, 64, state, 0, 4);
                choice = state[0];
                Buffer.BlockCopy(snapshot, 0, state, 0, 64);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[68];
            Buffer.BlockCopy(state, 0, snap, 0, 64);
            Buffer.BlockCopy(new uint[] { choice }, 0, snap, 64, 4);
            return snap;
        }

        public Randomness Copy()
        {
            return new HerdRandomness(state, choice);
        }

        public int Next32()
        {
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
        }

        public long Next64()
        {
            return (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1))
                * 0x632AE59B69B3C209L - choice;
        }
    }
    /// <summary>
    /// Very close to RNG with HerdRandomness hard-coded as its Randomness, but a fair amount faster thanks to less overhead.
    /// </summary>
    /// <remarks>
    /// Uses a different "format" of snapshot that this can process more easily, a uint array instead of a byte array.
    /// A good replacement for System.Random due to drastically higher speed and period, as well as comparable or better quality,
    /// a loadable and settable state via snapshots, and various other useful features, like NextInt() for 32-bit random values.
    /// </remarks>
    public class PRNG : Random
    {
        //public static Random GlobalRandom = new Random();

        public uint choice = 0U;
        public uint[] state = new uint[16];

        /// <summary>
        /// Constructs a randomly-seeded PRNG using 32 calls of System.Random's Next() method on a global Random.
        /// </summary>
        public PRNG()
        {
            for(int i = 0; i < 16; i++)
            {
                choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
            }
        }
        /// <summary>
        /// Constructs a PRNG using just one int for a seed. A PRNG object has 17 ints of total state, so the set of
        /// distinct PRNGs that can be produced by this constuctor is much smaller than the total set of PRNGs possible.
        /// Consider using the constructor that takes an int array, or a uint array and a uint.
        /// </summary>
        /// <param name="seed">any int</param>
        public PRNG(int seed)
        {
            uint seed2 = (uint)seed, p;
            seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
            for(int i = 0; i < 16; i++)
            {
                p = (seed2 += 0x9E3779B9U);
                p ^= p >> (4 + (int)(p >> 28));
                choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
            }
        }

        /// <summary>
        /// Constructs a PRNG while initializing the state (which is a uint array) using the given int array.
        /// Each item in the given int array will be used to iteratively modify each of the 16 state items, with
        /// the choice field starting at 0 but having each step of change to a state item added to it as a sum.
        /// You can give this an array with more or less than 16 elements, or even null, and it will still work.
        /// The order that values appear in the seed affects the resulting PRNG.
        /// </summary>
        /// <param name="seed">an int array, which can be null, empty, or any length</param>
        public PRNG(int[] seed)
        {
            uint sum = 0U, temp, p;
            if(seed == null)
            {
                temp = 0x13A5BA1DU;
                for(int i = 0; i < 16; i++)
                {
                    p = (temp += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
                }
            }
            else
            {
                temp = 0U;
                for(int s = 0; s < seed.Length; s++)
                {
                    sum += (uint)seed[s];
                    temp += ((sum >> 19 | sum << 13) ^ 0x13A5BA1DU);
                    for(int i = 0; i < 16; i++)
                    {
                        p = (temp += 0x9E3779B9U);
                        p ^= p >> (4 + (int)(p >> 28));
                        choice += (state[i] ^= ((p *= 277803737) >> 22) ^ p);
                    }
                }
            }
        }
        /// <summary>
        /// Attempts to reproduce the exact state and choice values given as parameters.
        /// </summary>
        /// <remarks>
        /// If stateSeed is null or is less than 16 elements in length, this will fall back to
        /// the behavior of the zero-argument constructor.
        /// </remarks>
        /// <param name="stateSeed">an array of uint that should be 16 elements long (it can be longer, but extra values won't be used)</param>
        /// <param name="choiceSeed">a uint that will be used in a slightly different way from the rest of the state, and so is separate</param>
        public PRNG(uint[] stateSeed, uint choiceSeed)
        {
            if(stateSeed == null || stateSeed.Length != 16)
            {
                for(int i = 0; i < 16; i++)
                {
                    choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
                }
            }
            else
            {
                Buffer.BlockCopy(stateSeed, 0, state, 0, 64);
                choice = choiceSeed;
            }
        }


        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice);
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 0 or less, this simply returns 0).
        /// </summary>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if(maxValue <= 0) return 0;
            long threshold = (0x7fffffffffffffffL - maxValue + 1) % maxValue;
            for(;;)
            {
                long bits = ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1))
                    * 0x632AE59B69B3C209L - choice) & 0x7fffffffffffffffL;
                if(bits >= threshold)
                    return bits % maxValue;
            }
        }
        /// <summary>
        /// Gets a random long that is between minValue (inclusive) and maxValue (exclusive);
        /// both should be positive and minValue should be less than maxValue.
        /// </summary>
        /// <param name="minValue">the lower bound as a long, inclusive</param>
        /// <param name="maxValue">the upper bound as a long, exclusive</param>
        /// <returns></returns>
        public long NextLong(long minValue, long maxValue)
        {
            return NextLong(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) & 0x7FFFFFFFL)) >> 31);
        }
        /// <summary>
        /// Gets a random int that is between minValue (inclusive) and maxValue (exclusive); both can be positive or negative.
        /// </summary>
        /// <param name="minValue">the inner bound as an int, inclusive</param>
        /// <param name="maxValue">the outer bound as an int, exclusive</param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            return Next(maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Fills buffer with random values, from its start to its end.
        /// </summary>
        /// <remarks>
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
            {
                uint r = (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
                for(int n = Math.Min(buffer.Length - i, 4); n-- > 0; r >>= 8)
                    buffer[i++] = (byte)r;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Gets a random double between -1.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between -1.0 inclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            return BitConverter.Int64BitsToDouble(0x4000000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 3.0;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Returns a new RNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this RNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this RNG</returns>
        public PRNG Copy()
        {
            return new PRNG(state, choice);
        }
        /// <summary>
        /// Gets a snapshot of the current state as a uint array. This snapshot can be used to restore the current state.
        /// </summary>
        /// <returns>a snapshot of the current state as a uint array</returns>
        public uint[] GetSnapshot()
        {
            uint[] snap = new uint[17];
            Array.Copy(state, snap, 16);
            snap[16] = choice;
            return snap;
        }

        /// <summary>
        /// Restores the state this uses internally to the one stored in snapshot, a uint array.
        /// </summary>
        /// <param name="snapshot">a uint array normally produced by GetSnapshot() called on this PRNG</param>
        public void FromSnapshot(uint[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 17)
            {
                uint seed2 = (uint)snapshot.Length, p;
                seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
                for(int i = 0; i < 16; i++)
                {
                    p = (seed2 += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    state[i] = ((p *= 277803737) >> 22) ^ p;
                }
                p = (seed2 += 0x8D265FCDU);
                p ^= p >> (4 + (int)(p >> 28));
                choice = ((p *= 277803737) >> 22) ^ p;

            }
            else
            {
                Array.Copy(snapshot, state, 16);
                choice = snapshot[16];
            }
        }

    }
}
