import { FileText, Sparkles, BarChart3 } from "lucide-react";
import type { LucideIcon } from "lucide-react";

interface Feature {
  icon: LucideIcon;
  title: string;
  description: string;
}

const features: Feature[] = [
  {
    icon: FileText,
    title: "AI Resume Parsing",
    description:
      "Our AI extracts skills and experience automatically from your resume, building a professional profile in seconds with zero manual entry.",
  },
  {
    icon: Sparkles,
    title: "Smart Job Matching",
    description:
      "Get matched with jobs that perfectly align with your background. We look beyond keywords to understand your career trajectory.",
  },
  {
    icon: BarChart3,
    title: "Skill Gap Analysis",
    description:
      "Identify missing skills and get personalized recommendations to land your dream role. Stay ahead of industry trends with ease.",
  },
];

export function FeaturesSection() {
  return (
    <section className="py-32 bg-surface-container-low relative">
      <div className="max-w-screen-2xl mx-auto px-6">
        <div className="text-center mb-20">
          <h2 className="text-4xl font-bold tracking-tight text-on-surface mb-4">Why Matchura?</h2>
          <div className="h-1.5 w-12 bg-primary mx-auto rounded-full" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {features.map((f) => (
            <div
              key={f.title}
              className="bg-surface-container-lowest p-10 rounded-xl editorial-shadow group hover:-translate-y-2 transition-all duration-300"
            >
              <div className="w-16 h-16 bg-primary-container/10 rounded-xl flex items-center justify-center mb-8 group-hover:bg-primary-container group-hover:text-white transition-colors duration-300">
                <f.icon className="h-8 w-8 text-primary group-hover:text-white transition-colors" />
              </div>
              <h3 className="text-2xl font-bold mb-4 text-on-surface">{f.title}</h3>
              <p className="text-on-surface-variant leading-relaxed">{f.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
