using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortVisualization
{
    public struct SortStep
    {
        private readonly int[] Sequence;

        public readonly IEnumerable<(int, Color4)> Processing;

        public readonly int Comparison, Swap;

        public int this[int index]
        {
            get => Sequence[index];
        }

        public int[] GetClonedSequence() => (int[])Sequence.Clone();

        public int Count => Sequence.Length;

        public SortStep(int[] sequence, IEnumerable<(int, Color4)> processing, int comparison = 0, int swap = 0)
        {
            Sequence = (int[])sequence.Clone();
            Processing = processing;
            Comparison = comparison;
            Swap = swap;
        }
    }
}
