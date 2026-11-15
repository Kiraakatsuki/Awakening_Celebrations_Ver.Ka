using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

internal sealed class WindowsSouvenirSigner
{
    internal const string Origin = "https://awakening-souvenir.awakening-celebrations-verka.workers.dev";
    internal static readonly string ProfileDirectory = Path.Combine(
        Environment.GetEnvironmentVariable("LOCALAPPDATA") ??
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AwakeningCelebrations", "SouvenirKiosk");
    const string ProviderName = "Microsoft Platform Crypto Provider";
    const int Silent = 0x40;

    [Serializable] sealed class Enrollment
    {
        public string kioskId, keyName, provider, origin;
    }

    readonly Enrollment enrollment;
    public string KioskId => enrollment.kioskId;

    public WindowsSouvenirSigner()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
            throw new PlatformNotSupportedException();
        string path = Path.Combine(ProfileDirectory, "enrollment.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Kiosk enrollment is unavailable.");
        enrollment = JsonUtility.FromJson<Enrollment>(File.ReadAllText(path));
        if (enrollment == null || enrollment.origin != Origin || enrollment.provider != ProviderName ||
            !Regex.IsMatch(enrollment.kioskId ?? "", @"\Aawakening-win-[0-9a-f]{32}\z") ||
            enrollment.keyName != enrollment.kioskId)
            throw new InvalidDataException("Invalid kiosk enrollment.");
    }

    public string Sign(string challengeId, long expiry, string message)
    {
        var canonical = FormattableString.Invariant($"awakening-souvenir/kiosk/v1\n{Origin}\n{KioskId}\n{challengeId}\n{expiry}");
        if (!Regex.IsMatch(challengeId ?? "", @"\A[A-Za-z0-9_-]{43}\z") || canonical != message)
            throw new InvalidDataException("Invalid kiosk challenge.");

        Check(NCryptOpenStorageProvider(out var provider, ProviderName, 0));
        try
        {
            Check(NCryptGetProperty(provider, "Impl Type", out int kind, 4, out int size, 0));
            if (size != 4 || (kind & 1) == 0) throw new CryptographicException("Hardware key provider required.");
            Check(NCryptOpenKey(provider, out var key, enrollment.keyName, 0, Silent));
            try
            {
                Check(NCryptGetProperty(key, "Export Policy", out int policy, 4, out size, 0));
                if (size != 4 || policy != 0) throw new CryptographicException("Non-exportable kiosk key required.");
                using var sha = SHA256.Create();
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                var signature = new byte[64];
                Check(NCryptSignHash(key, IntPtr.Zero, digest, digest.Length, signature, signature.Length,
                    out int written, Silent));
                if (written != signature.Length) throw new CryptographicException("P-256 kiosk key required.");
                return Convert.ToBase64String(signature).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
            finally { NCryptFreeObject(key); }
        }
        finally { NCryptFreeObject(provider); }
    }

    static void Check(int status)
    {
        if (status != 0) throw new CryptographicException(status);
    }

    [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
    static extern int NCryptOpenStorageProvider(out IntPtr provider, string name, int flags);
    [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
    static extern int NCryptOpenKey(IntPtr provider, out IntPtr key, string name, int legacySpec, int flags);
    [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
    static extern int NCryptGetProperty(IntPtr handle, string name, out int value, int length, out int size, int flags);
    [DllImport("ncrypt.dll")]
    static extern int NCryptSignHash(IntPtr key, IntPtr padding, byte[] digest, int digestLength,
        [Out] byte[] signature, int signatureLength, out int resultLength, int flags);
    [DllImport("ncrypt.dll")]
    static extern int NCryptFreeObject(IntPtr handle);
}
