#!/usr/bin/env node
import { readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  mapProbeResultToDoctorReport,
  mapProbeResultToHealthFindings,
} from "./doctor-adapter-draft.mjs";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const scenariosPath = path.resolve(
  scriptDir,
  "../../docs/contracts/openclaw-doctor-mapping.scenarios.json",
);
const scenarios = JSON.parse(readFileSync(scenariosPath, "utf8")).scenarios;

let failures = 0;
for (const scenario of scenarios) {
  const actualDoctorReport = mapProbeResultToDoctorReport(
    scenario.probeResult,
    scenario.exitCode,
  );
  const actualHealthFindings = mapProbeResultToHealthFindings(
    scenario.probeResult,
    scenario.exitCode,
  );
  if (!deepEqual(actualDoctorReport, scenario.doctorReport)) {
    failures++;
    console.error(`doctorReport mismatch: ${scenario.name}`);
    console.error(JSON.stringify({ expected: scenario.doctorReport, actual: actualDoctorReport }, null, 2));
  }
  if (!deepEqual(actualHealthFindings, scenario.healthFindings)) {
    failures++;
    console.error(`healthFindings mismatch: ${scenario.name}`);
    console.error(JSON.stringify({ expected: scenario.healthFindings, actual: actualHealthFindings }, null, 2));
  }
}

if (failures > 0) {
  process.exit(1);
}

console.log(`doctor adapter scenarios ok (${scenarios.length})`);

function deepEqual(left, right) {
  return JSON.stringify(left) === JSON.stringify(right);
}
