# 2026-06-09 Spike 012 Handoff

## Tamamlanan Is

Spike 012 tamamlandi:

**OpenClaw ACPX Contract Comparison**

## Okunan Ana OpenClaw Dosyalari

- `packages/acp-core/src/runtime/types.ts`
- `packages/acp-core/src/runtime/errors.ts`
- `extensions/acpx/runtime-api.ts`
- `extensions/acpx/src/runtime.ts`
- `extensions/acpx/src/process-lease.ts`
- `extensions/acpx/src/state.ts`

## Karar

Acp.Net, OpenClaw ACPX runtime backend'inin yerine gecmemeli.

En dogru kisa vadeli rol:

> external diagnostic command + run artifact/failure contract

En dogru orta vadeli rol:

> ACPX'e diagnostic/preflight/evidence katkisi veya test harness destegi

## Eklenen Fixture'lar

- `docs/contracts/acpnet-run-artifact.example.json`
- `docs/contracts/openclaw-acpnet-probe-result.example.json`

## Sonraki Oneri

Spike 013:

**Acp.Net Diagnostic Command Stabilization**

Bu spike'ta `openclaw-acpnet-probe` sample'i gercek CLI gibi davranmali:

- argumanlarla agent/cwd/tool policy almak,
- stdout'a sadece tek JSON result basmak,
- stderr'i diagnostic icin kullanmak,
- exit code sozlesmesini netlestirmek.

Bu adim OpenClaw core'a girmeden once gerekli.

