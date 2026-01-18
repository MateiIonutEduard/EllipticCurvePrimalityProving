using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elliptic_Curve_Primality_Proving.Core
{
    internal enum AtkinCertState : int
    {
        None = 0,
        SearchDiscriminant = 1,
        FindEllipticCurveOrder = 2,
        ComputeParameters = 3,
        FindGenerator = 4,
        GenCurvePoint = 5,
        PointInvalidOrder = 6,
        GenTwistedCurve = 7,
        SmallOrderPoint = 8,
        AppendCertificate = 9,
        GoNextCandidate = 10
    }
}
