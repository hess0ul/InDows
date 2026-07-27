using ICSharpCode.SharpZipLib.Zip;

namespace InDows.Providers.Windows.Restore;

/// <summary>
/// Reads the encrypted secrets vault, a standard WinZip AES-256 zip, with the passphrase and hands each
/// secret (its stored relative name and a decrypting stream) to the caller. Same format ReDows writes, so
/// InDows opens ReDows's vaults directly. Read-only; throws if the passphrase is wrong or an entry is corrupt.
/// </summary>
public static class VaultReader
{
    public static void ExtractEach(string vaultPath, string passphrase, Action<string, Stream> onEntry)
    {
        using var zip = new ZipFile(vaultPath) { Password = passphrase };
        foreach (ZipEntry entry in zip)
        {
            if (!entry.IsFile)
            {
                continue;
            }

            using var stream = zip.GetInputStream(entry);
            onEntry(entry.Name, stream);
        }
    }
}
