using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

namespace RoundaboutBuilder.Tools
{
    public class Ellipse
    {
        /* Y-axis squished to zero */
        /* The ellipse will be drawn using #DENSITY segments. Four should be enough.
         * From ellipse geometry should be multiple of four, so that the points hit the four main control points. */
        public static readonly int DENSITY = 8;

        public float RadiusMain { get; private set; }
        public float RadiusMinor { get; private set; }

        public Vector3 Center { get; private set; }
        public Vector3 Focal1 { get; private set; }
        public Vector3 Focal2 { get; private set; }

        public List<Bezier2> Beziers { get; private set; }

        public double Ratio { get; private set; }
        public double EllipseRotation { get; private set; }

        private Vector3 mainAxisDirection;


        public Ellipse(Vector3 center, Vector3 mainAxisDirection, float radiusMain, float radiusMinor)
        {
            Center = center;
            RadiusMain = radiusMain;
            RadiusMinor = radiusMinor;
            this.mainAxisDirection = mainAxisDirection;
            Ratio = ((double)RadiusMinor) / ((double)RadiusMain);
            EllipseRotation = VectorsAngle(mainAxisDirection);
            CalculateFocals();
            calculateBeziers();
        }

        public bool IsInsideEllipse(Vector3 vector)
        {
                if (VectorDistance(vector, Focal1) + VectorDistance(vector, Focal2) > 2 * RadiusMain ) return false;
                else return true;
        }

        public bool IsCircle()
        {
            return RadiusMain - RadiusMinor < 0.0001f;
        }

        /* Well, the vector density is uniform on the circle, but not on the ellipse. But whatever... */
        private void calculateBeziers()
        {
            if (DENSITY < 4) Beziers = null;
            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> tangents = new List<Vector3>();
            List<Bezier2> beziers = new List<Bezier2>();

            double angle = (2 * Math.PI) / DENSITY;

            for( int i = 0; i < DENSITY; i++)
            {             
                vectors.Add( VectorAtAngle(angle * i) );
                tangents.Add( TangentAtAngle(angle * i) );
            }

            for (int i = 0; i < DENSITY; i++)
            {
                Bezier3 bezier = new Bezier3();
                bezier.a = vectors[i];
                bezier.d = vectors[(i + 1) % DENSITY]; // line below: false false or something else?
                NetSegment.CalculateMiddlePoints(bezier.a, tangents[i], bezier.d, -1 * tangents[(i + 1) % DENSITY], false, false, out bezier.b, out bezier.c);
                beziers.Add(Bezier2.XZ(bezier));
            }

            Beziers = beziers;
        }

        /* What is an absolute angle? It is an angle between a point on the ellipse in the coordinate system of the (moved!) ellipse and X axis in standard coords.
         * Draw yourself a picture. ;) */

        public double RadiusAtAbsoluteAngle(double angle)
        {
            angle = angle - EllipseRotation;
            //Debug.Log(string.Format("Angle,er,newAngle {0} {1} {2}",angle+ EllipseRotation, EllipseRotation,angle));
            return RadiusAtAngle(angle);
        }

        /* Standard angle - we subtract the ellipse rotation */

        public double RadiusAtAngle(double angle)
        {
            return (RadiusMinor * RadiusMain) / Math.Sqrt(Math.Pow(RadiusMain * Math.Sin(angle), 2) + Math.Pow(RadiusMinor * Math.Cos(angle), 2));
        }

        /* Warning - this method works in the context I used it in the code, but didn't work for something else I had on my mind. I had to redo 
         * it using other procedure. Maybe I am just dumb and can't calculate the math. */
        public Vector3 VectorAtAbsoluteAngle(double angle)
        {
            return VectorAtAngle(angle - EllipseRotation);
        }

        public Vector3 TangentAtAbsoluteAngle(double angle)
        {
            return TangentAtAngle(angle - EllipseRotation);
        }

        public Vector3 VectorAtAngle(double angle)
        {
            Vector3 initialVector = new Vector3(RadiusMain, 0, 0);
            Vector3 rotSqVec = SquishVector(RotateVector(initialVector, angle), (float)Ratio);
            return  Center + RotateVector(rotSqVec, EllipseRotation);
        }

        public Vector3 TangentAtAngle(double angle)
        {
            Vector3 initialTangent = new Vector3(0, 0, 1);
            Vector3 rotSqTan = SquishVector(RotateVector(initialTangent, angle), (float)Ratio);
            return RotateVector(rotSqTan, EllipseRotation);
        }

        /* How to we create an ellipse? We stomp on a circle ;) */
        public static Vector3 SquishVector(Vector3 vector, float Ratio)
        {
            return new Vector3(vector.x,vector.y,vector.z*Ratio);
        }

        private void CalculateFocals()
        {
            float linearEccentricity = (float)(Math.Sqrt(RadiusMain * RadiusMain - RadiusMinor * RadiusMinor));
            mainAxisDirection.Normalize();
            Focal1 = Center + linearEccentricity * mainAxisDirection;
            Focal2 = Center - linearEccentricity * mainAxisDirection;
        }

        /* Utility */

        public static double VectorDistance(Vector3 vector1, Vector3 vector2)
        {
            return Math.Sqrt(Math.Pow(vector1.x - vector2.x, 2) + Math.Pow(vector1.z - vector2.z, 2));
        }

        public static Vector3 RotateVector(Vector3 origPoint, double angle)
        {
            return RotateVector(origPoint, angle, new Vector3(0, 0, 0));
        }
        public static Vector3 RotateVector(Vector3 origPoint, double angle, Vector3 pivot)
        {
            Vector3 difference = origPoint - pivot;

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            Vector3 newPoint = new Vector3((float)(difference.x * cos - difference.z * sin + pivot.x), pivot.y, (float)(difference.x * sin + difference.z * cos + pivot.z));

            return newPoint;
        }
        public static double VectorsAngle(Vector3 vector1)
        {
            return Math.Atan2( vector1.z, vector1.x );
        }
        public static double VectorsAngle(Vector3 vector1, Vector3 vector2)
        {
            return VectorsAngle(vector1, vector2, new Vector3(0,0,0));
        }
        public static double VectorsAngle(Vector3 vector1, Vector3 vector2, Vector3 pivot)
        {
            if (VectorDistance(vector1,vector2) < 0.01f) return (2 * Math.PI);
            Vector3 vec1 = vector1 - pivot;
            Vector3 vec2 = vector2 - pivot;
            float dot = vec1.x * vec2.x + vec1.z * vec2.z;
            float det = vec1.x * vec2.z - vec1.z * vec2.x;
            double angle = Math.Atan2(det, dot);
            if (angle > 0) return angle;

            return (2 * Math.PI + angle);
        }

    }
}
