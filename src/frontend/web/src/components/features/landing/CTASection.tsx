import { Button } from "@/components/ui";

export function CTASection() {
  return (
    <section className="py-24 max-w-screen-2xl mx-auto px-6">
      <div className="bg-primary-container/10 rounded-3xl p-12 md:p-20 flex flex-col items-center text-center overflow-hidden relative">
        <h2 className="text-4xl md:text-5xl font-black mb-6 text-on-surface max-w-2xl">
          Ready to find your professional soulmate?
        </h2>
        <p className="text-on-surface-variant text-lg mb-10 max-w-xl font-medium">
          Join thousands of professionals who have accelerated their careers with Matchura&apos;s AI-driven platform.
        </p>
        <div className="flex flex-col sm:flex-row gap-4 w-full sm:w-auto">
          <Button href="/register" size="lg">Get Started for Free</Button>
          <Button href="/jobs" variant="outline" size="lg">Browse Jobs</Button>
        </div>
      </div>
    </section>
  );
}
