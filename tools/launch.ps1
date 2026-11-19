param([string]$Action = 'launch')
$root = Split-Path -Parent $PSScriptRoot
$app = (Get-ChildItem -LiteralPath $root -Filter '*.exe' -File | Select-Object -First 1).FullName
$log = Join-Path $env:APPDATA 'dusk.zmBox\Local Store\log.txt'
$coinExe = Join-Path $PSScriptRoot 'cornercoin.exe'
$fpsExe = Join-Path $PSScriptRoot 'fpslabel.exe'

if (-not $app -or -not (Test-Path -LiteralPath $app)) {
    Write-Host 'App exe not found.'
    exit 1
}

function Start-Coin {
    if (-not (Get-Process cornercoin -ErrorAction SilentlyContinue)) {
        if (Test-Path -LiteralPath $coinExe) { Start-Process -FilePath $coinExe }
    }
    if (-not (Get-Process fpslabel -ErrorAction SilentlyContinue)) {
        if (Test-Path -LiteralPath $fpsExe) { Start-Process -FilePath $fpsExe }
    }
}

if ($Action -eq 'normal') {
    Write-Host 'Starting normally...'
    Start-Process -FilePath $app
    Start-Sleep -Seconds 2
    Start-Coin
    exit 0
}

if ($Action -eq 'toggle') {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public class HotkeySend {
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, IntPtr extra);
}
'@
    [HotkeySend]::keybd_event(0x11,0,0,[IntPtr]::Zero)
    [HotkeySend]::keybd_event(0x12,0,0,[IntPtr]::Zero)
    [HotkeySend]::keybd_event(0x53,0,0,[IntPtr]::Zero)
    Start-Sleep -Milliseconds 120
    [HotkeySend]::keybd_event(0x53,0,2,[IntPtr]::Zero)
    [HotkeySend]::keybd_event(0x12,0,2,[IntPtr]::Zero)
    [HotkeySend]::keybd_event(0x11,0,2,[IntPtr]::Zero)
    Write-Host 'Ctrl+Alt+S sent'
    exit 0
}

$token = $null
if (Test-Path $log) {
    $lines = Get-Content $log -Encoding UTF8
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        $m = [regex]::Match($lines[$i], '\u83b7\u53d6token\u6210\u529f:([^\s]+)')
        if ($m.Success) { $token = $m.Groups[1].Value.Trim(); break }
    }
}
if ($token -and ($token.Split('|').Count -ge 5)) {
    Write-Host 'Using cached token, entering game directly...'
    Start-Process -FilePath $app -ArgumentList @("auth=$token", 'ver=4')
} else {
    Write-Host 'No cached token, starting normally...'
    Start-Process -FilePath $app
}
Start-Sleep -Seconds 2
Start-Coin
