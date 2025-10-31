using System;
using Eduard;
using Eduard.Cryptography;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using ECPoint = Eduard.Cryptography.ECPoint;

namespace Elliptic_Curve_Primality_Proving
{
    class Atkin
    {
        private RandomNumberGenerator rand;

        public Atkin()
        {
            rand = RandomNumberGenerator.Create();
        }

        private int w(long D)
        {
            if (D < -4)
                return 2;
            else if (D == -4)
                return 4;
            return 6;
        }

        public void VerifyCert(StringBuilder sb, BackgroundWorker bw, Certificate cert)
        {
            BigInteger[] args = null;
            ECPoint point = ECPoint.POINT_INFINITY;

            ECPoint P = ECPoint.POINT_INFINITY;
            EllipticCurve curve = null;

            while((args = cert.Read()) != null)
            {
                if (bw.CancellationPending) return;
                curve = new EllipticCurve(args[0], args[1], args[2], args[3], args[3] / args[4]);

                sb.AppendFormat("N = {0}\n", args[2].ToString());
                sb.AppendFormat("a = {0}", args[0].ToString());

                sb.AppendLine();
                sb.AppendFormat("b = {0}", args[1].ToString());

                sb.AppendLine();
                sb.AppendFormat("m = {0}", args[3].ToString());

                sb.AppendLine();
                sb.AppendFormat("q = {0}", args[4].ToString());

                sb.AppendLine();
                point = new ECPoint(args[5], args[6]);

                sb.AppendFormat("P = ({0}, {1})", point.GetAffineX().ToString(), point.GetAffineY().ToString());
                sb.AppendLine();

                P = ECMath.Multiply(curve, args[3] / args[4], point, ECMode.EC_STANDARD_PROJECTIVE);
                sb.AppendFormat("Q = ({0}, {1})", P.GetAffineX().ToString(), P.GetAffineY().ToString());

                sb.AppendLine();
                P = ECMath.Multiply(curve, args[4], P, ECMode.EC_FASTEST);

                sb.Append("R = (0, 1)\n");
                sb.AppendLine();
            }
        }

