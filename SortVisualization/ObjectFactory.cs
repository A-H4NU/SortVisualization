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
        public static (ColoredVertex[], PrimitiveType) Rectangle(float width, float height, Color4 color)
        {
            width *= 0.5f; height *= 0.5f;
            ColoredVertex[] res = new ColoredVertex[]
            {
                new ColoredVertex(new Vector4(+width, +height, 0f, 1f), color),
                new ColoredVertex(new Vector4(-width, +height, 0f, 1f), color),
                new ColoredVertex(new Vector4(-width, -height, 0f, 1f), color),
                new ColoredVertex(new Vector4(+width, -height, 0f, 1f), color),
            };
            return (res, PrimitiveType.Quads);
        }
    }
}
