namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class FastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this FastList<T> list) {
            var index = list.length;
            if (++list.length == list.capacity) {
                ArrayHelpers.Grow(ref list.data, list.capacity <<= 1);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this FastList<T> list, T value) {
            var index = list.length;
            if (++list.length == list.capacity) {
                ArrayHelpers.Grow(ref list.data, list.capacity <<= 1);
            }

            list.data[index] = value;
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListRange<T>(this FastList<T> list, FastList<T> other) {
            if (other.length > 0) {
                var newSize = list.length + other.length;
                if (newSize > list.capacity) {
                    while (newSize > list.capacity) {
                        list.capacity <<= 1;
                    }

                    ArrayHelpers.Grow(ref list.data, list.capacity);
                }

                if (list == other) {
                    Array.Copy(list.data, 0, list.data, list.length, list.length);
                }
                else {
                    Array.Copy(other.data, 0, list.data, list.length, other.length);
                }

                list.length += other.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this FastList<T> list, int source, int destination)
        {
            list.data[destination] = list.data[source];
            list.lastSwappedIndex = destination;
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this FastList<T> list, T value) => ArrayHelpers.IndexOf(list.data, value, list.comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this FastList<T> list, T value) => list.RemoveAt(list.IndexOf(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwap<T>(this FastList<T> list, T value, out FastList<T>.ResultSwap swap) => list.RemoveAtSwap(list.IndexOf(value), out swap);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt<T>(this FastList<T> list, int index) {
            --list.length;
            if (index < list.length) {
                Array.Copy(list.data, index + 1, list.data, index, list.length - index);
            }

            list.data[list.length] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this FastList<T> list, int index, out FastList<T>.ResultSwap swap) {
            if (list.length-- > 1) {
                swap.oldIndex = list.length;
                swap.newIndex = index;

                list.data[swap.newIndex] = list.data[swap.oldIndex];
                list.data[swap.oldIndex] = default;
                list.lastSwappedIndex    = index;
                return true;
            }

            list.lastSwappedIndex = -1;
            swap = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this FastList<T> list, int index, out T newValue) {
            if (list.length-- > 1) {
                var oldIndex = list.length;
                newValue = list.data[index] = list.data[oldIndex];
                list.lastSwappedIndex = index;
                return true;
            }

            list.lastSwappedIndex = -1;
            newValue = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this FastList<T> list) {
            if (list.length <= 0) {
                return;
            }

            Array.Clear(list.data, 0, list.length);
            list.length = 0;
            list.lastSwappedIndex = -1;
        }

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list) => Array.Sort(list.data, 0, list.length, null);

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list, int index, int len) => Array.Sort(list.data, index, len, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this FastList<T> list) {
            var newArray = new T[list.length];
            Array.Copy(list.data, 0, newArray, 0, list.length);
            return newArray;
        }
    }
}