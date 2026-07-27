using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ICSharpCode.SharpZipLib.Zip;
using InDows.Core.Restore;
using InDows.Providers.Windows.Restore;
using Xunit;

namespace InDows.Gui.Tests;

/// <summary>
/// The safeguard for the reimplemented engine: fabricate a backup in the exact ReDows format (drive-as-folder
/// tree, hash manifest, WinZip AES-256 vault) and prove InDows restores it correctly. All work happens under a
/// temp folder and restores in "to a folder" mode, so it never touches real system locations.
/// </summary>
public sealed class RestoreRoundTripTests : IDisposable
{
    private readonly string _root;
    private readonly string _backup;
    private readonly string _target;

    public RestoreRoundTripTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "indows-restore-test-" + Guid.NewGuid().ToString("N"));
        _backup = Path.Combine(_root, "backup");
        _target = Path.Combine(_root, "target");
        Directory.CreateDirectory(_backup);
        Directory.CreateDirectory(_target);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public async Task Restores_files_and_verifies_them_against_the_recorded_hashes()
    {
        WriteBackupFile("C/data/hello.txt", "hello");
        WriteHashes(("C/data/hello.txt", "hello"));

        var result = await new WindowsRestoreRunner().RunAsync(
            new RestoreRequest(_backup, ToOriginalLocations: false, TargetFolder: _target, VaultPassword: null),
            new Progress<RestoreProgress>(),
            CancellationToken.None);

        var restored = Path.Combine(_target, "C", "data", "hello.txt");
        Assert.True(File.Exists(restored));
        Assert.Equal("hello", File.ReadAllText(restored));
        Assert.Contains("1 files", result.RestoredText);
        Assert.Contains("verified", result.VerifiedText!);
        Assert.Equal("0", result.FailedText);
    }

    [Fact]
    public async Task Restores_the_secrets_vault_with_the_password()
    {
        WriteHashes();
        WriteVault("pw", ("C/secrets/key.txt", "s3cr3t"));

        var result = await new WindowsRestoreRunner().RunAsync(
            new RestoreRequest(_backup, ToOriginalLocations: false, TargetFolder: _target, VaultPassword: "pw"),
            new Progress<RestoreProgress>(),
            CancellationToken.None);

        var secret = Path.Combine(_target, "C", "secrets", "key.txt");
        Assert.True(File.Exists(secret));
        Assert.Equal("s3cr3t", File.ReadAllText(secret));
        Assert.Contains("secret(s) restored", result.SecretsText);
    }

    [Fact]
    public async Task Second_run_skips_files_that_already_exist()
    {
        WriteBackupFile("C/data/hello.txt", "hello");
        WriteHashes(("C/data/hello.txt", "hello"));
        var request = new RestoreRequest(_backup, ToOriginalLocations: false, TargetFolder: _target, VaultPassword: null);

        await new WindowsRestoreRunner().RunAsync(request, new Progress<RestoreProgress>(), CancellationToken.None);
        var second = await new WindowsRestoreRunner().RunAsync(request, new Progress<RestoreProgress>(), CancellationToken.None);

        Assert.Contains("1 already existed", second.SkippedText);
        Assert.Contains("0 files", second.RestoredText);
    }

    private void WriteBackupFile(string relativePath, string content)
    {
        var full = Path.Combine(_backup, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }

    private void WriteHashes(params (string Path, string Content)[] files)
    {
        var rows = files.Select(f => new
        {
            path = f.Path,
            sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(f.Content))),
        });
        var json = JsonSerializer.Serialize(new { version = 1, algorithm = "SHA-256", files = rows });
        File.WriteAllText(Path.Combine(_backup, BackupFormat.HashesFileName), json);
    }

    private void WriteVault(string password, params (string Name, string Content)[] secrets)
    {
        using var fs = File.Create(Path.Combine(_backup, BackupFormat.VaultFileName));
        using var zip = new ZipOutputStream(fs) { Password = password, IsStreamOwner = true };
        zip.SetLevel(6);
        foreach (var (name, content) in secrets)
        {
            zip.PutNextEntry(new ZipEntry(name) { AESKeySize = 256, DateTime = DateTime.UtcNow });
            var bytes = Encoding.UTF8.GetBytes(content);
            zip.Write(bytes, 0, bytes.Length);
            zip.CloseEntry();
        }
    }
}
