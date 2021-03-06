﻿using System;


// To emphasise the types' sizes, this code uses Int32 and UInt32 rather than the usual equivalents int and uint.


namespace IntegerAbsoluteDifferenceCSharp
{
    sealed class Program
    {

#pragma warning disable IDE0060 // Remove unused parameter
        private static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            RunTests();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }


        // NO NO NO NO NO. BAD WRONG DANGEROUS TERRIBLE. May throw.
        // May also return incorrect value due to overflow.
        public static UInt32 INCORRECT_difference_int32(Int32 i, Int32 j)
        {
            unchecked
            {
                return (UInt32)Math.Abs(i - j);
                // If the subtraction overflows/underflows we produce an incorrect value.
                // The call to abs() is also unsafe: Math.Abs(Int32.MinValue) will throw.
            }
        }


        // The easy way using Int64. Of course, this approach can't be used
        // if the input integer type is the largest available integer type.
        public static UInt32 Easy_difference_int32(Int32 i, Int32 j)
        {
            unchecked
            {
                return (UInt32)Math.Abs((Int64)i - (Int64)j);
            }
        }


        // The low-level way.
        public static UInt32 LowLevel_difference_int32(Int32 i, Int32 j)
        {
            unchecked
            {
                // Map the signed values across to the number-line of UInt32.
                // Preserves the greater-than relation, such that an input of Int32.MinValue
                // is mapped to 0, and an input of 0 is mapped to near the middle
                // of the uint32_t number-line.
                // Leverages the wrap-around behaviour of unsigned integer types.

                // It would be more intuitive to set the offset to (UInt32)(-1 * Int32.MinValue)
                // but that multiplication overflows the signed integer type.
                // We get the right effect subtracting from zero.
                const UInt32 offset = 0u - (UInt32)(Int32.MinValue); // 2's complement / C-style conversion
                UInt32 i_u = (UInt32)i + offset;
                UInt32 j_u = (UInt32)j + offset;

#if true
                // Readable version:
                UInt32 ret = (i_u > j_u) ? (i_u - j_u) : (j_u - i_u);
#else
                // Unreadable branch-free version adapted from
                // https://graphics.stanford.edu/~seander/bithacks.html#IntegerMinOrMax
                // Modern code-generators are likely to generate *worse* code from this variation.

                //UInt32 t = (i_u ^ j_u) & -(i_u < j_u); // C code, invalid in C#
                UInt32 t = (i_u ^ j_u) & (0u - System.Convert.ToUInt32(i_u < j_u));
                UInt32 min = j_u ^ t; // min(i_u, j_u)
                UInt32 max = i_u ^ t; // max(i_u, j_u)
                UInt32 ret = max - min;
#endif
                return ret;
            }
        }


        // The laborious way
        public static UInt32 Laborious_difference_int32(Int32 i, Int32 j)
        {
            unchecked
            {
                UInt32 ret;
                if (i == j)
                {
                    ret = 0;
                }
                else
                {
                    if (j > i)
                    { // Swap them so that i > j
                        Int32 i_orig = i;
                        i = j;
                        j = i_orig;
                    } // We may now safely assume i > j

                    UInt32 magn_of_greater; // The magnitude
                    bool greater_is_negative; // Zero is of course non-negative
                    UInt32 magn_of_lesser;
                    bool lesser_is_negative;

                    if (i >= 0)
                    {
                        magn_of_greater = (UInt32)i;
                        greater_is_negative = false;
                    }
                    else
                    { // Here it follows that 'lesser' is also negative, but we'll keep the flow simple
                      // magn_of_greater = -i; // DANGEROUS, overflows if i == Int32.MinValue.
                        magn_of_greater = (UInt32)0 - (UInt32)i;
                        greater_is_negative = true;
                    }

                    if (j >= 0)
                    {
                        magn_of_lesser = (UInt32)j;
                        lesser_is_negative = false;
                    }
                    else
                    {
                        // magn_of_lesser = -j; // DANGEROUS, overflows if i == Int32.MinValue.
                        magn_of_lesser = (UInt32)((Int32)0 - (Int32)j);
                        lesser_is_negative = true;
                    }

                    // Finally compute the difference between lesser and greater
                    if (!greater_is_negative && !lesser_is_negative)
                    {
                        ret = magn_of_greater - magn_of_lesser;
                    }
                    else if (greater_is_negative && lesser_is_negative)
                    {
                        ret = magn_of_lesser - magn_of_greater;
                    }
                    else
                    { // One negative, one non-negative. Difference between them is sum of the magnitudes.
                      // This will never overflow.
                        ret = magn_of_lesser + magn_of_greater;
                    }
                }
                return ret;
            }
        }


        public static UInt32 Difference_int32(Int32 i, Int32 j) {
            var v1 = Laborious_difference_int32(i, j);
            var v2 = Easy_difference_int32(i, j);
            var v3 = LowLevel_difference_int32(i, j);

            bool ok = (v1 == v2) && (v2 == v3);
            if (!ok) {
                Console.WriteLine("CHECK FAILED. The 3 implementations did not agree.");
            }
            //else {
            //    Console.WriteLine( "Check passed." + "\n");
            //}
            return v1;
        }


