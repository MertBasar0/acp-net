# Release Checklist

Last updated: 2026-06-10

Use this before any alpha NuGet publication.

## Current Publication Decision

Do not publish to NuGet yet without an explicit publish command from the project owner.

The current repository can produce local alpha packages, and Apache-2.0 is selected. Public NuGet publication still needs final package ID availability confirmation, a NuGet API key, and generated package metadata recheck after the final release commit.

## Required Before Alpha

- Public API names reviewed.
- Package descriptions reviewed.
- License selected by project owner.
- Package ID availability checked immediately before publish.
- README examples tested from a fresh clone.
- `dotnet test` passes.
- `dotnet pack` passes for `Acp.Net.Process`.
- `dotnet pack` passes for `Acp.Net.Testing`.
- Generated packages inspected locally.
- Symbol packages inspected locally.
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
- The package contains generated artifacts or local transcripts.
- Diagnostics is packaged before the command contract is stable.

## Current Metadata State

- `RepositoryUrl`: `https://github.com/MertBasar0/acp-net`
- `PackageProjectUrl`: `https://github.com/MertBasar0/acp-net`
- `RepositoryType`: `git`
- `PackageLicenseExpression`: `Apache-2.0`
- `SymbolPackageFormat`: `snupkg`
- Published packages: none

## Package ID Check

Checked on 2026-06-10 through NuGet flat-container API:

- `https://api.nuget.org/v3-flatcontainer/acp.net.process/index.json`: 404
- `https://api.nuget.org/v3-flatcontainer/acp.net.testing/index.json`: 404

Interpretation: these package IDs were not published at check time. Recheck immediately before `dotnet nuget push`.
