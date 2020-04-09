using System.Collections.Generic;
using WW.Cad.Drawing;
using WW.Cad.Model.Entities;
using WW.Drawing;
using WW.Math;
using WW.Math.Geometry;

namespace CAD2CSV
{
    class LengthComputerGraphicsFactory : BaseWireframeGraphicsFactory
    {
        private double m_TotalLength = 0.0;

        public double GetLength()
        {
            return m_TotalLength;
        }

        public override void CreateLine(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            Vector4D start,
            Vector4D end
        )
        {
            Point3D point1 = (Point3D)start;
            Point3D point2 = (Point3D)end;
            m_TotalLength += Point3D.Subtract(point2, point1).GetLength();
        }


        public override void CreatePath(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IList<Polyline4D> polylines,
            bool fill,
            bool correctForBackgroundColor
        )
        {
            m_TotalLength += PolyLineLength(polylines);
        }

        public override void CreatePathAsOne(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IList<Polyline4D> polylines,
            bool fill,
            bool correctForBackgroundColor
        )
        {
            m_TotalLength += PolyLineLength(polylines);
        }

        public override void CreateShape(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IShape4D shape
        )
        {
            m_TotalLength += PolyLineLength(shape.ToPolylines4D(ShapeTool.DefaultEpsilon));
        }

        private double PolyLineLength(IList<Polyline4D> polylines)
        {
            double polylinestotallength = 0.0;
            foreach (Polyline4D polyline in polylines)
            {
                polylinestotallength += polyline.GetLength();
            }
            return polylinestotallength;
        }
    }
}
