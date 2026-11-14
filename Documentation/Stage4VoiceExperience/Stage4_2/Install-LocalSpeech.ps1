param(
    [Parameter(Mandatory = $true)][string] $Destination,
    [Parameter(Mandatory = $true)][string] $DownloadCache
)

$ErrorActionPreference = 'Stop'
$target = [IO.Path]::GetFullPath($Destination)
$cache = [IO.Path]::GetFullPath($DownloadCache)
if ((Split-Path -Leaf $target) -ne 'VoiceLocal') { throw 'Destination must be named VoiceLocal.' }
if (Test-Path -LiteralPath $target) { throw 'Choose a new VoiceLocal folder. Existing installations are preserved.' }
New-Item -ItemType Directory -Force $cache | Out-Null

function Get-PinnedFile([string]$Filename, [string]$Uri, [string]$Hash) {
    $path = Join-Path $cache $Filename
    if (!(Test-Path -LiteralPath $path)) {
        $partial = "$path.partial"
        & curl.exe --fail --location --silent --show-error --retry 2 --max-time 480 --output $partial $Uri
        if ($LASTEXITCODE -ne 0) { throw "Download failed for $Filename" }
        if ((Get-FileHash -LiteralPath $partial -Algorithm SHA256).Hash -ne $Hash) {
            throw "SHA-256 mismatch for $Filename"
        }
        Move-Item -LiteralPath $partial -Destination $path
    }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $Hash) {
        throw "SHA-256 mismatch for existing $Filename. Use a fresh cache."
    }
    return $path
}

$whisper = 'https://github.com/ggml-org/whisper.cpp/releases/download/b5454'
$nvidia = 'https://developer.download.nvidia.com/compute/cuda/redist'
$weights = 'https://huggingface.co/ggerganov/whisper.cpp/resolve/5359861c739e955e79d9a303bcbc70fb988958b1'
$vad = 'https://huggingface.co/ggml-org/whisper-vad/resolve/9ffd54a1e1ee413ddf265af9913beaf518d1639b'
$opencc = 'https://raw.githubusercontent.com/BYVoid/OpenCC/556ed22496d650bd0b13b6c163be9814637970ae'
$engine = Get-PinnedFile 'cuda.zip' "$whisper/whisper-bin-win-cuda-11.8.0-x64.zip" 'e81c86c542be6e3d7b7245392d4e1c52ab6e5c61dbe38721754bc0ab59d4d027'
$blas = Get-PinnedFile 'cublas.zip' "$nvidia/libcublas/windows-x86_64/libcublas-windows-x86_64-11.11.3.6-archive.zip" '67b0934a6359e4ee26fff823c356021589d392c4fd49ca12624f570edc08e2b9'
$model = Get-PinnedFile 'ggml-large-v3-turbo-q5_0.bin' "$weights/ggml-large-v3-turbo-q5_0.bin" '394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2'
$detector = Get-PinnedFile 'ggml-silero-v6.2.0.bin' "$vad/ggml-silero-v6.2.0.bin" '2aa269b785eeb53a82983a20501ddf7c1d9c48e33ab63a41391ac6c9f7fb6987'
$characters = Get-PinnedFile 'STCharacters.txt' "$opencc/data/dictionary/STCharacters.txt" 'ed1d268e0ad028511dcf5b0089faed0a980ad332449ec11d481ceefde6879f41'
$phrases = Get-PinnedFile 'STPhrases.txt' "$opencc/data/dictionary/STPhrases.txt" '17fece21a28f3db2dc32397abd73d21ae73261c3f295fa6a3fe64b5e8d8b554b'

$staging = Join-Path (Split-Path -Parent $target) ('VoiceLocal-preparing-' + [guid]::NewGuid().ToString('N'))
$bin = Join-Path $staging 'bin'
$models = Join-Path $staging 'models'
$script = Join-Path $staging 'script'
$licenses = Join-Path $staging 'licenses'
New-Item -ItemType Directory -Force $bin,$models,$script,$licenses | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Copy-ArchiveFiles([string]$Archive, [string]$Pattern, [string]$Folder) {
    $zip = [IO.Compression.ZipFile]::OpenRead($Archive)
    try {
        foreach ($entry in $zip.Entries) {
            if ($entry.Name -match $Pattern) {
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $Folder $entry.Name), $false)
            }
        }
    }
    finally { $zip.Dispose() }
}

Copy-ArchiveFiles $engine '^(whisper-cli\.exe|whisper\.dll|ggml(?:-base|-cuda|-cpu-[a-z0-9-]+)?\.dll|cudart64_110\.dll)$' $bin
Copy-ArchiveFiles $blas '^(cublas64_11|cublasLt64_11)\.dll$' $bin
Copy-ArchiveFiles $blas '^LICENSE$' $licenses
Copy-Item -LiteralPath $model,$detector -Destination $models
Copy-Item -LiteralPath $characters,$phrases -Destination $script

$notices = @{
    'whisper.cpp-MIT.txt' = 'https://raw.githubusercontent.com/ggml-org/whisper.cpp/d1be6fde11ac6e0407606b4e42fe72d34add8037/LICENSE'
    'OpenAI-Whisper-MIT.txt' = 'https://raw.githubusercontent.com/openai/whisper/86098128c0b4f24f0e2aa2994de830614b474227/LICENSE'
    'Silero-MIT.txt' = 'https://raw.githubusercontent.com/snakers4/silero-vad/be95df9152c0d7618fa1edfeb296fc3dae32376f/LICENSE'
    'OpenCC-Apache-2.0.txt' = "$opencc/LICENSE"
}
foreach ($entry in $notices.GetEnumerator()) {
    & curl.exe --fail --location --silent --show-error --max-time 30 --output (Join-Path $licenses $entry.Key) $entry.Value
    if ($LASTEXITCODE -ne 0) { throw "Could not retrieve $($entry.Key). Incomplete installation remains in $staging." }
}
Move-Item -LiteralPath $staging -Destination $target
Get-ChildItem -LiteralPath $target -Recurse -File | Measure-Object Length -Sum
