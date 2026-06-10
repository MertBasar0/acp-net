# Release Checklist

Last updated: 2026-06-10

Use this before any alpha NuGet publication.

## Required Before Alpha

- Public API names reviewed.
- Package descriptions reviewed.
- License selected by project owner.
- README examples tested from a fresh clone.
- `dotnet test` passes.
- `dotnet pack` passes for `Acp.Net.Process`.
- `dotnet pack` passes for `Acp.Net.Testing`.
- Generated packages inspected locally.
- CI added or intentionally deferred with a written reason.

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
