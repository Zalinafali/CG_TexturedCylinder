using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TexturedCylinder
{
    class Cylinder
    {
        // number of sides
        public int n = 12;

        public Vertex[] vertices = null;
        public Triangle[] triangles = null;

        public Cylinder(double h, double r)
        {
            // Create mesh
            int numOfVertices = 4 * n + 2;
            int numOfTriangles = 4 * n;

            vertices = new Vertex[numOfVertices];
            triangles = new Triangle[numOfTriangles];

            // Top base
            int[] normal = { 0, 1, 0, 0 };
            float[] points = { 0, (float)h, 0, 1 };

            vertices[0] = new Vertex(points, normal);

            for(int i = 0; i < n; i++)
            {
                points[0] = (float) (r * Math.Cos(2 * Math.PI * i / n));
                points[2] = (float) (r * Math.Sin(2 * Math.PI * i / n));

                vertices[i + 1] = new Vertex(points, normal);
            }

            // Bottom base
            normal[1] = -1;

            points[0] = 0;
            points[1] = 0;
            points[2] = 0;

            vertices[4 * n + 1] = new Vertex(points, normal);

            for (int i = 0; i < n; i++)
            {
                points[0] = (float) (r * Math.Cos(2 * Math.PI * i / n));
                points[2] = (float) (r * Math.Sin(2 * Math.PI * i / n));

                vertices[3*n + 1 + i] = new Vertex(points, normal);
            }

            // Sides
            normal[1] = 0;
            normal[3] = 0;
            for (int i = n + 1; i < 2*n + 1; i++)
            {
                points = vertices[i - n].p;

                normal[0] = (int)(points[0] / r);
                normal[2] = (int)(points[2] / r);

                vertices[i] = new Vertex(points, normal);
            }

            for (int i = 2*n + 1; i < 3 * n + 1; i++)
            {
                points = vertices[i + n].p;

                normal[0] = (int)(points[0] / r);
                normal[2] = (int)(points[2] / r);

                vertices[i] = new Vertex(points, normal);
            }

            SetTextureCoordinates();

            // Create Triangles
            CreateTriangles();
        }

        public void CreateTriangles()
        {
            // Top base
            triangles[n - 1] = new Triangle(vertices[0], vertices[1], vertices[n]);
            for (int i = 0; i < n - 1; i++)
            {
                triangles[i] = new Triangle(vertices[0], vertices[i + 2], vertices[i + 1]);
            }

            // Bottom base
            triangles[4 * n - 1] = new Triangle(vertices[4 * n + 1], vertices[4 * n], vertices[3 * n + 1]);
            for (int i = 3 * n; i < 4 * n - 1; i++)
            {
                triangles[i] = new Triangle(vertices[4 * n + 1], vertices[i + 1], vertices[i + 2]);
            }

            // Sides
            triangles[2 * n - 1] = new Triangle(vertices[2 * n], vertices[n + 1], vertices[3 * n]);
            triangles[3 * n - 1] = new Triangle(vertices[3 * n], vertices[n + 1], vertices[2 * n + 1]);

            for (int i = n; i < 2 * n - 1; i++)
            {
                triangles[i] = new Triangle(vertices[i + 1], vertices[i + 2], vertices[i + 1 + n]);
            }

            for (int i = 2 * n; i < 3 * n - 1; i++)
            {
                triangles[i] = new Triangle(vertices[i + 1], vertices[i + 2 - n], vertices[i + 2]);
            }
        }

        private void SetTextureCoordinates()
        {
            // top base
            vertices[0].t.X = 0.25f;
            vertices[0].t.Y = 0.25f;

            for(int i = 1; i <= n; i++)
            {
                double ang = 2 * Math.PI * (i - 1) / n; 

                vertices[i].t.X = (float)(1 + Math.Cos(ang)) / 4;
                vertices[i].t.Y = (float)(1 + Math.Sin(ang)) / 4;
            }

            // bottom base
            vertices[4*n + 1].t.X = 0.75f;
            vertices[4*n + 1].t.Y = 0.25f;

            for(int i = 3*n + 1; i <= 4*n; i++)
            {
                double ang = 2 * Math.PI * (i - 1) / n;

                vertices[i].t.X = (float)(3 + Math.Cos(ang)) / 4;
                vertices[i].t.Y = (float)(1 + Math.Sin(ang)) / 4;
            }

            // side along the top base
            for (int i = 1; i <= n; i++)
            {
                vertices[i+n].t.X = (float)(i-1)/(n-1);
                vertices[i+n].t.Y = 1;
            }

            // side along the bottom base
            for (int i = 1; i <= n; i++)
            {
                vertices[i + 2*n].t.X = (float)(i - 1) / (n - 1);
                vertices[i + 2*n].t.Y = 0.5f;
            }
        }
        
        public void Scaling(float sx, float sy, float sz)
        {
            Matrix4x4 S = new Matrix4x4( sx,  0,  0,  0,
                                          0, sy,  0,  0,
                                          0,  0, sz,  0,
                                          0,  0,  0,  1  );

            for(int i = 0; i < 4*n + 2; i++)
            {
                vertices[i].pv = Vector4.Transform(vertices[i].pv, S);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            CreateTriangles();
        }

        public void RotatingY(float theta)
        {
            double thetaRad = theta * Math.PI / 180;
            float cos = (float) Math.Cos(thetaRad);
            float sin = (float)Math.Sin(thetaRad);

            Matrix4x4 Ry = new Matrix4x4( cos,  0, sin,  0,
                                           0 ,  1,  0 ,  0,
                                         -sin,  0, cos,  0,
                                           0,   0,  0,   1  );

            for (int i = 0; i < 4 * n + 2; i++)
            {
                vertices[i].pv = Vector4.Transform(vertices[i].pv, Ry);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            CreateTriangles();
        }

        public void RotatingX(float theta)
        {
            double thetaRad = theta * Math.PI / 180;
            float cos = (float)Math.Cos(thetaRad);
            float sin = (float)Math.Sin(thetaRad);

            Matrix4x4 Ry = new Matrix4x4( 1 ,  0 ,   0 ,  0,
                                          0 , cos, -sin,  0,
                                          0 , sin,  cos,  0,
                                          0 ,  0 ,   0 ,  1);

            for (int i = 0; i < 4 * n + 2; i++)
            {
                vertices[i].pv = Vector4.Transform(vertices[i].pv, Ry);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            CreateTriangles();
        }

        public void View(float x0, float y0, Matrix4x4 CVM, Matrix4x4 PPM)
        {
            // global coordinates
            Vector4 globCoord = new Vector4(x0, y0, 0, 0);
            for (int i = 0; i < 4 * n + 2; i++)
            {
                vertices[i].pv = Vector4.Add(vertices[i].pv, globCoord);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            // camera coordinate system
            for (int i = 0; i < 4 * n + 2; i++)
            {
                vertices[i].pv = Vector4.Transform(vertices[i].pv, CVM);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            // perspective projection
            for (int i = 0; i < 4 * n + 2; i++)
            {
                vertices[i].pv = Vector4.Transform(vertices[i].pv, PPM);
                vertices[i].pv = Vector4.Divide(vertices[i].pv, vertices[i].pv.W);

                vertices[i].pv.CopyTo(vertices[i].p);
            }

            CreateTriangles();
        }
    }
}