        public int Start(BigInteger field, StringBuilder sb, BackgroundWorker bw, Certificate cert)
        {
            long D = 0;
            int ID = 1;
            int k = 0;

            Random rng = new Random(Environment.TickCount);
            BigInteger val = field;

            ECPoint point = null;
            EllipticCurve curve = null;
            ECPoint P = null, Q = null;

            Hilbert hp = new Hilbert();
            BigInteger u = 0, v = 0;

            BigInteger order = 0;
            BigInteger factor = 0;

            BigInteger a = 0, b = 0;
            Hilbert.SetModulus(val);
            BigInteger g = 1;

            switch(ID)
            {
                case 1:
                    if (bw.CancellationPending) return -1;
                    D = hp.NextDiscriminant();
                    if (D == -1) return -2;

                    if (BigInteger.Jacobi(D, val) == -1)
                        goto case 1;
                    
                    if (!ModifiedCornacchia(val, ref u, ref v, D))
                        goto case 1;

                goto case 2;
                case 2:
                    if (bw.CancellationPending) return -1;
                    if (D == -3)
                    {
                        order = val + 1 + (u + 3 * v);
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;

                        order = val + 1 - (u + 3 * v);
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;

                        order = val + 1 + (u - 3 * v);
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;

                        order = val + 1 - (u - 3 * v);
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;
                    }
                    else
                    if(D == -4)
                    {
                        order = val + 1 + 2 * v;
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;

                        order = val + 1 - 2 * v;
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;
                    }
                    else
                    {
                        order = val + 1 + u;
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;

                        order = val + 1 - u;
                        factor = FindFactor(order);

                        if (factor != -1)
                            goto case 3;
                    }
                    goto case 1;
                case 3:
                    if (bw.CancellationPending) return -1;
                    if (D == -3)
                    {
                        a = 0;
                        b = val - 1;
                    }
                    else
                    if(D == -4)
                    {
                        a = val - 1;
                        b = 0;
                    }
                    else
                    {
                        Polynomial poly = hp.GetHilbertPolynomial(D);
                        List<BigInteger> roots = new List<BigInteger>();
                        if (poly.Degree > 1) poly.FindRoots(ref roots);
                        else roots.Add(val - poly.coeffs[0]);

                        BigInteger J = roots[rng.Next(roots.Count)];
                        a = J - 1728;

                        BigInteger inv = a.Inverse(val);
                        BigInteger c = (J * inv) % val;

                        if (c == 0) goto case 1;
                        a = (-3 * c) % val;

                        if (a < 0) a += val;
                        b = (2 * c) % val;
                    }
                    goto case 4;
                case 4:
                    if (bw.CancellationPending) return -1;
                    g = BigInteger.Next(rand, 1, val - 1);
                    if (D == -3)
                    {
                        if (BigInteger.Pow(g, (val - 1) / 3, val) == 1)
                            goto case 4;
                    }
                    else
                    {
                        if (BigInteger.Jacobi(g, val) != -1)
                            goto case 4;
                    }

                    k = 0;
                    goto case 5;
                case 5:
                    if (bw.CancellationPending) return -1;
                    curve = new EllipticCurve(a, b, val, factor, order / factor);
                    point = curve.BasePoint;
                    goto case 6;
                case 6:
                    P = ECMath.Multiply(curve, order / factor, point, ECMode.EC_STANDARD_PROJECTIVE);
                    Q = ECMath.Multiply(curve, factor, P, ECMode.EC_FASTEST);

                    if (Q == ECPoint.POINT_INFINITY)
                        goto case 9;

                    goto case 7;
                case 7:
                    if (bw.CancellationPending) return -1;
                    k++;
                    if (k >= w(D))
                        goto case 9;

                    if (D == -3)
                        b = (b * g) % val;
                    else
                        if (D == -4)
                        a = (a * g) % val;
                    else
                    {
                        BigInteger g2 = (g * g) % val;
                        a = (a * g2) % val;

                        g2 = (g2 * g) % val;
                        b = (b * g2) % val;
                    }
                    goto case 5;
                case 8:
                    if (bw.CancellationPending) return -1;
                    point = curve.BasePoint;
                    P = ECMath.Multiply(curve, order / factor, point, ECMode.EC_STANDARD_PROJECTIVE);

                    if (P == ECPoint.POINT_INFINITY)
                        goto case 8;

                    Q = ECMath.Multiply(curve, factor, P, ECMode.EC_FASTEST);

                    if (Q != ECPoint.POINT_INFINITY)
                        goto case 7;

                    goto case 9;
                case 9:
                    if (bw.CancellationPending) return -1;
                    if (P == ECPoint.POINT_INFINITY) goto case 8;
                    sb.AppendFormat("N = {0}", val.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("a = {0}", a.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("b = {0}", b.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("m = {0}", order.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("q = {0}", factor.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("P = ({0}, {1})", point.GetAffineX().ToString(), point.GetAffineY().ToString());
                    sb.AppendLine();
                    sb.AppendFormat("Q = ({0}, {1})", P.GetAffineX().ToString(), P.GetAffineY().ToString());
                    sb.AppendLine();
                    sb.Append("R = (0, 1)\n");
                    sb.AppendLine();
                    cert.Write(a, b, val, order, factor, point.GetAffineX(), point.GetAffineY());
                    goto case 10;
                case 10:
                    if (bw.CancellationPending) return -1;
                    val = factor;
                    Hilbert.SetModulus(val);
                    hp.Reset();
                    
                    if (val.GetBits() <= 64)
                        return 1;

                    goto case 1;
            }

            return 0;
        }

        private BigInteger FindFactor(BigInteger val)
        {
            Sieve sieve = new Sieve(2000);
            BigInteger factor = val;

            for(int i = 0; i < sieve.Count; i++)
            {
                while(factor > 1 && factor % sieve[i] == 0)
                    factor /= sieve[i];
            }

            BigInteger root = val.Sqrt();
            root = root.Sqrt();

            root++;
            root = root * root;

            if (factor > root && BigInteger.IsProbablePrime(rand, factor, 50))
                return factor;

            return -1;
        }

        private bool ModifiedCornacchia(BigInteger field, ref BigInteger u, ref BigInteger v, long D)
        {
            BigInteger root = 0;

            if (BigInteger.Jacobi(field + D, field) == 1)
            {
                root = Sqrt(field + D, field);

                if ((root & 1) != (D & 1))
                    root = field - root;
            }
            else return false;
            
            BigInteger a = field << 1;
            BigInteger b = root;

            BigInteger t = 0;
            BigInteger c = field.Sqrt();
            c <<= 1;

            while(b > c)
            {
                t = a % b;
                a = b;
                b = t;
            }

            a = b * b;
            c = field << 2;
            t = c - a;

            a = Math.Abs(D);
            c = t / a;

            a = t % a;
            root = c.Sqrt();
            
            if (a == 0 && (root * root) == c)
            {
                u = b;
                v = c;
                return true;
            }
            
            return false;
        }

        private BigInteger Sqrt(BigInteger val, BigInteger field)
        {
            if ((field & 3) == 3)
                return BigInteger.Pow(val, (field + 1) >> 2, field);

            BigInteger root = 0;
            BigInteger delta = ((field - 4) * (field - val)) % field;
            BigInteger temp = 1;
            BigInteger qnr = 0;
            BigInteger buf = 0;
            int ID = 1;

            switch (ID)
            {
                case 1:

                    root = TonelliShanks(val, field);

                    if (val == (root * root) % field)
                        return root;
                    goto case 2;

                case 2:

                    qnr = BigInteger.Next(rand, 2, field - 1);

                    if (BigInteger.Jacobi(qnr, field) != -1)
                        goto case 2;

                    BigInteger square = (qnr * qnr) % field;
                    delta = (delta * square) % field;
                    temp = (temp * qnr) % field;
                    buf = TonelliShanks(delta, field);

                    if (delta != (buf * buf) % field)
                        goto case 2;
                    goto case 3;

                case 3:

                    BigInteger vtemp = (2 * temp) % field;
                    BigInteger inv = vtemp.Inverse(field);

                    root = (buf * inv) % field;
                    break;
            }

            return root;
        }

        private BigInteger TonelliShanks(BigInteger val, BigInteger field)
        {
            long e = 0, r, s;
            BigInteger b = 0, bp = 0, q = field - 1, m = 0, n = 0;
            BigInteger p1 = field - 1, t = 0, x = 0, y = 0, z = 0;

            while ((q & 1) == 0)
            {
                q >>= 1;
                e++;
            }

            int JSymbol = 0;

            do
            {
                n = BigInteger.Next(rand, 1, field - 1);
                JSymbol = BigInteger.Jacobi(n, field);
            } while (JSymbol == -1);

            z = BigInteger.Pow(n, q, field);
            y = z;
            r = e;
            x = BigInteger.Pow(val, (q - 1) >> 1, field);
            b = (((val * x) % field) * x) % field;
            x = (val * x) % field;

            while (true)
            {
                if (b == 1 || b == p1)
                    return x;

                s = 1;

                do
                {
                    bp = BigInteger.Pow(b, (long)Math.Pow(2, s), field);
                    s++;
                } while (bp != 1 && bp != p1 && s < r);

                if (s == r)
                    return 0;

                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), field);
                y = (t * t) % field;
                x = (x * t) % field;
                b = (b * y) % field;
                r = s;
            }
        }
    }
}
