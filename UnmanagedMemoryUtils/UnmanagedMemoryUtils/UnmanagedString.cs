using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnmanagedMemoryUtils;

/// <summary>
/// <para>
/// Contains an unmanaged string. 
/// </para>
/// <para>
/// The managed string is accessed by calling <see cref="ToString"/> which will marshal the string from unmanaged to a managed <see cref="string"/>.
/// </para>
/// <para>
/// If the unmanaged string will never be modified you should use the faster alternative <see cref="UnmanagedStringReadOnly"/> instead.
/// </para>
/// </summary>
public unsafe struct UnmanagedString : IUnmanagedString, IEquatable<UnmanagedString>
{
    /// <summary>
    /// Represents an empty unmanaged string (equal to nullptr).
    /// </summary>
    public static UnmanagedString Empty { get; } = default;

    /// <summary>
    /// Creates a new unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemAuto(string?)"/>.
    /// </summary>
    /// <param name="value">The managed string.</param>
    /// <returns>The unmanaged string.</returns>
    public static UnmanagedString FromAuto(string value) => new(Marshal.StringToCoTaskMemAuto(value), Marshal.PtrToStringAuto, Marshal.FreeCoTaskMem);
    /// <summary>
    /// Creates a new ANSI unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemAnsi(string?)"/>.
    /// </summary>
    /// <param name="value">The managed string.</param>
    /// <returns>The unmanaged string.</returns>
    public static UnmanagedString FromAnsi(string value) => new(Marshal.StringToCoTaskMemAnsi(value), Marshal.PtrToStringAnsi, Marshal.FreeCoTaskMem);
    /// <summary>
    /// Creates a new UTF8 unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemUTF8(string?)"/>.
    /// </summary>
    /// <param name="value">The managed string.</param>
    public static UnmanagedString FromUTF8(string value) => new(Marshal.StringToCoTaskMemUTF8(value), Marshal.PtrToStringUTF8, Marshal.FreeCoTaskMem);
    /// <summary>
    /// Creates a new Unicode unmanaged string from a managed string, allocating the memory using <see cref="Marshal.StringToCoTaskMemUni(string?)"/>.
    /// </summary>
    /// <param name="value">The managed string.</param>
    /// <returns>The unmanaged string.</returns>
    public static UnmanagedString FromUnicode(string value) => new(Marshal.StringToCoTaskMemUni(value), Marshal.PtrToStringUni, Marshal.FreeCoTaskMem);

    /// <summary>
    /// Creates a new empty unmanaged string of the specified length, allocating the memory using <see cref="Marshal.StringToCoTaskMemAuto(string?)"/>.
    /// </summary>
    /// <param name="length">The length of the string in bytes.</param>
    /// <returns>The unmanaged string.</returns>
    /// <remarks>
    /// Please note that as the length is given in bytes it might not match the actual length of the string (which might be encoded with Unicode), you have to calculate this by yourself.
    /// </remarks>
    public static UnmanagedString FromAuto(int length) => new(length, Marshal.PtrToStringAuto);
    /// <summary>
    /// Creates a new empty ANSI unmanaged string of the specified length, allocating the memory using <see cref="Marshal.StringToCoTaskMemAnsi(string?)"/>.
    /// </summary>
    /// <param name="length">The length of the string in bytes.</param>
    /// <returns>The unmanaged string.</returns>
    public static UnmanagedString FromAnsi(int length) => new(length, Marshal.PtrToStringAnsi);
    /// <summary>
    /// Creates a new empty UTF8 unmanaged string of the specified length, allocating the memory using <see cref="Marshal.StringToCoTaskMemUTF8(string?)"/>.
    /// </summary>
    /// <param name="length">The length of the string in bytes.</param>
    /// <returns>The unmanaged string.</returns>
    /// <remarks>
    /// Please note that as the length is given in bytes it might not match the actual length of the string, since with UTF8 characters are between 1 and 4 bytes, you have to calculate this by yourself.
    /// </remarks>
    public static UnmanagedString FromUTF8(int length) => new(length, Marshal.PtrToStringUTF8);
    /// <summary>
    /// Creates a new empty Unicode unmanaged string of the specified length, allocating the memory using <see cref="Marshal.StringToCoTaskMemUni(string?)"/>.
    /// </summary>
    /// <param name="length">The length of the string in bytes.</param>
    /// <returns>The unmanaged string.</returns>
    /// <remarks>
    /// Please note that as the length is given in bytes it might not match the actual length of the string, since with UTF16 characters are either 2 or 4 bytes, you have to calculate this by yourself.
    /// </remarks>
    public static UnmanagedString FromUnicode(int length) => new(length, Marshal.PtrToStringUni);

    private readonly Func<nint, string?> m_Marshaller;
    private readonly Action<nint>? m_FreeCallback;

    /// <summary>
    /// The pointer that points to the unmanaged string.
    /// </summary>
    public nint Pointer { get; private set; }

    /// <summary>
    /// Create a new unmanaged string from an existing pointer with a custom free and marshaller function.
    /// </summary>
    /// <param name="pointer">The pointer that points to the unmanaged string.</param>
    /// <param name="marshaller">The function used to convert the unmanaged string into a managed string when <see cref="ToString"/> is called.</param>
    /// <param name="free">The function called when the unmanaged string is freed.</param>
    public UnmanagedString(nint pointer, Func<nint, string?> marshaller, Action<nint>? free = null)
    {
        Pointer = pointer;
        m_Marshaller = marshaller;
        m_FreeCallback = free;
    }

    /// <summary>
    /// Creates a new empty unmanaged string of the specified length, with a custom marshaller.
    /// </summary>
    /// <param name="length">The length of the allocated buffer.</param>
    /// <param name="marshaller">The function used to convert the unmanaged string into a managed string when <see cref="ToString"/> is called.</param>
    public UnmanagedString(int length, Func<nint, string?> marshaller) : this(Marshal.AllocCoTaskMem(length), marshaller, Marshal.FreeCoTaskMem)
    {
        Unsafe.InitBlock((void*)Pointer, 0, (uint)length);
    }

    /// <summary>
    /// Free the unmanaged string. The <see cref="Pointer"/> is set to <see cref="nint.Zero"/> and the <see cref="Value"/> is set to <see cref="string.Empty"/>.
    /// </summary>
    public void Free()
    {
        m_FreeCallback?.Invoke(Pointer);
        Pointer = nint.Zero;
    }

    public override readonly string ToString()
    {
        if (Pointer != nint.Zero) return string.Empty;
        return m_Marshaller.Invoke(Pointer) ?? string.Empty;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is UnmanagedString other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Pointer);
    }

    public readonly bool Equals(UnmanagedString other)
    {
        return Pointer == other.Pointer;
    }

    public static bool operator ==(UnmanagedString left, UnmanagedString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnmanagedString left, UnmanagedString right)
    {
        return left.Equals(right);
    }

    public static implicit operator string(UnmanagedString value) => value.ToString();
}