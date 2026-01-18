using System;
using System.Collections.Generic;
using System.IO;
using Eduard;
using Eduard.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Elliptic_Curve_Primality_Proving.Core
{
    class Hilbert
    {
        private static readonly JObject jsonData;
        private static readonly List<long> discriminants;
        private static BigInteger field;

        private int _index;

        /// <summary>
        /// Static constructor for one-time initialization.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        static Hilbert()
        {
            try
            {
                var jsonText = File.ReadAllText(@"data\disc.json");
                jsonData = JObject.Parse(jsonText);

                /* pre-initialize discriminants list */
                discriminants = new List<long> { -3, -4 };

                if (jsonData["disc"] is JArray discArray)
                {
                    foreach (var item in discArray)
                    {
                        if (long.TryParse(item.ToString(), out long discriminant))
                            discriminants.Add(discriminant);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Hilbert class: "
                    + ex.Message, ex);
            }
        }

        public Hilbert()
        {
            _index = 0;
        }

        public void Reset() => _index = 0;

        public static void SetModulus(BigInteger modulus)
        {
            field = modulus;
            Polynomial.SetField(modulus);
        }

        public long NextDiscriminant()
        {
            return _index < discriminants.Count ? discriminants[_index++] : -1;
        }

        public Polynomial GetHilbertPolynomial(long D)
        {
            string key = D.ToString();

            if (jsonData[key] is JArray coefficientsArray)
            {
                /* pre-allocate array with known size for better performance */
                var coeffs = new BigInteger[coefficientsArray.Count];

                for (int i = 0; i < coefficientsArray.Count; i++)
                    coeffs[i] = new BigInteger(coefficientsArray[i].ToString());

                /* reverse in-place instead of creating new collections */
                Array.Reverse(coeffs);
                return new Polynomial(coeffs);
            }

            return null;
        }
    }
}