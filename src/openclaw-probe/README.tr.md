# OpenClaw Acp.Net Probe (Node Taslakları)

🇬🇧 English version: [README.md](README.md)

Bu klasör, OpenClaw entegrasyonu için küçük Node tarafı taslakları içerir. OpenClaw core deposunu değiştirmek yerine bilinçli olarak Acp.Net deposu içinde yaşarlar.

## Komut Wrapper Probe'u

`openclaw-acpnet-probe.mjs`, bir OpenClaw komut/araç wrapper'ının yapması gerekenleri yansıtır:

1. Acp.Net OpenClaw tarzı subagent sample'ını çalıştırır,
2. sample stdout sonucunu parse eder,
3. Acp.Net run artifact'ini okur,
4. tek bir kompakt OpenClaw odaklı JSON sonucu döndürür.

Çalıştırma:

```bash
node src/openclaw-probe/openclaw-acpnet-probe.mjs
```

Çıktı kontratı:

```json
{
  "kind": "openclaw.acpnet.probe.result",
  "ok": true,
  "result": "completed",
  "failureKind": "None",
  "agentName": "openclaw-fake-acp-subagent",
  "usesWsl": true,
  "preflight": {
    "criticalMissing": [],
    "warnings": []
  }
}
```

Bu probe, deterministik sahte ACP agent'ı kullanır. Herhangi bir LLM veya model API'si çağırmaz.

## Doctor Adapter Taslağı

`doctor-adapter-draft.mjs`, probe sonucunu [docs/contracts/openclaw-doctor-adapter-draft.tr.md](../../docs/contracts/openclaw-doctor-adapter-draft.tr.md) dosyasında tanımlanan OpenClaw doctor/lint yüzeylerine eşler.

Senaryo fixture'larına karşı doğrulayın:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Beklenen:

```text
doctor adapter scenarios ok (4)
```
