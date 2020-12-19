using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortVisualization
{
    public static class ObjectFactory
    {
        public static (Vertex[], PrimitiveType) Rectangle(float width, float height)
        {
            width *= 0.5f; height *= 0.5f;
            Vertex[] res = new Vertex[]
            {
                new Vertex(new Vector4(+width, +height, 0f, 1f)),
                new Vertex(new Vector4(-width, +height, 0f, 1f)),
                new Vertex(new Vector4(-width, -height, 0f, 1f)),
                new Vertex(new Vector4(+width, -height, 0f, 1f)),
            };
            return (res, PrimitiveType.Quads);
        }

        public static (Vertex[], PrimitiveType) Arc(float radius, float angle, int precision = 300)
        {
            precision = Math.Max((int)(precision * angle / MathHelper.TwoPi), 1);
            Vertex[] res = new Vertex[precision + 2];
            res[0] = new Vertex(new Vector4(0f, 0f, 0f, 1f));
            for (int i = 0; i <= precision; ++i)
            {
                float ang = angle * i / precision;
                res[i+1] = new Vertex(new Vector4(radius * (float)Math.Cos(ang), radius * (float)Math.Sin(ang), 0f, 1f));
            }
            return (res, PrimitiveType.TriangleFan);
        }

        public static (Vertex[], PrimitiveType) Point()
        {
            return (Enumerable.Empty<Vertex>().ToArray(), PrimitiveType.Points);
        }
    }
}
