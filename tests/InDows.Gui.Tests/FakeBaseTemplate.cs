using InDows.Core.Build;

namespace InDows.Gui.Tests;

/// <summary>A base template that returns a fixed XML string.</summary>
internal sealed class FakeBaseTemplate(string xml) : IBaseTemplate
{
    public string Read() => xml;
}
