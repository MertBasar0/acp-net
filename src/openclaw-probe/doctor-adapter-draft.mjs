#!/usr/bin/env node
import { readFileSync } from "node:fs";

const CHECK_ID = "plugin/acpnet/diagnostic-probe";

export function mapProbeResultToDoctorReport(result, exitCode = 0) {
  const warnings = readArray(result?.preflight?.warnings);
  const criticalMissing = readArray(result?.preflight?.criticalMissing);

  if (result?.ok === true && warnings.length === 0) {
    return {
      ok: true,
      message: "Acp.Net diagnostic probe completed.",
    };
  }

  if (result?.ok === true) {
    return {
      ok: true,
      code: "ACPNET_PREFLIGHT_WARNING",
      message: "Acp.Net diagnostic probe completed with optional runtime warnings.",
      details: warnings.map(formatOptionalTool),
    };
  }

  if (exitCode === 2 || result?.failureKind === "EnvironmentFailure") {
    return {
      ok: false,
      code: "ACPNET_ENVIRONMENT_FAILURE",
      message: "Acp.Net diagnostic probe failed before the agent started.",
      details: criticalMissing.map(formatCriticalTool),
    };
  }

  if (exitCode === 64 || result?.failureKind === "ConfigurationFailure") {
    return {
      ok: false,
      code: "ACPNET_PROBE_CONFIG_INVALID",
      message: "Acp.Net diagnostic probe configuration is invalid.",
      details: [result?.failureMessage].filter(Boolean),
    };
  }

  return {
    ok: false,
    code: "ACPNET_PROBE_FAILED",
    message: "Acp.Net diagnostic probe failed.",
    details: [result?.failureMessage, result?.transcriptPath, result?.runArtifactPath].filter(Boolean),
  };
}

export function mapProbeResultToHealthFindings(result, exitCode = 0) {
  const warnings = readArray(result?.preflight?.warnings);
  const criticalMissing = readArray(result?.preflight?.criticalMissing);

  if (result?.ok === true) {
    return warnings.map((tool) => ({
      checkId: CHECK_ID,
      severity: "warning",
      message: `Acp.Net optional runtime tool is missing: ${readToolName(tool)}`,
      source: "acpnet",
      target: readToolName(tool),
      requirement: "optional executable",
      fixHint: `Install ${readToolName(tool)} in the agent runtime or pass it through PATH/AdditionalPathEntries.`,
    }));
  }

  if (exitCode === 2 || result?.failureKind === "EnvironmentFailure") {
    return criticalMissing.map((tool) => ({
      checkId: CHECK_ID,
      severity: "error",
      message: `Acp.Net critical runtime tool is missing: ${readToolName(tool)}`,
      source: "acpnet",
      target: readToolName(tool),
      requirement: "critical executable",
      fixHint: `Install ${readToolName(tool)} in the agent runtime or configure the probe with a valid PATH.`,
    }));
  }

  if (exitCode === 64 || result?.failureKind === "ConfigurationFailure") {
    return [
      {
        checkId: CHECK_ID,
        severity: "error",
        message: `Acp.Net diagnostic probe configuration is invalid: ${result?.failureMessage ?? "unknown configuration error"}`,
        source: "acpnet",
        requirement: "valid probe configuration",
        fixHint: "Fix the Acp.Net probe command arguments.",
      },
    ];
  }

  return [
    {
      checkId: CHECK_ID,
      severity: "error",
      message: `Acp.Net diagnostic probe failed: ${result?.failureMessage ?? result?.failureKind ?? "unknown failure"}`,
      source: "acpnet",
      requirement: "successful diagnostic probe",
      fixHint: "Inspect the run artifact and transcript paths reported by the probe.",
    },
  ];
}

function readArray(value) {
  return Array.isArray(value) ? value : [];
}

function readToolName(tool) {
  return typeof tool?.name === "string" && tool.name.trim() ? tool.name : "unknown";
}

function formatCriticalTool(tool) {
  return `Critical tool missing: ${readToolName(tool)}${tool?.error ? ` (${tool.error})` : ""}`;
}

function formatOptionalTool(tool) {
  return `Optional tool missing: ${readToolName(tool)}${tool?.error ? ` (${tool.error})` : ""}`;
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const inputPath = process.argv[2];
  const exitCode = Number(process.argv[3] ?? 0);
  if (!inputPath) {
    console.error("Usage: doctor-adapter-draft.mjs <probe-result.json> [exitCode]");
    process.exit(64);
  }
  const result = JSON.parse(readFileSync(inputPath, "utf8"));
  console.log(
    JSON.stringify(
      {
        doctorReport: mapProbeResultToDoctorReport(result, exitCode),
        healthFindings: mapProbeResultToHealthFindings(result, exitCode),
      },
      null,
      2,
    ),
  );
}
