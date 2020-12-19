using OpenTK;
using OpenTK.Graphics;

namespace SortVisualization
{
    public struct Vertex
    {
        public const int SIZE = 16;

        public readonly Vector4 Position;

        public Vertex(Vector4 position)
        {
            Position = position;
        }
    }
}
