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
        public readonly IEnumerable<(int index, int value)> Changes;

        public readonly IEnumerable<(int, Color4)> Processing;

        public readonly int Comparison, Swap;

        public SortStep(int[] sequence, IEnumerable<(int, Color4)> processing, int comparison, int swap, params int[] changedIndices)
        {
            Changes = (from i in changedIndices select (i, sequence[i])).ToArray();
            Processing = processing;
            Comparison = comparison;
            Swap = swap;
        }
    }
}
