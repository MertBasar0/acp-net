# Spike 007 Sonuc Raporu: Runtime Environment Shaping

Tarih: 2026-06-07

## Ozet

Spike basarili.

Gemini dogfood sirasinda gorulen `Ripgrep is not available. Falling back to GrepTool.` uyarisi urun kabiliyeti olarak ele alindi.

Acp.Net.Process artik agent process baslatmadan once runtime environment'i sekillendirebilir ve gerekli executable'lari preflight edebilir.

## Eklenen API

`AcpProcessOptions` icine eklendi:

```csharp
public IReadOnlyDictionary<string, string?> Environment { get; init; }
public IReadOnlyList<string> AdditionalPathEntries { get; init; }
public IReadOnlyList<string> RequiredExecutables { get; init; }
```

Anlamlari:

- `Environment`: agent process environment variable set/remove.
- `AdditionalPathEntries`: agent process PATH basina eklenecek dizinler.
- `RequiredExecutables`: agent baslamadan once runtime icinde aranacak araclar.

Transcript event'leri:

- `preflight.tool.found`
- `preflight.tool.missing`

## Gemini Dogfood Dogrulamasi

Gemini sample:

`src/samples/acp-process-with-gemini/`

Sample config:

```csharp
RequiredExecutables = ["rg", "git", "node"]
```

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\acp-process-with-gemini\acp-process-with-gemini.csproj'
```

Cikti:

```text
protocol=1
usesWsl=True
session=0fdbaaf5-70ae-4696-9b5b-9b0b96b9f6a1
agentCwd=/home/mertb/.openclaw/workspace/acp-net-training-factory/src/samples/acp-process-with-gemini
ACP-DOGFOOD-OK
stopReason=EndTurn
chunks=1
```

Transcript preflight sonucu:

```text
preflight.tool.missing rg which exited 1
preflight.tool.found git /usr/bin/git
preflight.tool.found node /usr/bin/node
```

Gemini stderr sonucu:

```text
Ripgrep is not available. Falling back to GrepTool.
```

Bu, preflight'in gercek sorunu agent calismadan once gorunur hale getirdigini kanitliyor.

## Test Dogrulamasi

Komut:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Cikti ozeti:

```text
Acp.Net.UnitTests: 13 passed
Acp.Net.IntegrationTests: 2 passed
```

## Pack Dogrulamasi

Iki paket tekrar pack edildi:

- `Acp.Net.Process.0.1.0-alpha.1.nupkg`
- `Acp.Net.Testing.0.1.0-alpha.1.nupkg`

## Karar Etkisi

Bu spike Acp.Net.Process'in urun iddiasini guclendirdi.

Runner artik sadece process baslatmiyor:

- environment variable set/remove yapabiliyor
- PATH'i runtime'a gore sekillendirebiliyor
- required executable preflight yapabiliyor
- eksikleri transcript'e yazabiliyor

Bir sonraki teknik soru:

- Eksik executable icin sadece uyarı mı verilmeli, yoksa opsiyonel fail-fast policy mi olmali?
- `AdditionalPathEntries` ile Codex vendored `rg` gibi path'ler WSL runtime'a guvenli aktarilmali mi?
- Yoksa kullaniciya sistem-level `sudo apt install ripgrep` onerisi mi dokumante edilmeli?
