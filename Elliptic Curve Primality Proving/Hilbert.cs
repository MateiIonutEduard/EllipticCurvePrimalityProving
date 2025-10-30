using System;
using Eduard;
using System.IO;
using Eduard.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elliptic_Curve_Primality_Proving
{
    class Hilbert
    {
        private JObject obj;
        private static BigInteger field;
        private List<long> list;
        private int index;

        public Hilbert()
        {
            list = new List<long>();
            var temp = File.ReadAllText(@"data\disc.json");
            obj = JObject.Parse(temp);

            try
            {
                JArray items = (JArray)obj["disc"];
                list.Add(-3);
                list.Add(-4);

                foreach (var item in items)
                    list.Add(long.Parse(item.ToString()));

                index = 0;
            }
            catch(Exception)
            { throw; }
        }

        public void Reset()
        { index = 0; }

        public static void SetModulus(BigInteger modulus)
        {
            field = modulus;
            Polynomial.SetField(modulus);
        }

        public long NextDiscriminant()
        {
            if (index < list.Count - 1)
                return list[index++];
            return -1;
        }

        public Polynomial GetHilbertPolynomial(long D)
        {
            try
            {
                JArray array = (JArray)obj[D.ToString()];
                List<BigInteger> coeffs = new List<BigInteger>();

                foreach (var item in array)
                    coeffs.Add(new BigInteger(item.ToString()));

                coeffs.Reverse();
                Polynomial result = new Polynomial(coeffs.ToArray());
                return result;
            }
            catch (Exception)
            { return null; }
        }
    }
}
