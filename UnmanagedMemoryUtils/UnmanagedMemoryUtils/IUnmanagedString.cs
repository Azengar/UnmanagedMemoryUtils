namespace UnmanagedMemoryUtils
{
    /// <summary>
    /// Represents an unmanaged string most basic form.
    /// </summary>
    public interface IUnmanagedString
    {
        nint Pointer { get; }
        string ToString();
    }
}