# UnmanagedMemoryUtils
Hosted on github: https://github.com/Azengar/UnmanagedMemoryUtils.

A .NET collection of utilities for working with unmanaged memory.

Provides faster alternatives to `Memory<T>` and `ReadOnlyMemory<T>` in addition to accessing unmanaged string from managed code.

Allows to manipulate array of unmanaged pointers with the `UnmanagedArrayPointer<T>` struct.

## Changelog

### Version 1.0.2

* Changed from `Unsafe.CopyBlock` to `Unsafe.CopyBlockUnaligned`.

### Version 1.0.3

* Added Aggressive Inlining for Allocate methods.

### Version 1.0.4

* Added the `UnmanagedPointerArray<T>` struct used to manipulate array of pointers.
* Marked all properties as `readonly` as they don't modify the state of the structs.
* Use the new `namespace` syntax for less indentation levels.
