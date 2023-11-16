namespace UnmanagedMemoryUtils;

/// <summary>
/// Represents an object that contains native resources which should be freed, using the <see cref="Free"/> method.
/// </summary>
/// <remarks>
/// This interface differs of the .NET <see cref="IDisposable"/> implementation in that it is not expected 
/// to be used alongside a finalizer to avoid the performance penalty, it is your responsibility to call <see cref="Free"/> 
/// and you will face memory leaks if you don't do so consistently. So it is a tradeoff of adding more responsibility on the developer in favor of performance.
/// </remarks>
public interface IUnsafeDisposable
{
    /// <summary>
    /// Free the native resources used as part of this object. 
    /// It is not neceessary to free managed resources altough it can also be done if so wished.
    /// </summary>
    void Free();
}
