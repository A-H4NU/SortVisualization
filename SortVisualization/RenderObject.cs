using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortVisualization
{
    public class RenderObject : IDisposable
    {
        public Vector3 Position, Scale = Vector3.One, Rotation;

        private readonly int _program, _vertexArray, _buffer, _verticeCount;
        private PrimitiveType _renderType;

        public RenderObject((ColoredVertex[], PrimitiveType) tuple, int program)
        {
            _program = program;
            var vertices = tuple.Item1;
            _verticeCount = vertices.Length;
            _renderType = tuple.Item2;

            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();
            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            GL.NamedBufferStorage(
                _buffer,
                ColoredVertex.SIZE * _verticeCount,
                vertices,
                BufferStorageFlags.MapWriteBit);

            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                0,
                4,
                VertexAttribType.Float,
                false,
                0);

            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                1,
                4,
                VertexAttribType.Float,
                false,
                16);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, ColoredVertex.SIZE);
        }

        public void Render(ref Matrix4 projection, in Vector3 translation, in Vector3 scale, in Vector3 rotation)
        {
            Matrix4 t = Matrix4.CreateTranslation(Position + translation);
            Matrix4 s = Matrix4.CreateScale(Scale * scale);
            Matrix4 r = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z);
            Matrix4 modelView = r * s* t;
            GL.UseProgram(_program);
            GL.UniformMatrix4(10, false, ref modelView);
            GL.UniformMatrix4(11, false, ref projection);
            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.DrawArrays(_renderType, 0, _verticeCount);
        }

        public void Render(ref Matrix4 projection) => Render(ref projection, Vector3.Zero, Vector3.One, Vector3.Zero);

        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_buffer);
        }
    }
}
