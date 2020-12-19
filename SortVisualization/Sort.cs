using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SortVisualization
{
    public static class Sort
    {
        private static int _comp = 0, _swap = 0;

        public static IEnumerable<SortStep> Bubble(int[] array)
        {
            int comparison = 0, swap = 0;
            bool swapped = true;
            int k = 0;
            while (swapped)
            {
                swapped = false;
                for (int i = 1; i < array.Length - k; ++i)
                {
                    ++comparison;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (i-1, MainWindow.Compare), (i, MainWindow.Compare)
                    }, comparison, swap);
                    if (array[i-1] > array[i])
                    {
                        (array[i-1], array[i]) = (array[i], array[i-1]);
                        swapped = true;
                        ++swap;
                        yield return new SortStep(array, new (int, Color4)[]
                        {
                            (i-1, MainWindow.Swap), (i, MainWindow.Swap)
                        }, comparison, swap);
                    }
                }
                ++k;
            }
        }

        public static IEnumerable<SortStep> Insertion(int[] array)
        {
            int comparison = 0, swap = 0;
            for (int k = 1; k < array.Length; ++k)
            {
                int j = 0;
                while (k-1 >= j)
                {
                    ++comparison;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (k-j-1, MainWindow.Compare), (k-j, MainWindow.Compare)
                    }, comparison, swap);
                    if (array[k-j-1] > array[k-j])
                    {
                        (array[k-j-1], array[k-j]) = (array[k-j], array[k-j-1]);
                        ++swap;
                        yield return new SortStep(array, new (int, Color4)[]
                        {
                            (k-j-1, MainWindow.Swap), (k-j, MainWindow.Swap)
                        }, comparison, swap);
                    }
                    else break;
                    ++j;
                }

            }
        }

        public static IEnumerable<SortStep> Selection(int[] array)
        {
            int comparison = 0, swap = 0;
            for (int i = 0; i < array.Length - 1; ++i)
            {
                int minIdx = i;
                for (int j = i + 1; j < array.Length; ++j)
                {
                    ++comparison;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (minIdx, MainWindow.Compare), (j, MainWindow.Compare)
                    }, comparison, swap);
                    if (array[minIdx] > array[j])
                    {
                        minIdx = j;
                    }
                }
                if (i != minIdx)
                {
                    (array[minIdx], array[i]) = (array[i], array[minIdx]);
                    ++swap;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (minIdx, MainWindow.Swap), (i, MainWindow.Swap)
                    }, comparison, swap);
                }
            }
        }

        public static IEnumerable<SortStep> Shell(int[] array)
        {
            if (array.Length < 4)
                return Insertion(array);
            var steps = new List<SortStep>();
            _comp = _swap = 0;
            int logn = (int)Math.Floor(Math.Log(array.Length)) + 1;
            logn = logn / 2 * 2 + 1;
            for (int k = logn; k >= 1; k -= 2)
            {
                for (int p = 0; p < k; ++p)
                {
                    steps.AddRange(ShellStep(array, k, p));
                }
            }
            return steps;
        }

        private static IEnumerable<SortStep> ShellStep(int[] array, int k, int p) // 0 <= i < k
        {
            for (int i = p; i < array.Length; i += k)
            {
                int j = 0;
                while (i-k*(j+1) >= 0)
                {
                    ++_comp;
                    yield return new SortStep(array, new (int, Color4)[]
                        {
                        (i-k*(j+1), MainWindow.Compare), (i-k*j, MainWindow.Compare)
                        }, _comp, _swap);
                    if (array[i-k*(j+1)] > array[i-k*j])
                    {
                        ++_swap;
                        yield return new SortStep(array, new (int, Color4)[]
                        {
                        (i-k*(j+1), MainWindow.Swap), (i-k*j, MainWindow.Swap)
                        }, _comp, _swap);
                        (array[i-k*(j+1)], array[i-k*j]) = (array[i-k*j], array[i-k*(j+1)]);
                    }
                    else break;
                    j++;
                }
            }
        }

        public static IEnumerable<SortStep> Merge(int[] array)
        {
            int comparison = 0, swap = 0;
            var stack = new Stack<(int start, int mid, int end)>();
            var mergestack = new Stack<(int start, int mid, int end)>();
            stack.Push((0, array.Length / 2, array.Length));
            while (stack.Any())
            {
                var tuple = stack.Pop();
                (int start, int mid, int end) = tuple;
                if (start == mid || mid == end) continue;
                mergestack.Push(tuple);
                stack.Push((start, (start + mid) / 2, mid));
                stack.Push((mid, (mid + end) / 2, end));
            }
            while (mergestack.Any())
            {
                (int start, int mid, int end) = mergestack.Pop();
                List<int> merged = new List<int>(end - start);
                int i = start, j = mid;
                while (i < mid && j < end)
                {
                    ++comparison;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (i, MainWindow.Compare), (j, MainWindow.Compare)
                    }, comparison, swap);
                    if (array[i] < array[j])
                    {
                        merged.Add(array[i]);
                        ++i;
                    }
                    else
                    {
                        merged.Add(array[j]);
                        ++j;
                    }
                }
                while (i < mid)
                {
                    merged.Add(array[i]);
                    ++i;
                }
                while (j < end)
                {
                    merged.Add(array[j]);
                    ++j;
                }
                if (merged.Any((n) => n == 0))
                {
                    StringBuilder sb = new StringBuilder(2 * merged.Count);
                    foreach (var n in merged)
                        sb.Append(n + " ");
                }
                for (int k = 0; k < end - start; ++k)
                {
                    ++swap;
                    array[k + start] = merged[k];
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (k + start, MainWindow.Swap)
                    }, comparison, swap);
                }
            }
        }

        public static IEnumerable<SortStep> Quick(int[] array)
        {
            _comp = _swap = 0;
            return Quick(array, 0, array.Length);
        }

        private static IEnumerable<SortStep> Quick(int[] array, int offset, int size)
        {
            if (size <= 1)
            {
                return Enumerable.Empty<SortStep>();
            }
            List<SortStep> steps = new List<SortStep>();
            int pivot = offset + size - 1;
            int left = offset, right = pivot - 1;
            while (left <= right)
            {
                while (left <= right)
                {
                    ++_comp;
                    steps.Add(new SortStep(array, new (int, Color4)[]
                    {
                        (left, MainWindow.Compare), (pivot, MainWindow.Compare)
                    }, _comp, _swap));
                    if (array[left] < array[pivot]) ++left;
                    else break;
                }
                while (left <= right)
                {
                    ++_comp;
                    steps.Add(new SortStep(array, new (int, Color4)[]
                    {
                        (right, MainWindow.Compare), (pivot, MainWindow.Compare)
                    }, _comp, _swap));
                    if (array[right] > array[pivot]) --right;
                    else break;
                }
                if (left <= right)
                {
                    ++_swap;
                    (array[left], array[right]) = (array[right], array[left]);
                    steps.Add(new SortStep(array, new (int, Color4)[]
                    {
                        (left, MainWindow.Swap), (right, MainWindow.Swap)
                    }, _comp, _swap));
                }
            }
            ++_swap;
            (array[left], array[pivot]) = (array[pivot], array[left]);
            steps.Add(new SortStep(array, new (int, Color4)[]
                    {
                        (left, MainWindow.Swap), (pivot, MainWindow.Swap)
                    }, _comp, _swap));
            var quick1 = Quick(array, offset, left - offset);
            var quick2 = Quick(array, left + 1, offset + size - left - 1);
            steps.AddRange(quick1); steps.AddRange(quick2);
            return steps;
        }

        public static IEnumerable<SortStep> RadixLSD(int[] array)
        {
            int swap = 0;
            int mod = 4;
            int maxdigits = (int)Math.Floor(Math.Log(array.Max(), mod)) + 1;
            for (int d = 0; d < maxdigits; ++d)
            {
                var temp = (from i in Enumerable.Range(0, mod) select new Queue<int>()).ToArray();
                for (int i = 0; i < array.Length; ++i)
                {
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (i, MainWindow.Compare)
                    }, 0, swap);
                    temp[array[i] / IntPow(mod, d) % mod].Enqueue(array[i]);
                }
                var temp2 = new List<int>(array.Length);
                for (int i = 0; i < mod; ++i)
                    temp2.AddRange(temp[i]);
                for (int i = 0; i < array.Length; ++i)
                {
                    array[i] = temp2[i];
                    ++swap;
                    yield return new SortStep(array, new (int, Color4)[]
                    {
                        (i, MainWindow.Swap)
                    }, 0, swap);
                }
            }
        }

        /// <summary>Heap sort with max heap</summary>
        public static IEnumerable<SortStep> Heap(int[] array)
        {
            _comp = _swap = 0;
            int n = array.Length;
            var result = new List<SortStep>();
            for (int i = n / 2; i >= 0; --i)
                result.AddRange(HeapifyStep(array, i, n));
            for (int i = n-1; i >= 0; --i)
            {
                ++_swap;
                (array[0], array[i]) = (array[i], array[0]);
                result.Add(new SortStep(array, new (int, Color4)[]
                {
                    (0, MainWindow.Swap), (i, MainWindow.Swap)
                }, _comp, _swap));
                result.AddRange(HeapifyStep(array, 0, i));
            }
            return result;
        }

        public static IEnumerable<SortStep> HeapifyStep(int[] array, int idx, int size)
        {
            var result = new List<SortStep>();
            int largest = idx;
            int left = 2 * idx, right = left + 1;

            if (left < size)
            {
                ++_comp;
                result.Add(new SortStep(array, new (int, Color4)[]
                {
                    (largest, MainWindow.Compare), (left, MainWindow.Compare)
                }, _comp, _swap));
                if (array[largest] < array[left])
                {
                    largest = left;
                }
            }
            if (right < size)
            {
                ++_comp;
                result.Add(new SortStep(array, new (int, Color4)[]
                {
                    (largest, MainWindow.Compare), (right, MainWindow.Compare)
                }, _comp, _swap));
                if (array[largest] < array[right])
                {
                    largest = right;
                }
            }
            
            if (largest != idx)
            {
                ++_swap;
                (array[largest], array[idx]) = (array[idx], array[largest]);
                result.Add(new SortStep(array, new (int, Color4)[]
                {
                    (idx, MainWindow.Swap), (largest, MainWindow.Swap)
                }, _comp, _swap));
                result.AddRange(HeapifyStep(array, largest, size));
            }

            return result;
        }

        private static int IntPow(int b, int n)
        {
            if (n == 1) return b;
            else if (n <= 0) return 1;
            int r = IntPow(b, n/2);
            if (n % 2 == 0)
                return r * r;
            return r * r * b;
        }
    }
}
