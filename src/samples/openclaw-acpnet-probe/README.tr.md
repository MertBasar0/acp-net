# OpenClaw Acp.Net Probe Komutu

🇬🇧 English version: [README.md](README.md)

Bu sample, stabilize edilmiş tanılama komut probe'udur.

Bir OpenClaw plugin/araç wrapper'ının çalıştırabileceği bir komut gibi davranır:

1. `AcpProcessRunner` ile deterministik bir ACP subagent başlatır,
2. kritik/opsiyonel preflight politikasını uygular,
3. bir ACP prompt'u çalıştırır,
4. Acp.Net transcript'ini ve run artifact'ini yazar,
5. stdout'a tek bir OpenClaw odaklı JSON sonucu basar.

Varsayılan olarak herhangi bir LLM veya model API'si çağırmaz.

Çalıştırma:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Yararlı seçenekler:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent my-agent \
  --cwd /path/to/workspace \
  --required-tool git \
  --optional-tool rg \
  --artifact-dir artifacts/openclaw-probe
```

Özel ACP agent komutu:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent gemini \
  --command /path/to/gemini \
  --arg --acp \
  --arg --skip-trust \
  --required-tool git \
  --optional-tool rg
```

Exit code'lar:

- `0`: `ok=true`
- `2`: environment/preflight hatası
- `3`: runtime/protokol/agent/bilinmeyen hata
- `64`: geçersiz CLI konfigürasyonu

Stdout, tek bir JSON sonucu için ayrılmıştır. Yardım metni ve tanılama mesajları stderr'e gider.
