# ADR-0000: Spike Results And Direction Decision

> 🇹🇷 Türkçe sürüm: [ADR-0000-spike-results-and-direction.tr.md](ADR-0000-spike-results-and-direction.tr.md)

Date: 2026-06-07

## Status

The earlier spikes and document reviews produced the following picture:

| Area | Proven | Not proven |
| --- | --- | --- |
| Acp.Net | A .NET 8 client, stdio, Python process, and JSON-RPC flow can work together | A clear differentiator against the existing AgentClientProtocol package |
| Platform interop | Windows/WSL paths, process lifecycle, and runtime validation are the main difficulty | That every edge case can be solved at the SDK level |
| Training Factory | Rollouts can run in a toy RL environment | That SB3 training beats a baseline; Gazebo/PX4 stability; reproducibility |
| Agent productivity | Code generation is fast, verification is slow | That agent-driven development is a deterministic accelerator |

## Decision

Work on Acp.Net should continue, but a productization decision must only be made after the incumbent comparison.

Training Factory should not be treated as the main product for now. It should be shelved as a future test/dogfooding area that Acp.Net may use. The next Training Factory experiment must be a real SB3 learning baseline, not a random rollout.

## Rationale

Spike 1 showed that Acp.Net's value area is not writing a protocol wrapper but cleaning up the platform's messy realities at the SDK/API level. Process lifecycle, Windows/WSL path translation, the stdio bridge, cancel/timeout, and test helpers are where a difference can be made.

Spike 2 is not sufficient for Training Factory. Running 20 steps with a random agent proves the environment can be set up, but it does not carry a "this has product value" conclusion. Value requires a training signal, an eval suite, a baseline comparison, and reproducibility.

Spike 3 showed that the agent's acceleration effect does not remove the verification bottleneck. Time plans should therefore be based on test and environment verification costs, not code-writing speed.

## Outcome

The next critical decision gate is the AgentClientProtocol incumbent comparison. Acp.Net should not be fully committed to before that test. If the test shows a clear difference, Acp.Net narrows down to a tightly scoped MVP; if the difference is weak, the project is repositioned or stopped.

## Accepted Time Outlook

- Acp.Net incumbent comparison: 1 day.
- Acp.Net MVP scope/prototype: 1–3 weeks, only if focused on the differentiating areas.
- Training Factory real spike: 2–4 weeks.
- Training Factory productization: at least 6–12 months with high integration risk.

## Evidence

The spike session reports behind this ADR (spikes 001–003) are kept in the untracked `notes/` folder at the repository root (ignored by git, not pushed to the remote).
