#!/usr/bin/env node
import { spawn } from "node:child_process";
import { existsSync } from "node:fs";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "../..");
const defaultProject = path.join(
  repoRoot,
  "src",
  "samples",
  "openclaw-subagent-runner",
  "openclaw-subagent-runner.csproj",
);

const args = parseArgs(process.argv.slice(2));
const projectPath = path.resolve(args.project ?? defaultProject);
const cwd = path.resolve(args.cwd ?? repoRoot);
const timeoutMs = parsePositiveInteger(args.timeoutMs) ?? 120_000;

if (!existsSync(projectPath)) {
  fail({
    failureKind: "ConfigurationFailure",
    message: `Project does not exist: ${projectPath}`,
  });
}

const dotnetProjectPath = toDotnetPath(projectPath);
const run = await spawnAndCollect("dotnet", ["run", "--project", dotnetProjectPath], {
  cwd,
  timeoutMs,
});

if (run.timedOut) {
  fail({
    failureKind: "ProcessFailure",
    message: `Probe timed out after ${timeoutMs}ms.`,
    stdout: run.stdout,
    stderr: run.stderr,
  });
}

if (run.exitCode !== 0) {
  fail({
    failureKind: "ProcessFailure",
    message: `Probe command exited with code ${run.exitCode}.`,
    stdout: run.stdout,
    stderr: run.stderr,
  });
}

const sampleResult = parseLastJsonObject(run.stdout);
if (!sampleResult) {
  fail({
    failureKind: "ProtocolFailure",
    message: "Probe command did not print a JSON result.",
    stdout: run.stdout,
    stderr: run.stderr,
  });
}

const runArtifactPath = fromDotnetPath(readString(sampleResult.runArtifactPath));
if (!runArtifactPath) {
  fail({
    failureKind: "ProtocolFailure",
    message: "Probe result did not include runArtifactPath.",
    stdout: run.stdout,
    stderr: run.stderr,
  });
}

let artifact;
try {
  artifact = JSON.parse(await readFile(runArtifactPath, "utf8"));
} catch (error) {
  fail({
    failureKind: "ArtifactFailure",
    message: `Could not read run artifact: ${formatError(error)}`,
    runArtifactPath,
    stdout: run.stdout,
    stderr: run.stderr,
  });
}

const preflight = Array.isArray(artifact.preflight) ? artifact.preflight : [];
const criticalMissing = preflight.filter(
  (item) => item?.found === false && item?.missingPolicy === "Throw",
);
const warnings = preflight.filter(
  (item) => item?.found === false && item?.missingPolicy !== "Throw",
);

const result = {
  kind: "openclaw.acpnet.probe.result",
  ok: artifact.result === "completed" && artifact.failureKind === "None" && criticalMissing.length === 0,
  result: artifact.result ?? "unknown",
  failureKind: artifact.failureKind ?? "Unknown",
  failureMessage: artifact.failureMessage ?? null,
  agentName: artifact.agentName ?? null,
  usesWsl: artifact.usesWsl === true,
  sessionId: readString(sampleResult.sessionId),
  stopReason: readString(sampleResult.stopReason),
  runArtifactPath,
  transcriptPath: fromDotnetPath(readString(artifact.transcriptPath ?? sampleResult.transcriptPath)),
  preflight: {
    total: preflight.length,
    criticalMissing: criticalMissing.map(toToolSummary),
    warnings: warnings.map(toToolSummary),
    tools: preflight.map(toToolSummary),
  },
};

console.log(JSON.stringify(result, null, 2));
process.exit(result.ok ? 0 : 2);

function parseArgs(values) {
  const parsed = {};
  for (let i = 0; i < values.length; i += 1) {
    const value = values[i];
    if (value === "--project" && values[i + 1]) {
      parsed.project = values[++i];
      continue;
    }
    if (value === "--cwd" && values[i + 1]) {
      parsed.cwd = values[++i];
      continue;
    }
    if (value === "--timeout-ms" && values[i + 1]) {
      parsed.timeoutMs = values[++i];
      continue;
    }
    if (value === "--help" || value === "-h") {
      console.log(`Usage: node ${path.basename(fileURLToPath(import.meta.url))} [--project <csproj>] [--cwd <dir>] [--timeout-ms <ms>]`);
      process.exit(0);
    }
    fail({
      failureKind: "ConfigurationFailure",
      message: `Unknown argument: ${value}`,
    });
  }
  return parsed;
}

function spawnAndCollect(command, commandArgs, options) {
  return new Promise((resolve) => {
    const child = spawn(command, commandArgs, {
      cwd: options.cwd,
      env: process.env,
      windowsHide: true,
    });

    let stdout = "";
    let stderr = "";
    let timedOut = false;
    const timeout = setTimeout(() => {
      timedOut = true;
      child.kill("SIGTERM");
      setTimeout(() => child.kill("SIGKILL"), 2_000).unref();
    }, options.timeoutMs);
    timeout.unref();

    child.stdout?.setEncoding("utf8");
    child.stderr?.setEncoding("utf8");
    child.stdout?.on("data", (chunk) => {
      stdout += chunk;
    });
    child.stderr?.on("data", (chunk) => {
      stderr += chunk;
    });
    child.on("error", (error) => {
      clearTimeout(timeout);
      resolve({ exitCode: 1, stdout, stderr: `${stderr}${formatError(error)}`, timedOut });
    });
    child.on("close", (exitCode) => {
      clearTimeout(timeout);
      resolve({ exitCode: exitCode ?? 1, stdout, stderr, timedOut });
    });
  });
}

function parseLastJsonObject(text) {
  for (let start = text.lastIndexOf("{"); start >= 0; start = text.lastIndexOf("{", start - 1)) {
    const candidate = text.slice(start).trim();
    try {
      return JSON.parse(candidate);
    } catch {
      // Keep scanning earlier braces.
    }
  }
  return null;
}

function toDotnetPath(value) {
  if (process.platform !== "linux" || !process.env.WSL_DISTRO_NAME || value.startsWith("\\\\")) {
    return value;
  }
  const distro = process.env.WSL_DISTRO_NAME;
  return `\\\\wsl.localhost\\${distro}${value.replace(/\//g, "\\")}`;
}

function fromDotnetPath(value) {
  if (!value) {
    return null;
  }
  const match = /^\\\\wsl\.localhost\\([^\\]+)\\(.+)$/i.exec(value);
  if (!match) {
    return value;
  }
  return `/${match[2].replace(/\\/g, "/")}`;
}

function readString(value) {
  return typeof value === "string" && value.trim() ? value : null;
}

function parsePositiveInteger(value) {
  if (value === undefined) {
    return undefined;
  }
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
}

function toToolSummary(item) {
  return {
    name: readString(item?.name),
    found: item?.found === true,
    path: readString(item?.path),
    error: readString(item?.error),
    missingPolicy: readString(item?.missingPolicy) ?? "Warn",
  };
}

function formatError(error) {
  return error instanceof Error ? error.message : String(error);
}

function fail(payload) {
  console.log(
    JSON.stringify(
      {
        kind: "openclaw.acpnet.probe.result",
        ok: false,
        ...payload,
      },
      null,
      2,
    ),
  );
  process.exit(2);
}
