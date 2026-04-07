import { Button } from "@/components/ui";

export function HeroSection() {
  return (
    <section className="relative pt-20 pb-32 overflow-hidden bg-surface">
      <div className="absolute inset-0 z-0 opacity-20">
        <div className="absolute top-[-10%] left-[-10%] w-[50%] h-[50%] rounded-full bg-primary-container blur-[120px]" />
        <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] rounded-full bg-tertiary-container blur-[100px]" />
      </div>
      <div className="max-w-screen-2xl mx-auto px-6 relative z-10 flex flex-col items-center text-center">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-secondary-container/50 text-on-secondary-container text-xs font-bold tracking-widest uppercase mb-8 border border-white/20">
          AI-Driven Career Matching
        </div>
        <h1 className="text-6xl md:text-8xl font-black tracking-tighter text-on-surface mb-6 max-w-5xl leading-[1.05]">
          Find Your Perfect Job <span className="text-primary italic">Match</span> with AI
        </h1>
        <p className="text-lg md:text-xl text-on-surface-variant max-w-2xl mb-12 font-medium leading-relaxed">
          Experience the next generation of job searching with AI-driven resume parsing, smart job matching, and comprehensive skill gap analysis.
        </p>
        <div className="flex flex-col sm:flex-row gap-4 w-full sm:w-auto">
          <Button href="/jobs" size="lg">Find Jobs</Button>
          <Button href="/register" variant="outline" size="lg">Get Started</Button>
        </div>
      </div>
    </section>
  );
}
