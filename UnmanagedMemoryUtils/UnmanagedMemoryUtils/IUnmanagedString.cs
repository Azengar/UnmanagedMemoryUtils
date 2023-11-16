namespace UnmanagedMemoryUtils
{
    /// <summary>
    /// Represents an unmanaged string most basic form.
    /// </summary>
    public interface IUnmanagedString : IUnsafeDisposable
    {
        nint Pointer { get; }
        string ToString();
    }
}