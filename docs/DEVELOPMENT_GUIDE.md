# Development Guide

> 🇹🇷 Türkçe sürüm: [DEVELOPMENT_GUIDE.tr.md](DEVELOPMENT_GUIDE.tr.md)

Last updated: 2026-06-10

## Requirements

- .NET 8 SDK
- Node.js (only for the OpenClaw doctor adapter draft verifier)
- `python3` available in the agent runtime (the fake ACP agent and the default probe use it)

Development has been verified on Windows + WSL and the same commands work on a plain Linux setup.

## Windows + WSL Path Note

On some Windows + WSL setups, the `dotnet` command available inside WSL is actually the Windows `dotnet.exe`. Windows MSBuild cannot interpret Linux-style absolute paths such as `/home/<user>/...` and may treat them as invalid switches.

When that happens, pass WSL files to `dotnet` through UNC paths:

```bash
dotnet test '\\wsl.localhost\<Distro>\<path-to-repo>\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Replace `<Distro>` with your WSL distribution name (for example `Ubuntu`) and `<path-to-repo>` with the repository location inside WSL. Alternatively, install the native Linux .NET SDK inside WSL and use plain relative paths.

All commands below assume they are run from the repository root with a `dotnet` that understands the repository path.

## Common Commands

Run tests:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
```

Pack:

```bash
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
```

Run the diagnostic probe with the fake agent:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Run the doctor adapter draft verifier:

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

## Documentation Conventions

- English is the primary documentation language.
- Every core document has a full Turkish version next to it with a `.tr.md` suffix; keep both in sync when editing.
- Do not put machine-specific absolute paths into documentation; use repository-relative paths and placeholders like `<Distro>`.
- Dated working notes (spike session reports, daily handoffs) do not belong in the git history; they live in the untracked `notes/` folder at the repository root. Record durable outcomes as ADRs under `docs/decisions/`.
- `docs/CURRENT_STATUS.md` is the single source of truth for project state; link to it instead of duplicating status or test counts.

## Coding Principles

- Keep protocol schema ownership out of Acp.Net unless absolutely necessary.
- Prefer interop with `AgentClientProtocol` for protocol-level behavior.
- Keep production runtime and testing helpers separate.
- Keep diagnostic CLI/tooling optional until usage is proven.
- Add features from failing tests or concrete dogfood evidence.
