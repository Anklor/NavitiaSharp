using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.App.Topology
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

        //  cos(a) = dx²/(dx²+dy²)
        public static double CalculateHeading(Microsoft.SqlServer.Types.SqlGeometry p0, Microsoft.SqlServer.Types.SqlGeometry p1)
        {
            double dx = p0.STX.Value - p1.STX.Value;
            double dy = p0.STY.Value - p1.STY.Value;
            if (dx == 0 || dy == 0)
                return 0;

            double cos = (dx * dx) / (dx * dx + dy * dy);
            var angle = Math.Acos(cos);
            return angle;
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
