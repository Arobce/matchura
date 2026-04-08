"use client";

import { useState } from "react";
import { Button } from "@/components/ui";
import { MatchScorePanel } from "./MatchScorePanel";
import { SkillGapSection } from "./SkillGapSection";
import type { MatchScoreResponse } from "@/lib/types";
import { Star, Loader2, BookOpen } from "lucide-react";

interface CandidateInsightsPanelProps {
  jobId: string;
  matchScore: MatchScoreResponse | null;
  onComputeMatch: () => void;
  computing: boolean;
}

const tabs = [
  { key: "match", label: "Match Score", icon: Star },
  { key: "skillgap", label: "Skill Gap Analysis", icon: BookOpen },
] as const;

type TabKey = (typeof tabs)[number]["key"];

export function CandidateInsightsPanel({ jobId, matchScore, onComputeMatch, computing }: CandidateInsightsPanelProps) {
  const [activeTab, setActiveTab] = useState<TabKey>("match");

  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow mt-8">
      {/* Tab bar */}
      <div className="flex border-b border-outline-variant">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.key;
          return (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`flex items-center gap-2 px-6 py-3.5 text-sm font-medium transition-colors relative ${
                isActive
                  ? "text-primary"
                  : "text-on-surface-variant hover:text-on-surface"
              }`}
            >
              <Icon className="h-4 w-4" />
              {tab.label}
              {isActive && (
                <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary rounded-t" />
              )}
            </button>
          );
        })}
      </div>

      {/* Tab content */}
      <div className="p-6">
        {activeTab === "match" && (
          <>
            {matchScore ? (
              <MatchScorePanel matchScore={matchScore} />
            ) : (
              <div className="text-center py-12">
                <p className="text-on-surface-variant mb-4">
                  See how well your profile matches this role.
                </p>
                <Button onClick={onComputeMatch} disabled={computing}>
                  {computing ? <Loader2 className="h-4 w-4 animate-spin" /> : <Star className="h-4 w-4" />}
                  Compute Match Score
                </Button>
              </div>
            )}
          </>
        )}

        {activeTab === "skillgap" && (
          <SkillGapSection jobId={jobId} />
        )}
      </div>
    </div>
  );
}
