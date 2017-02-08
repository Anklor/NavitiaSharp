using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.App.Topology
{
    public static class Geometry
    {
        public static SqlGeometry LastSegment(SqlGeometry line, SqlGeometry origin)
        {
            SqlGeometry segment = null;
            if (line.STStartPoint().STEquals(origin))
            {
                int numPoints = line.STNumPoints().Value;
                segment = Geometry.LineSegment(line.STPointN(numPoints - 1), line.STPointN(numPoints));
            }
            else if (line.STEndPoint().STEquals(origin))
            {
                segment = Geometry.LineSegment(line.STPointN(1), line.STPointN(2));
            }
            else
            {
                segment = null;
            }

            return segment;
        }
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
            foreach(var g in geoms)
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
    }
}