# UnmanagedMemoryUtils
Hosted on github: https://github.com/Azengar/UnmanagedMemoryUtils.

Available on Nuget at: https://www.nuget.org/packages/UnmanagedMemoryUtils/ (UnmanagedMemoryUtils)

A .NET collection of utilities for working with unmanaged memory.

Provides faster alternatives to `Memory<T>` and `ReadOnlyMemory<T>` in addition to accessing unmanaged string from managed code.

Allows to manipulate array of unmanaged pointers with the `UnmanagedArrayPointer<T>` struct.

## Making a release

* Generate new commit with your changes with a message: `git commit -m "Version X.X.X: Notes about the change"`
* Tag the new commit: `git tag vX.X.X -m "Version X.X.X"`
* Push the commit: `git push` and the tag `git push origin vX.X.X`
* Build the new `Release` binaries. The package is built automatically alongside the binaries.
* Navigate to `./UnmanagedMemoryUtils/UnmanagedMemoryUtils/bin/Release`
* Publish the package: `dotnet nuget push .\UnmanagedMemoryUtils.X.X.X.nupkg --api-key <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json`

## Changelog

### Version 1.0.2

* Changed from `Unsafe.CopyBlock` to `Unsafe.CopyBlockUnaligned`.

### Version 1.0.3

* Added Aggressive Inlining for Allocate methods.

### Version 1.0.4

* Added the `UnmanagedPointerArray<T>` struct used to manipulate array of pointers.
* Marked all properties as `readonly` as they don't modify the state of the structs.
* Use the new `namespace` syntax for less indentation levels.

### Version 1.0.5

* Fixed an issue that made it impossible to retrieve the string value of an `UnmanagedString` with the `ToString` method.

### Version 1.0.6

* Readme update with Nuget link.

### Version 1.1.0

* Update to .NET 8
* Adds a new `IUnsafeDisposable` interface which offers an unsafe but less performance impacting to the standard `IDisposable` and finalizer pattern.
	* This interface is now implemented by `IUnmanagedString` and is available to use outside the library.