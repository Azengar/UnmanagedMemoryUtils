using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnmanagedMemoryUtils;

/// <summary>
/// Stores an array of pointers and provides easy access to the objects inside.
/// </summary>
/// <typeparam name="T"></typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct UnmanagedPointerArray<T> where T : unmanaged
{
    /// <summary>
    /// The size of a memory address on this system.
    /// </summary>
    public static readonly int AddressSize = sizeof(nint);

    /// <summary>
    /// Allocates an array of pointers of the specified <paramref name="length"/> using <see cref="Marshal.AllocHGlobal(int)"/> and returns a <see cref="UnmanagedPointerArray{T}"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The amount of pointers to store in this array.</param>
    /// <returns>The unmanaged pointer array that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedPointerArray<T> AllocateFromHGlobal(int length)
    {
        return new UnmanagedPointerArray<T>(Marshal.AllocHGlobal(length * AddressSize), length);
    }

    /// <summary>
    /// Allocates an array of pointers of the specified <paramref name="length"/> using <see cref="Marshal.AllocCoTaskMem(int)"/> and returns a <see cref="UnmanagedPointerArray{T}"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The amount of pointers to store in this array.</param>
    /// <returns>The unmanaged pointer array that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedPointerArray<T> AllocateFromCoTaskMem(int length)
    {
        return new UnmanagedPointerArray<T>(Marshal.AllocCoTaskMem(length * AddressSize), length);
    }

    /// <summary>
    /// Returns an empty <see cref="UnmanagedPointerArray{T}"/>.
    /// </summary>
    public static UnmanagedPointerArray<T> Empty => default;

    private readonly T** m_Pointer;
    private readonly int m_Length;

    /// <summary>
    /// Returns the pointer to the unmanaged memory.
    /// </summary>
    public readonly nint BasePointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return (nint)m_Pointer;
        }
    }
    /// <summary>
    /// Returns the void pointer to the unmanaged memory.
    /// </summary>
    public readonly void* VoidBasePointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Pointer;
        }
    }
    /// <summary>
    /// Returns the typed array pointer to the unmanaged memory.
    /// </summary>
    public readonly T** TypedBasePointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Pointer;
        }
    }
    /// <summary>
    /// Returns the amount of pointer to items that can be stored in this memory.
    /// </summary>
    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Length;
        }
    }
    /// <summary>
    /// Returns true if the length of the memory is 0.
    /// </summary>
    public readonly bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Length == 0;
        }
    }

    /// <summary>
    /// Returns true if the pointer of this unmanaged memory is the null pointer.
    /// </summary>
    public readonly bool IsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Pointer == null;
        }
    }

    /// <summary>
    /// Returns true if the length of the memory is 0, or if the pointer of this unmanaged memory is the null pointer.
    /// </summary>
    public readonly bool IsNullOrEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return IsEmpty || IsNull;
        }
    }

    /// <summary>
    /// Access the pointers inside this array.
    /// </summary>
    /// <param name="index">The index of the pointer to retrieve.</param>
    /// <returns>The pointer at the given index.</returns>
    public T* this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(index >= 0 && index < m_Length);
            return TypedBasePointer[index];
        }
        set
        {
            Debug.Assert(index >= 0 && index < m_Length);
            TypedBasePointer[index] = value;
        }
    }

    /// <summary>
    /// Creates a new array of pointers.
    /// </summary>
    /// <param name="pointer">The array pointer to the unmanaged pointer array, it MUST be unmanaged.</param>
    /// <param name="length">The amount of pointer to items that can be stored in this memory.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public UnmanagedPointerArray(T** pointer, int length)
    {
        m_Pointer = pointer;
        m_Length = length;
    }

    /// <summary>
    /// Creates a new array of pointers.
    /// </summary>
    /// <param name="pointer">The pointer to the unmanaged pointer array, it MUST be unmanaged</param>
    /// <param name="length">The amount of pointer to items that can be stored in this memory.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public UnmanagedPointerArray(nint pointer, int length) : this((T**)pointer, length) { }

    /// <summary>
    /// Forms a slice out of the given array pointer, beginning at <paramref name="start"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly UnmanagedPointerArray<T> Slice(int start)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        return new UnmanagedPointerArray<T>(m_Pointer + start, m_Length);
    }

    /// <summary>
    /// Forms a slice out of the given array pointer, beginning at <paramref name="start"/>, of given <paramref name="length"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice (exclusive).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly UnmanagedPointerArray<T> Slice(int start, int length)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        Debug.Assert(length <= m_Length - start);
        return new UnmanagedPointerArray<T>(m_Pointer + start, length);
    }

    /// <summary>
    /// Returns the pointer at the given index as a reference.
    /// </summary>
    /// <param name="index">The index of the pointer to retrieve.</param>
    /// <returns>The pointer at the given index as a reference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T GetAsRef(int index)
    {
        return ref Unsafe.AsRef<T>(this[index]);
    }

    public readonly bool Equals(UnmanagedPointerArray<T> other)
    {
        return m_Pointer == other.m_Pointer && m_Length == other.m_Length;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is UnmanagedPointerArray<T> memory && Equals(memory);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine((nint)m_Pointer, m_Length);
    }

    public static bool operator ==(UnmanagedPointerArray<T> left, UnmanagedPointerArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnmanagedPointerArray<T> left, UnmanagedPointerArray<T> right)
    {
        return !left.Equals(right);
    }
}
