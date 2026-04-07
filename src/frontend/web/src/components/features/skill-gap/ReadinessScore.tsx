interface ReadinessScoreProps {
  score: number;
  summary?: string;
}

export function ReadinessScore({ score, summary }: ReadinessScoreProps) {
  const dashOffset = 553 - (553 * score) / 100;

  return (
    <div className="bg-surface-container-lowest p-8 rounded-2xl editorial-shadow flex flex-col items-center justify-center text-center">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-8">Overall Readiness</h3>
      <div className="relative w-48 h-48 flex items-center justify-center">
        <svg className="w-full h-full transform -rotate-90">
          <circle className="text-surface-container-high" cx="96" cy="96" fill="transparent" r="88" stroke="currentColor" strokeWidth="12" />
          <circle className="text-primary" cx="96" cy="96" fill="transparent" r="88" stroke="currentColor" strokeDasharray="553" strokeDashoffset={dashOffset} strokeWidth="12" strokeLinecap="round" />
        </svg>
        <div className="absolute inset-0 flex flex-col items-center justify-center">
          <span className="text-5xl font-black text-on-surface">{score}%</span>
          <span className="text-xs font-semibold text-primary mt-1">Ready for Role</span>
        </div>
      </div>
      {summary && (
        <p className="mt-8 text-sm text-on-surface-variant leading-relaxed">{summary}</p>
      )}
    </div>
  );
}
