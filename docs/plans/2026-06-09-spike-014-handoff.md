# 2026-06-09 Spike 014 Handoff

## Tamamlanan Is

Spike 014 tamamlandi:

**OpenClaw Doctor Adapter Draft**

## Eklenenler

Contract dokumani:

`docs/contracts/openclaw-doctor-adapter-draft.md`

Scenario fixture:

`docs/contracts/openclaw-doctor-mapping.scenarios.json`

Executable draft:

`src/openclaw-probe/doctor-adapter-draft.mjs`

Verifier:

`src/openclaw-probe/verify-doctor-adapter-draft.mjs`

## Mapping Karari

Acp.Net diagnostic probe sonucu iki OpenClaw yuzeyine map edilebilir:

1. `AcpRuntimeDoctorReport`
2. `HealthFinding[]`

Kritik mapping:

- `ok=true`, warning yok -> doctor ok, finding yok
- `ok=true`, optional missing tool -> doctor ok + warning code, lint warning
- `EnvironmentFailure` -> doctor not ok, lint error
- `ConfigurationFailure` -> doctor not ok, lint error
- diger failure -> generic probe failed

## Son Dogrulamalar

Adapter verifier:

```text
doctor adapter scenarios ok (4)
```

Ana test suite:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Pack:

```text
Acp.Net.Process: ok
Acp.Net.Testing: ok
```

## Sonraki Oneri

Spike 015:

**OpenClaw Doctor Adapter Implementation Probe**

Bu spike'ta gercek OpenClaw kaynak agacina girilip girilmeyecegine karar verilmeli. Girilecekse once fake JSON fixture ile bir `HealthCheck` adapter test'i yazilmali; Acp.Net command'i calistirma konusu ikinci adim olmali.

