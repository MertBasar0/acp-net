# OpenClaw Subagent Runner Sample'ı

🇬🇧 English version: [README.md](README.md)

Bu sample, en küçük yararlı OpenClaw entegrasyon şeklini simüle eder:

1. ACP uyumlu bir subagent process'i başlatır.
2. Gerekli runtime araçları için preflight yapar.
3. Bir ACP prompt'u gönderir.
4. Bir transcript ve makine tarafından okunabilir bir run artifact'i kaydeder.
5. Çağırana kompakt bir sonuç nesnesi döndürür.

Sample'ın deterministik olması ve model kotası harcamaması için gerçek bir LLM agent'ı yerine bilinçli olarak `FakeAcpAgentScript` kullanır.

Çalıştırma:

```bash
dotnet run --project src/samples/openclaw-subagent-runner/openclaw-subagent-runner.csproj
```
