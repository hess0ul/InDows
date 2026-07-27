namespace InDows.Core.Build;

/// <summary>Provides the anchored InDows base autounattend.xml the Build screen grafts modules onto. A seam,
/// so generation is testable off a fixed base.</summary>
public interface IBaseTemplate
{
    string Read();
}

/// <summary>Reads the base from a file (the copy bundled next to the app).</summary>
public sealed class FileBaseTemplate(string path) : IBaseTemplate
{
    public string Read()
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"The base autounattend.xml is missing: '{path}'.");
        }

        return File.ReadAllText(path);
    }
}
