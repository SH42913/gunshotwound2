﻿#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    public struct NativeIntHashMap<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* capacityPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* capacityMinusOnePtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lastIndexPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* buckets;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe IntHashMapSlot* slots;
        
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        internal unsafe TNative* data;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.hashMap = this;
            e.index   = 0;
            e.current = default;
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Enumerator {
            public NativeIntHashMap<TNative> hashMap;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < *this.hashMap.lastIndexPtr; ++this.index) {
                    ref var slot = ref this.hashMap.slots[this.index];
                    if (slot.key - 1 < 0) {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index   = *this.hashMap.lastIndexPtr + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;
        }
    }
}
#endif