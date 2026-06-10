# Release Checklist

> 🇹🇷 Türkçe sürüm: [RELEASE_CHECKLIST.tr.md](RELEASE_CHECKLIST.tr.md)

Last updated: 2026-06-10

Use this before any alpha NuGet publication. For current project state see [CURRENT_STATUS.md](CURRENT_STATUS.md).

## Current Publication Decision

Do not publish to NuGet without an explicit publish decision from the project owner.

The repository can produce local alpha packages and Apache-2.0 is selected. Public NuGet publication still needs final package ID availability confirmation, a NuGet API key, and a package metadata recheck on the final release commit.

## Required Before Alpha

- Public API names reviewed.
- Package descriptions reviewed.
- License selected by project owner.
- NuGet API key created for the publish workflow.
- GitHub repository secret `NUGET_API_KEY` configured.
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

From the repository root:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

On Windows + WSL setups see the path note in [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md).

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

Checked on 2026-06-10 through the NuGet flat-container API:

- `https://api.nuget.org/v3-flatcontainer/acp.net.process/index.json`: 404
- `https://api.nuget.org/v3-flatcontainer/acp.net.testing/index.json`: 404

Interpretation: these package IDs were not published at check time. Recheck immediately before `dotnet nuget push`.

## GitHub Actions Publishing

Publishing workflow file:

```text
.github/workflows/publish.yml
```

The workflow is manually triggered and requires `confirm_publish=publish`. It uses the `NUGET_API_KEY` repository secret.

## Recommended Publish Order

1. Re-run test and pack.
2. Recheck package IDs.
3. Push `.nupkg` packages.
4. Push `.snupkg` symbol packages.
5. Confirm NuGet package pages show Apache-2.0, README, repository URL, and prerelease version.
6. Confirm GitHub repository visibility matches the release decision.
