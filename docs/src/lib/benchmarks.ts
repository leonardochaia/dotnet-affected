/**
 * Loads the benchmark comparison snapshots under src/data/benchmarks.
 *
 * One file per release, named after it. The release is read from the file name so
 * the snapshots stay exactly as the generator wrote them: they are the record of a
 * run that happened, and nothing here should be able to edit the numbers.
 */

export interface BenchmarkStats {
	minSeconds: number;
	meanSeconds: number;
	stdDevSeconds: number;
	stdDevPercentOfMean: number;
	samplesSeconds: number[] | null;
}

export interface BenchmarkMeasurement {
	totalProjects: number;
	childrenPerProject: number;
	graphNodes: number;
	changedFiles: number;
	affectedProjects: number;
	outputsMatch: boolean;
	iterations: number;
	baseline: BenchmarkStats;
	candidate: BenchmarkStats;
	changePercent: number;
	speedup: number;
	sizeDurationSeconds: number;
	sourceLog: string;
}

export interface ScalingStep {
	fromProjects: number;
	toProjects: number;
	costRatio: number;
	exponent: number;
}

export interface BenchmarkComparison {
	schemaVersion: number;
	generatedAt: string;
	description: string;
	comparison: {
		baseline: { ref: string; commit: string; committedAt: string };
		candidate: { ref: string; commit: string; committedAt: string; subject?: string };
	};
	environment: Record<string, string>;
	method: Record<string, string | boolean | number>;
	measurements: BenchmarkMeasurement[];
	scaling: { note: string; baseline: ScalingStep[]; candidate: ScalingStep[] };
	abandonedRuns?: unknown[];
	notMeasured?: { totalProjects: number; reason: string }[];
	sourceLogs?: unknown[];
}

export interface BenchmarkRelease {
	/** Release the snapshot was taken for, from the file name: `v7.0.0.json` → `7.0.0`. */
	release: string;
	data: BenchmarkComparison;
}

const files = import.meta.glob<BenchmarkComparison>('../data/benchmarks/*.json', {
	eager: true,
	import: 'default',
});

/** Numeric-aware descending sort, so 7.0.0 sorts above 6.10.0 rather than below it. */
function compareReleases(a: string, b: string): number {
	const parts = (release: string) => release.split(/[.-]/).map(part => Number(part) || 0);
	const [left, right] = [parts(a), parts(b)];

	for (let i = 0; i < Math.max(left.length, right.length); i++) {
		const diff = (right[i] ?? 0) - (left[i] ?? 0);
		if (diff !== 0) return diff;
	}

	return 0;
}

export const releases: BenchmarkRelease[] = Object.entries(files)
	.map(([path, data]) => ({
		release: path.replace(/^.*\/v?/, '').replace(/\.json$/, ''),
		data,
	}))
	.sort((a, b) => compareReleases(a.release, b.release));

export const latest: BenchmarkRelease | undefined = releases[0];

/** The largest size measured, which is where the difference is biggest. */
export function headline(data: BenchmarkComparison): BenchmarkMeasurement {
	return data.measurements.reduce((biggest, m) =>
		m.totalProjects > biggest.totalProjects ? m : biggest,
	);
}

export function formatSeconds(seconds: number): string {
	if (seconds >= 100) return `${Math.round(seconds)}s`;
	if (seconds >= 10) return `${seconds.toFixed(1)}s`;
	return `${seconds.toFixed(2)}s`;
}

export function formatProjects(count: number): string {
	return count.toLocaleString('en-US');
}

/**
 * One decimal, for prose and chart labels. The table shows the unrounded value:
 * a label is an approximation, a table is the record.
 */
export function formatSpeedup(speedup: number): string {
	return `${speedup.toFixed(1)}×`;
}