        public static int RunTests()
        {
            Console.WriteLine( "Starting..." + "\n");

            var d1 = Difference_int32(0, 0);
            bool check1 = (d1 == 0);
            Console.WriteLine( "Check 1: " + check1 + "\n");

            var d2 = Difference_int32(0, Int32.MaxValue);
            bool check2 = (d2 == Int32.MaxValue);
            Console.WriteLine( "Check 2: " + check2 + "\n");

            // This one causes overflow in naive implementation
            var d3 = Difference_int32(Int32.MinValue, Int32.MaxValue);
            bool check3_1 = ((Int64)d3 == (Int64)Int32.MaxValue - (Int64)Int32.MinValue);
            bool check3_2 = (d3 == UInt32.MaxValue);
            Console.WriteLine( "Check 3_1: " + check3_1 + "\n");
            Console.WriteLine( "Check 3_2: " + check3_2 + "\n");

            var d4 = Difference_int32(-14, 20);
            bool check4 = (d4 == 34);
            Console.WriteLine( "Check 4: " + check4 + "\n");


            var d5 = Difference_int32(Int32.MinValue, Int32.MinValue + 1);
            bool check5 = (d5 == 1);
            Console.WriteLine( "Check 5: " + check5 + "\n");

            var d6 = Difference_int32(-400, -300);
            bool check6 = (d6 == 100);
            Console.WriteLine( "Check 6: " + check6 + "\n");

            var d7 = Difference_int32(-300, -400);
            bool check7 = (d7 == 100);
            Console.WriteLine( "Check 7: " + check7 + "\n");

            var d8 = Difference_int32(400, 300);
            bool check8 = (d8 == 100);
            Console.WriteLine( "Check 8: " + check8 + "\n");

            var d9 = Difference_int32(300, 400);
            bool check9 = (d9 == 100);
            Console.WriteLine( "Check 9: " + check9 + "\n");

            var d10 = Difference_int32(20, -14); // 4 but commuted
            bool check10 = (d10 == 34);
            Console.WriteLine( "Check 10: " + check10 + "\n");

            var d11 = Difference_int32(Int32.MinValue + 1, Int32.MinValue); // 5 but commuted
            bool check11 = (d11 == 1);
            Console.WriteLine( "Check 11: " + check11 + "\n");

            var d12 = Difference_int32(21, 1021);
            bool check12 = (d12 == 1000);
            Console.WriteLine( "Check 12: " + check12 + "\n");

            var d13 = Difference_int32(1021, 21);
            bool check13 = (d13 == 1000);
            Console.WriteLine( "Check 13: " + check13 + "\n");

            // This one causes overflow in naive implementation
            var d14 = Difference_int32(Int32.MaxValue, Int32.MinValue); // 3 but commuted
            bool check14_1 = ((Int64)d14 == (Int64)Int32.MaxValue - (Int64)Int32.MinValue);
            bool check14_2 = (d14 == UInt32.MaxValue);
            Console.WriteLine( "Check 14_1: " + check14_1 + "\n");
            Console.WriteLine( "Check 14_2: " + check14_2 + "\n");

            var d15 = Difference_int32(Int32.MaxValue, Int32.MaxValue);
            bool check15 = (d15 == 0);
            Console.WriteLine( "Check 15: " + check15 + "\n");

            var d16 = Difference_int32(Int32.MinValue, Int32.MinValue);
            bool check16 = (d16 == 0);
            Console.WriteLine( "Check 16: " + check16 + "\n");

            var d17 = Difference_int32(Int32.MaxValue, 0); // 2 but commuted
            bool check17 = (d17 == Int32.MaxValue);
            Console.WriteLine( "Check 17: " + check17 + "\n");

            var d18 = Difference_int32(12345, 12345);
            bool check18 = (d18 == 0);
            Console.WriteLine( "Check 18: " + check18 + "\n");

            var d19 = Difference_int32(-45678, -45678);
            bool check19 = (d19 == 0);
            Console.WriteLine( "Check 19: " + check19 + "\n");

            var d20 = Difference_int32(Int32.MaxValue - 234, Int32.MaxValue);
            bool check20 = (d20 == 234);
            Console.WriteLine( "Check 20: " + check20 + "\n");

            var d21 = Difference_int32(Int32.MaxValue, Int32.MaxValue - 234);
            bool check21 = (d21 == 234);
            Console.WriteLine( "Check 21: " + check21 + "\n");

            var d22 = Difference_int32(Int32.MinValue + 234, Int32.MinValue);
            bool check22 = (d22 == 234);
            Console.WriteLine( "Check 22: " + check22 + "\n");

            var d23 = Difference_int32(Int32.MinValue, Int32.MinValue + 234);
            bool check23 = (d23 == 234);
            Console.WriteLine( "Check 23: " + check23 + "\n");

            // This one causes overflow in naive implementation
            Int64 abs_Int32_MinValue_as_int64 = -1 * (Int64)(Int32.MinValue);
            var d24 = Difference_int32(Int32.MinValue, 0);
            bool check24 = ((Int64)d24 == abs_Int32_MinValue_as_int64);
            Console.WriteLine( "Check 24: " + check24 + "\n");

            // This one causes overflow in naive implementation
            var d25 = Difference_int32(0, Int32.MinValue);
            bool check25 = ((Int64)d25 == abs_Int32_MinValue_as_int64);
            Console.WriteLine( "Check 25: " + check25 + "\n");


            return 0;
        }

    }
}
