using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.Topology
{
    public static class Angle
    {
        ///<summary>
        /// Returns the angle of the vector from p0 to p1,
        /// relative to the positive X-axis.
        /// The angle is normalized to be in the range [ -Pi, Pi ].
        ///</summary>
        ///<param name="p0"></param>
        ///<param name="p1"></param>
        ///<returns>the normalized angle (in radians) that p0-p1 makes with the positive x-axis.</returns>
        public static double CalculateAngle(Microsoft.SqlServer.Types.SqlGeometry p0, Microsoft.SqlServer.Types.SqlGeometry p1)
        {
            Double dx = p1.STX.Value - p0.STX.Value;
            Double dy = p1.STY.Value - p0.STY.Value;
            return Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Return angle between two segments
        /// see http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/
        /// </summary>
        /// <param name="seg1"></param>
        /// <param name="seg2"></param>
        /// <returns></returns>
        public static double AngleBetweenSegments(SqlGeometry seg1, SqlGeometry seg2)
        {
            if (!IsSegment(seg1))
                throw new ArgumentException("seg1 must be a segment!");

            if (!IsSegment(seg2))
                throw new ArgumentException("seg2 must be a segment!");


            //angle = acos(A•B) where • is Ax * Bx + Ay * By
            double x1 = seg1.STEndPoint().STX.Value - seg1.STStartPoint().STX.Value;
            double x2 = seg2.STEndPoint().STX.Value - seg2.STStartPoint().STX.Value;
           
            double y1 = seg1.STEndPoint().STY.Value - seg1.STStartPoint().STY.Value;
            double y2 = seg2.STEndPoint().STY.Value - seg2.STStartPoint().STY.Value;
            double[] normalized1 = Normalize(new double[] { x1, y1 });
            double[] normalized2 = Normalize(new double[] { x2, y2 });

            double angle = Math.Acos(normalized1[0] * normalized2[0] + normalized1[1] * normalized2[1]);
            return angle;
        }

        public static double[] Normalize(double[] coords)
        {
            double distance = Math.Sqrt(coords[0] * coords[0] + coords[1] * coords[1]);
            return new double[] { coords[0] / distance, coords[1] / distance };
        }

        public static bool IsSegment(SqlGeometry segment)
        {
            return segment != null
                 && segment.STGeometryType().Value.ToUpper() == "LINESTRING"
                 && segment.STNumPoints().Value == 2;
        }
        ///<summary>
        /// Converts from radians to degrees.
        ///</summary>
        ///<param name="radians">an angle in radians</param>
        ///<returns>the angle in degrees</returns>
        public static double ToDegrees(double radians)
        {
            return (radians * 180) / (Math.PI);
        }

        ///<summary>
        /// Converts from degrees to radians.
        ///</summary>
        ///<param name="angleDegrees">an angle in degrees</param>
        ///<returns>the angle in radians</returns>
        public static Double ToRadians(Double angleDegrees)
        {
            return (angleDegrees * Math.PI) / 180.0;
        }
    }
}
