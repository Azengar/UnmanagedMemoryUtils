using System.Runtime.InteropServices;

namespace UnmanagedMemoryUtils
{
    /// <summary>
    /// <para>
    /// Contains an immutable unmanaged string. 
    /// </para>
    /// <para>
    /// This struct stores the managed string in parallel of the unmanaged string. Because of this the managed or unmanaged strings contained should never be modified.
    /// </para>
    /// <para>
    /// The advantage is that accessing the managed string via <see cref="Value"/> or <see cref="ToString"/> is faster than in the <see cref="UnmanagedString"/> alternative.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Modifying the data found at <see cref="Pointer"/> will result in an invalid <see cref="Value"/>.
    /// </remarks>
    public unsafe struct UnmanagedStringReadOnly : IUnmanagedString, IEquatable<UnmanagedStringReadOnly>
    {
        /// <summary>
        /// Represents an empty unmanaged string (equal to nullptr).
        /// </summary>
        public static UnmanagedStringReadOnly Empty { get; } = default;

        /// <summary>
        /// Creates a new unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemAuto(string?)"/>.
        /// </summary>
        /// <param name="value">The managed string.</param>
        /// <returns>The unmanaged string.</returns>
        public static UnmanagedStringReadOnly FromAuto(string value) => new(value, Marshal.StringToCoTaskMemAuto, Marshal.FreeCoTaskMem);
        /// <summary>
        /// Creates a new ANSI unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemAnsi(string?)"/>.
        /// </summary>
        /// <param name="value">The managed string.</param>
        /// <returns>The unmanaged string.</returns>
        public static UnmanagedStringReadOnly FromAnsi(string value) => new(value, Marshal.StringToCoTaskMemAnsi, Marshal.FreeCoTaskMem);
        /// <summary>
        /// Creates a new UTF8 unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemUTF8(string?)"/>.
        /// </summary>
        /// <param name="value">The managed string.</param>
        /// <returns>The unmanaged string.</returns>
        public static UnmanagedStringReadOnly FromUTF8(string value) => new(value, Marshal.StringToCoTaskMemUTF8, Marshal.FreeCoTaskMem);
        /// <summary>
        /// Creates a new Unicode unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemUni(string?)"/>.
        /// </summary>
        /// <param name="value">The managed string.</param>
        /// <returns>The unmanaged string.</returns>
        public static UnmanagedStringReadOnly FromUnicode(string value) => new(value, Marshal.StringToCoTaskMemUni, Marshal.FreeCoTaskMem);

        private readonly Action<nint> m_FreeCallback;

        /// <summary>
        /// The pointer that points to the unmanaged string.
        /// </summary>
        public nint Pointer { get; private set; }
        /// <summary>
        /// The managed string.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Create a new unmanaged string with a custom allocator and free functions.
        /// </summary>
        /// <param name="value">The managed string.</param>
        /// <param name="allocator">The function used to allocate the unmanaged string.</param>
        /// <param name="free">The function used to free the unmanaged string.</param>
        public UnmanagedStringReadOnly(string value, Func<string, nint> allocator, Action<nint> free)
        {
            Value = value;
            Pointer = allocator.Invoke(value);
            m_FreeCallback = free;
        }

        /// <summary>
        /// Free the unmanaged string. The <see cref="Pointer"/> is set to <see cref="nint.Zero"/> and the <see cref="Value"/> is set to <see cref="string.Empty"/>.
        /// </summary>
        public void Free()
        {
            m_FreeCallback.Invoke(Pointer);
            Pointer = nint.Zero;
            Value = string.Empty;
        }

        public override readonly string ToString()
        {
            return Value;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is UnmanagedStringReadOnly other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Value, Pointer);
        }

        public readonly bool Equals(UnmanagedStringReadOnly other)
        {
            return Pointer == other.Pointer;
        }

        public static bool operator ==(UnmanagedStringReadOnly left, UnmanagedStringReadOnly right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnmanagedStringReadOnly left, UnmanagedStringReadOnly right)
        {
            return left.Equals(right);
        }

        public static implicit operator string(UnmanagedStringReadOnly value) => value.Value;
    }
}