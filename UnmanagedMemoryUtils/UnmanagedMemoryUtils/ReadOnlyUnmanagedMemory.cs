using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnmanagedMemoryUtils;

/// <summary>
/// A faster alternative to <see cref="ReadOnlyMemory{T}"/> when working with unmanaged memory.
/// Stores a pointer to contiguous unmanaged memory and the length of that memory. The memory cannot be written to.
/// </summary>
/// <remarks>
/// This struct is only 16 bytes on 64-bit or 8 bytes on 32-bit which makes it easy to pass by value.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct ReadOnlyUnmanagedMemory : IEquatable<ReadOnlyUnmanagedMemory>
{
    /// <summary>
    /// Allocates memory of the specified <paramref name="length"/> using <see cref="Marshal.AllocHGlobal(int)"/> and returns a <see cref="ReadOnlyUnmanagedMemory"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The length of the memory range.</param>
    /// <returns>The unmanaged memory that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyUnmanagedMemory AllocateFromHGlobal(int length)
    {
        return new ReadOnlyUnmanagedMemory(Marshal.AllocHGlobal(length), length);
    }

    /// <summary>
    /// Allocates memory of the specified <paramref name="length"/> using <see cref="Marshal.AllocCoTaskMem(int)"/> and returns a <see cref="ReadOnlyUnmanagedMemory"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The length of the memory range.</param>
    /// <returns>The unmanaged memory that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyUnmanagedMemory AllocateFromCoTaskMem(int length)
    {
        return new ReadOnlyUnmanagedMemory(Marshal.AllocCoTaskMem(length), length);
    }

    /// <summary>
    /// Returns an empty <see cref="ReadOnlyUnmanagedMemory"/>.
    /// </summary>
    public static ReadOnlyUnmanagedMemory Empty => default;

    private readonly nint m_Pointer;
    private readonly int m_Length;

    /// <summary>
    /// Return the pointer to the unmanaged memory.
    /// </summary>
    public readonly nint Pointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Pointer;
        }
    }
    /// <summary>
    /// Returns the void pointer to the unmanaged memory.
    /// </summary>
    public readonly void* VoidPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return (void*)m_Pointer;
        }
    }
    /// <summary>
    /// Returns the length of the unmanaged memory.
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
    /// Returns a readonly span from the memory.
    /// </summary>
    public readonly ReadOnlySpan<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return new(VoidPointer, m_Length);
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
            return m_Pointer == nint.Zero;
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
    /// Creates a new read-only unmanaged memory.
    /// </summary>
    /// <param name="pointer">The pointer to the unmanaged memory, it MUST be unmanaged.</param>
    /// <param name="length">The length of the unmanaged memory range.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public ReadOnlyUnmanagedMemory(nint pointer, int length)
    {
        Debug.Assert(pointer != nint.Zero);
        Debug.Assert(length >= 0);
        m_Pointer = pointer;
        m_Length = length;
    }

    /// <summary>
    /// Creates a new read-only unmanaged memory.
    /// </summary>
    /// <param name="pointer">The void pointer to the unmanaged memory, it MUST be unmanaged.</param>
    /// <param name="length">The length of the unmanaged memory range.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public ReadOnlyUnmanagedMemory(void* pointer, int length) : this((nint)pointer, length) { }

    /// <summary>
    /// Forms a slice out of the given memory, beginning at <paramref name="start"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlyUnmanagedMemory Slice(int start)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        return new ReadOnlyUnmanagedMemory(m_Pointer + start, m_Length);
    }

    /// <summary>
    /// Forms a slice out of the given memory, beginning at <paramref name="start"/>, of given <paramref name="length"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice (exclusive).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlyUnmanagedMemory Slice(int start, int length)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        Debug.Assert(length <= m_Length - start);
        return new ReadOnlyUnmanagedMemory(m_Pointer + start, length);
    }

    /// <summary>
    /// Copies the memory to the destination buffer as fast as possible without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(ReadOnlyUnmanagedMemory)"/> for a safe but a bit slower alternative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(UnmanagedMemory destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Unsafe.CopyBlockUnaligned(destination.VoidPointer, VoidPointer, (uint)Length);
    }

    /// <summary>
    /// Copies the memory to the destination buffer as fast as possible without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(Memory{byte})"/> for a safe but a bit slower alternative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Memory<byte> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Unsafe.CopyBlockUnaligned(ref destination.Span[0], ref ((byte*)Pointer)[0], (uint)Length);
    }

    /// <summary>
    /// Copies the memory to the destination buffer quickly without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(byte[])"/> for a safe but a bit slower alternative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(byte[] destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Unsafe.CopyBlockUnaligned(ref destination[0], ref ((byte*)Pointer)[0], (uint)Length);
    }

    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(UnmanagedMemory destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.Span);
    }


    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(Memory<byte> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.Span);
    }

    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(byte[] destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.AsSpan());
    }

    /// <summary>
    /// Reinterprets the unmanaged memory range as a reference to an unmanaged struct without allocating any extra memory.
    /// </summary>
    /// <typeparam name="T">The type of unmanaged struct to return.</typeparam>
    /// <returns>The reference to a struct of the given type.</returns>
    /// <remarks>You are responsible for making sure that the given struct type fits this memory range, otherwise your application will crash.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T AsRef<T>() where T : unmanaged => ref Unsafe.AsRef<T>((void*)m_Pointer);

    public readonly bool Equals(ReadOnlyUnmanagedMemory other)
    {
        return m_Pointer == other.m_Pointer && m_Length == other.m_Length;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ReadOnlyUnmanagedMemory memory && Equals(memory);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(m_Pointer, m_Length);
    }

    public static bool operator ==(ReadOnlyUnmanagedMemory left, ReadOnlyUnmanagedMemory right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ReadOnlyUnmanagedMemory left, ReadOnlyUnmanagedMemory right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// <para>
/// A faster alternative to <see cref="ReadOnlyMemory{T}"/> when working with unmanaged memory.
/// Stores a pointer to contiguous unmanaged memory and the length of that memory. The memory cannot be written to.
/// </para>
/// <para>
/// This generic version is very slightly slower than the non-generic alternative <see cref="ReadOnlyUnmanagedMemory"/>, so if you only need to work with bytes use that instead.
/// </para>
/// </summary>
/// <remarks>
/// This struct is only 16 bytes on 64-bit or 8 bytes on 32-bit which makes it easy to pass by value.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct ReadOnlyUnmanagedMemory<T> : IEquatable<ReadOnlyUnmanagedMemory<T>> where T : unmanaged
{
    /// <summary>
    /// Allocates memory of the specified <paramref name="length"/> using <see cref="Marshal.AllocHGlobal(int)"/> and returns a <see cref="ReadOnlyUnmanagedMemory{T}"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The amount of items to store in this memory.</param>
    /// <returns>The unmanaged memory that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyUnmanagedMemory<T> AllocateFromHGlobal(int length)
    {
        return new ReadOnlyUnmanagedMemory<T>(Marshal.AllocHGlobal(length * TypeSize), length);
    }

    /// <summary>
    /// Allocates memory of the specified <paramref name="length"/> using <see cref="Marshal.AllocCoTaskMem(int)"/> and returns a <see cref="ReadOnlyUnmanagedMemory{T}"/> objects that wraps it.
    /// </summary>
    /// <param name="length">The amount of items to store in this memory.</param>
    /// <returns>The unmanaged memory that points to the newly allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyUnmanagedMemory<T> AllocateFromCoTaskMem(int length)
    {
        return new ReadOnlyUnmanagedMemory<T>(Marshal.AllocCoTaskMem(length * TypeSize), length);
    }

    /// <summary>
    /// Returns an empty <see cref="ReadOnlyUnmanagedMemory{T}"/>.
    /// </summary>
    public static ReadOnlyUnmanagedMemory<T> Empty => default;

    /// <summary>
    /// The size of the item type T.
    /// </summary>
    private static int TypeSize { get; } = sizeof(T);

    private readonly T* m_Pointer;
    private readonly int m_Length;

    /// <summary>
    /// Returns the pointer to the unmanaged memory.
    /// </summary>
    public readonly nint Pointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return (nint)m_Pointer;
        }
    }
    /// <summary>
    /// Returns the typed pointer to the unmanaged memory.
    /// </summary>
    public readonly T* TypedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Pointer;
        }
    }
    /// <summary>
    /// Returns the amount of items that can be stored in this memory.
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
    /// Returns the length of the memory range in bytes.
    /// </summary>
    public readonly int ByteLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_Length * TypeSize;
        }
    }

    /// <summary>
    /// Returns a span from the memory.
    /// </summary>
    public readonly ReadOnlySpan<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return new(m_Pointer, m_Length);
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
    /// Creates a new read-only unmanaged memory.
    /// </summary>
    /// <param name="pointer">The pointer to the unmanaged memory, it MUST be unmanaged.</param>
    /// <param name="length">The amount of items that can be stored in this memory.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public ReadOnlyUnmanagedMemory(T* pointer, int length)
    {
        m_Pointer = pointer;
        m_Length = length;
    }

    /// <summary>
    /// Creates a new read-only unmanaged memory.
    /// </summary>
    /// <param name="pointer">The void pointer to the unmanaged memory, it MUST be unmanaged</param>
    /// <param name="length">The amount of items that can be stored in this memory.</param>
    /// <remarks>
    /// The pointer MUST point to unmanaged memory, otherwise you expose yourself to random crashes.
    /// </remarks>
    public ReadOnlyUnmanagedMemory(nint pointer, int length) : this((T*)pointer, length) { }

    /// <summary>
    /// Forms a slice out of the given memory, beginning at <paramref name="start"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlyUnmanagedMemory<T> Slice(int start)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        return new ReadOnlyUnmanagedMemory<T>(m_Pointer + start, m_Length);
    }

    /// <summary>
    /// Forms a slice out of the given memory, beginning at <paramref name="start"/>, of given <paramref name="length"/>.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice (exclusive).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlyUnmanagedMemory<T> Slice(int start, int length)
    {
        Debug.Assert(start >= 0 && start < m_Length);
        Debug.Assert(length <= m_Length - start);
        return new ReadOnlyUnmanagedMemory<T>(m_Pointer + start, length);
    }

    /// <summary>
    /// Copies the memory to the destination buffer as fast as possible without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(ReadOnlyUnmanagedMemory{T})"/> for a safe but a bit slower alternative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(UnmanagedMemory<T> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Unsafe.CopyBlockUnaligned(TypedPointer, destination.TypedPointer, (uint)ByteLength);
    }

    /// <summary>
    /// Copy the memory to the destination managed buffer quickly without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(Memory{T})"/> for a safe but a bit slower alternative
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Memory<T> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Memory<byte> buffer = Unsafe.As<Memory<T>, Memory<byte>>(ref destination);
        Unsafe.CopyBlockUnaligned(ref buffer.Span[0], ref ((byte*)Pointer)[0], (uint)ByteLength);
    }

    /// <summary>
    /// Copy the memory to the destination managed buffer quickly without caring if the destination is valid.
    /// </summary>
    /// <param name="destination">The destination buffer</param>
    /// <remarks>
    /// When unsure about the destination, see <see cref="CopyToSafe(T[])"/> for a safe but a bit slower alternative
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(T[] destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span<byte> buffer = MemoryMarshal.AsBytes(destination.AsSpan());
        Unsafe.CopyBlockUnaligned(ref buffer[0], ref ((byte*)Pointer)[0], (uint)ByteLength);
    }

    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(UnmanagedMemory<T> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.Span);
    }


    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(Memory<T> destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.Span);
    }

    /// <summary>
    /// Copies the contents of the memory into the destination. 
    /// If the source and destination overlap, this method behaves as if the original values are in a temporary location before the destination is overwritten.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is shorter than the source.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyToSafe(T[] destination)
    {
        Debug.Assert(destination.Length >= m_Length);
        Span.CopyTo(destination.AsSpan());
    }

    /// <summary>
    /// Reinterprets the unmanaged memory range as a reference to an unmanaged struct without allocating any extra memory.
    /// </summary>
    /// <typeparam name="T">The type of unmanaged struct to return.</typeparam>
    /// <returns>The reference to a struct of the given type.</returns>
    /// <remarks>You are responsible for making sure that the given struct type fits this memory range, otherwise your application will crash.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref TRef AsRef<TRef>() where TRef : unmanaged => ref Unsafe.AsRef<TRef>(m_Pointer);

    public readonly bool Equals(ReadOnlyUnmanagedMemory<T> other)
    {
        return m_Pointer == other.m_Pointer && m_Length == other.m_Length;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ReadOnlyUnmanagedMemory<T> memory && Equals(memory);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine((nint)m_Pointer, m_Length);
    }

    public static bool operator ==(ReadOnlyUnmanagedMemory<T> left, ReadOnlyUnmanagedMemory<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ReadOnlyUnmanagedMemory<T> left, ReadOnlyUnmanagedMemory<T> right)
    {
        return !left.Equals(right);
    }
}