namespace Dotori.PackageManager;

/// <summary>Exception thrown by the package manager subsystem.</summary>
public sealed class PackageManagerException(string message, Exception? inner = null)
    : Exception(message, inner);
