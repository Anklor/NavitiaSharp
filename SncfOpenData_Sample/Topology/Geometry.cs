using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.App.Topology
{
    public static class Geometry
    {

        public static SqlGeometry LastSegmentTo(SqlGeometry line, SqlGeometry dest)
        {
            SqlGeometry segment = null;
            if (line.STStartPoint().STEquals(dest))
            {
                segment = Geometry.LineSegment(line.STPointN(2), line.STPointN(1));
            }
            else if (line.STEndPoint().STEquals(dest))
            {
                int numPoints = line.STNumPoints().Value;
                segment = Geometry.LineSegment(line.STPointN(numPoints - 1), line.STPointN(numPoints));
            }
            else
            {
                segment = null;
            }

            return segment;
        }

        public static SqlGeometry FirstSegmentFrom(SqlGeometry line, SqlGeometry origin)
        {
            SqlGeometry segment = null;

            if (line.STStartPoint().STEquals(origin))
            {
                segment = Geometry.LineSegment(line.STPointN(1), line.STPointN(2));
            }
            else if (line.STEndPoint().STEquals(origin))
            {
                int numPoints = line.STNumPoints().Value;
                segment = Geometry.LineSegment(line.STPointN(numPoints), line.STPointN(numPoints - 1));
            }
            else

            {
                segment = null;
            }

            return segment;
        }

        public static SqlGeometry LineSegment(SqlGeometry from, SqlGeometry to)
        {
            SqlGeometryBuilder gb = new SqlGeometryBuilder();
            gb.SetSrid(from.STSrid.Value);
            gb.BeginGeometry(OpenGisGeometryType.LineString);
            gb.BeginFigure(from.STX.Value, from.STY.Value);
            gb.AddLine(to.STX.Value, to.STY.Value);
            gb.EndFigure();
            gb.EndGeometry();
            return gb.ConstructedGeometry;
        }

        public static SqlGeometry Union(float buffer, params SqlGeometry[] geoms)
        {
            int srid = geoms.First().STSrid.Value;
            var union = SqlTypesExtensions.PointEmpty_SqlGeometry(srid);
            foreach (var g in geoms)
            {
                union = union.STUnion(buffer == 0 ? g : g.STBuffer(buffer));
            }
            return union;
        }

        public static double AngleBetweenLines(SqlGeometry line1, SqlGeometry line2)
        {
            SqlGeometry connectionPoint = line1.STIntersection(line2);

            SqlGeometry segment1 = FirstSegmentFrom(line1, connectionPoint);
            SqlGeometry segment2 = FirstSegmentFrom(line2, connectionPoint);

            return Angle.AngleBetweenSegments(segment1, segment2);

        }

        public static Vector2 ToVector(this SqlGeometry geom)
        {
            return new Vector2((float)(geom.STEndPoint().STX.Value - geom.STStartPoint().STX.Value), (float)(geom.STEndPoint().STY.Value - geom.STStartPoint().STY.Value));
        }

        /// <summary>
        /// http://stackoverflow.com/questions/10096930/how-do-i-know-if-two-line-segments-are-near-collinear
        /// </summary>
        /// <param name="linea"></param>
        /// <param name="lineb"></param>
        /// <param name="intersectionPoint"></param>
        /// <param name="threshold"></param>
        /// <returns>1 equals segments are colinear</returns>
        public static float ColinearIndice(SqlGeometry linea, SqlGeometry lineb, SqlGeometry intersectionPoint)
        {
            var a = Geometry.FirstSegmentFrom(linea, intersectionPoint);
            var b = Geometry.FirstSegmentFrom(lineb, intersectionPoint);
            Vector2 av = a.ToVector();
            Vector2 bv = b.ToVector();
            float aMag = av.Length();
            float bMag = bv.Length();

            Vector2 aCosVec = new Vector2(av.X / aMag, av.Y / aMag);
            Vector2 bCosVec = new Vector2(bv.X / bMag, bv.Y / bMag);
            float cosAngle = Vector2.Dot(aCosVec, bCosVec);
            return Math.Abs(cosAngle);

        }

        public static bool AreColinear(SqlGeometry linea, SqlGeometry lineb, SqlGeometry intersectionPoint, float threshold = 0.05f)
        {
            var indice = ColinearIndice(linea, lineb, intersectionPoint);
            float delta = Math.Abs((1 - threshold) - indice);
            return delta < threshold;

        }
    }
}