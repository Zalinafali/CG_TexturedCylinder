using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TexturedCylinder
{
    class ActiveEdge
    {
        public int ymax;
        public double x;
        public double m;
        public bool horizontal = false;

        public Vertex up;
        public Vertex down;

        public ActiveEdge(int ymax, double x, double m)
        {
            this.ymax = ymax;
            this.x = x;
            this.m = m;
        }

        public ActiveEdge(Vector4 p, Vector4 q, Vertex vp, Vertex vq)
        {
            if ((int)p.Y > (int)q.Y)
            {
                ymax = (int)p.Y;
                x = q.X;

                m = (p.X - q.X) / (p.Y - q.Y);

                up = vp;
                down = vq;
            }
            else if ((int)p.Y < (int)q.Y)
            {
                ymax = (int)q.Y;
                x = p.X;

                m = (p.X - q.X) / (p.Y - q.Y);

                up = vq;
                down = vp;
            }
            else
            {
                horizontal = true;

                up = vp;
                down = vq;
            }
        }
    }
}
