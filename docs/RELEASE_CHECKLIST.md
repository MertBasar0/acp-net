# Release Checklist

Last updated: 2026-06-10

Use this before any alpha NuGet publication.

## Current Publication Decision

Do not publish to NuGet yet.

The current repository can produce local alpha packages, but public NuGet publication is blocked until the project owner selects a license and the generated package metadata is rechecked after the final release commit.

## Required Before Alpha

- Public API names reviewed.
- Package descriptions reviewed.
- License selected by project owner.
- README examples tested from a fresh clone.
- `dotnet test` passes.
- `dotnet pack` passes for `Acp.Net.Process`.
- `dotnet pack` passes for `Acp.Net.Testing`.
- Generated packages inspected locally.
- CI passes.
- Repository visibility intentionally set for the release phase.

## Verification Commands

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Process\Acp.Net.Process.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Testing\Acp.Net.Testing.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

## Do Not Publish If

- Any public API still feels sample-shaped.
- OpenClaw diagnostic behavior is described as core integration.
- The package identity is still unclear against `AgentClientProtocol`.
- License is undecided.
- The package contains generated artifacts or local transcripts.
- Diagnostics is packaged before the command contract is stable.

## Current Metadata State

- `RepositoryUrl`: `https://github.com/MertBasar0/acp-net`
- `PackageProjectUrl`: `https://github.com/MertBasar0/acp-net`
- `RepositoryType`: `git`
- `PackageLicenseExpression`: intentionally not set yet
- Published packages: none
