using Eduard;
using Eduard.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ECPoint = Eduard.Cryptography.ECPoint;

namespace Elliptic_Curve_Primality_Proving
{
    class AtkinMorainUtil
    {
        static RandomNumberGenerator rand;

        static AtkinMorainUtil()
        {
            rand = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Reimplements base point generation on the elliptic curve to optimize performance
        /// for this application.<br/> This version differs from the implementation in the Eduard framework.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static ECPoint GetBasePoint(EllipticCurve curve)
        {
            bool done = false;
            BigInteger x = 0;

            BigInteger y = 0, temp = 0, p = curve.field;
            ECPoint basePoint = ECPoint.POINT_INFINITY;

            do
            {
                x = BigInteger.Next(rand, 0, p - 1);
                temp = curve.Evaluate(x);

                if (temp < 2)
                    return new ECPoint(x, temp);

                if (BigInteger.Jacobi(temp, p) == 1)
                {
                    done = true;
                    y = curve.Sqrt(temp);

                    BigInteger eval = (y * y) % p;
                    if (temp != eval) done = false;

                    /* a valid point on the Weierstrass curve has been found */
                    if (done) basePoint = new ECPoint(x, y);
                }
            }
            while (!done);

            return basePoint;
        }
    }
}
