# NuGet Prepublish Check

Date: 2026-06-10

## Scope

Checked whether `Acp.Net.Process` and `Acp.Net.Testing` are ready for an alpha NuGet publish.

No package was published.

## Result

The packages are technically publishable as `0.1.0-alpha.1`, but publication should still wait for an explicit owner decision.

## Checks

### Package ID Availability

NuGet flat-container API returned 404 for both IDs:

- `https://api.nuget.org/v3-flatcontainer/acp.net.process/index.json`
- `https://api.nuget.org/v3-flatcontainer/acp.net.testing/index.json`

Interpretation: no currently published package was visible for either ID at check time. Recheck immediately before `dotnet nuget push`.

### Local Test Suite

Command:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Result:

- `Acp.Net.UnitTests`: 14 passed
- `Acp.Net.IntegrationTests`: 3 passed

### Package Build

Both packages built successfully:

- `Acp.Net.Process.0.1.0-alpha.1.nupkg`
- `Acp.Net.Process.0.1.0-alpha.1.snupkg`
- `Acp.Net.Testing.0.1.0-alpha.1.nupkg`
- `Acp.Net.Testing.0.1.0-alpha.1.snupkg`

### Package Metadata

Both packages include:

- `PackageLicenseExpression`: `Apache-2.0`
- `PackageProjectUrl`: `https://github.com/MertBasar0/acp-net`
- `RepositoryUrl`: `https://github.com/MertBasar0/acp-net`
- `RepositoryType`: `git`
- `Copyright`: `Copyright 2026 Mert Basar`
- `README.md`

Symbol packages contain only the expected `.pdb` payloads.

### Local Consumer Smoke Test

A clean temporary consumer project restored both packages from the local artifact feed and used:

- `AcpNet.Process`
- `AcpNet.Testing`
- `FakeAcpAgentScript`
- `AcpProcessRunner`
- `AcpRequiredExecutable`
- `AcpShutdownPolicy`
- `AcpTranscriptAssert`

Result with WSL runtime:

```text
ExitCode=0
UsesWsl=True
```

An initial native-runtime attempt on this Windows/WSL setup produced process exit code `9009` for `python3`. This is not a package failure; it confirms that real Linux-side agents should be launched through `AcpRuntime.Wsl` or `AcpRuntime.Auto` with WSL paths in this environment.

### GitHub Actions

Latest CI run for commit `1158ba6e346314679d8b79e004e1223a79c4e00e` completed successfully.

## Public API Review

No blocking API naming issue was found for alpha.

The alpha public surface is small enough:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRequiredExecutable`
- `AcpExecutablePreflightResult`
- `AcpPreflightException`
- `AcpRunArtifact`
- `AcpRunFailureKind`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

Known alpha-level concern:

- `AcpTranscriptRecorder.In(...)` and `Out(...)` are low-level names. They are acceptable for alpha but should be reconsidered before a stable release.

## Publish Blockers

None are technical blockers for an alpha package.

Remaining operational blockers:

- explicit owner approval to publish
- NuGet API key
- GitHub repository secret `NUGET_API_KEY`
- final package ID availability check immediately before push
- decision to make GitHub repo public immediately after NuGet publish

## Recommended Publish Order

1. Re-run test and pack.
2. Recheck package IDs.
3. Push `.nupkg` packages.
4. Push `.snupkg` symbol packages.
5. Confirm NuGet package pages show Apache-2.0, README, repository URL, and prerelease version.
6. Make GitHub repo public.

GitHub Actions publishing workflow file:

```text
publish.yml
```
