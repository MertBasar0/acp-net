# Development Guide

Last updated: 2026-06-10

## Requirements

Current development has been verified on Windows + WSL.

Important practical detail:

The local `dotnet` command in WSL may call Windows `dotnet.exe`. When that happens, pass WSL files to `dotnet` through UNC paths:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Using `/home/.../AcpNetMvp.slnx` can make Windows MSBuild interpret the path as an invalid switch.

## Common Commands

Run tests:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Pack:

```bash
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Process\Acp.Net.Process.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Testing\Acp.Net.Testing.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
```

Run diagnostic probe with fake agent:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj'
```

Run doctor adapter draft verifier:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

## Generated Files

Ignored by git:

- `bin/`
- `obj/`
- `artifacts/`
- `*.ndjson`

Do not commit local transcripts, package outputs, or generated run artifacts unless a specific fixture is intentionally placed under `docs/contracts`.

## Coding Principles

- Keep protocol schema ownership out of Acp.Net unless absolutely necessary.
- Prefer interop with `AgentClientProtocol` for protocol-level behavior.
- Keep production runtime and testing helpers separate.
- Keep diagnostic CLI/tooling optional until usage is proven.
- Add features from failing tests or concrete dogfood evidence.

