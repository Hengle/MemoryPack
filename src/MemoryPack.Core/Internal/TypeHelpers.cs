﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Internal;

// TODO: use or remove this?

internal static class TypeHelpers
{
    static readonly MethodInfo isReferenceOrContainsReferences = typeof(RuntimeHelpers).GetMethod("IsReferenceOrContainsReferences")!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReferenceOrNullable<T>()
    {
        return Cache<T>.IsReferenceOrNullable;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetUnmanagedSZArrayElementSize<T>(out int size)
    {
        var x = Cache<T>.UnmanagedSZArrayElementSize;
        if (x.HasValue)
        {
            size = x.Value;
            return true;
        }
        else
        {
            size = 0;
            return false;
        }
    }

    static class Cache<T>
    {
        public static bool IsReferenceOrNullable;
        public static int? UnmanagedSZArrayElementSize;

        static Cache()
        {
            try
            {
                var type = typeof(T);
                IsReferenceOrNullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

                if (type.IsSZArray)
                {
                    var elementType = type.GetElementType();
                    bool containsReference = (bool)(isReferenceOrContainsReferences.MakeGenericMethod(elementType!).Invoke(null, null)!);
                    if (!containsReference)
                    {
                        UnmanagedSZArrayElementSize = Marshal.SizeOf(elementType!);
                    }
                }
            }
            catch
            {
                UnmanagedSZArrayElementSize = null;
            }
        }
    }
}