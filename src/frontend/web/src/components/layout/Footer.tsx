import Link from "next/link";

const footerLinks = {
  Product: [
    { label: "Browse Jobs", href: "/jobs" },
    { label: "Resume Parser", href: "/resumes" },
    { label: "Skill Gap AI", href: "/skill-gap" },
  ],
  Company: [
    { label: "About Us", href: "#" },
    { label: "Careers", href: "#" },
    { label: "Contact", href: "#" },
  ],
  Legal: [
    { label: "Privacy Policy", href: "#" },
    { label: "Terms of Service", href: "#" },
    { label: "Support", href: "#" },
  ],
};

export function Footer() {
  return (
    <footer className="bg-surface py-12 border-t border-outline-variant/10">
      <div className="max-w-7xl mx-auto px-8 grid grid-cols-1 md:grid-cols-4 gap-12 md:gap-8">
        <div>
          <span className="text-xl font-bold text-on-surface mb-6 block">Matchura</span>
          <p className="text-on-surface-variant text-sm leading-relaxed">
            Redefining the job search with intelligent automation and human-centric design.
          </p>
        </div>
        {Object.entries(footerLinks).map(([title, links]) => (
          <div key={title}>
            <h5 className="text-xs uppercase tracking-widest font-bold text-on-surface-variant mb-6">{title}</h5>
            <ul className="space-y-4">
              {links.map((link) => (
                <li key={link.label}>
                  <Link href={link.href} className="text-on-surface-variant hover:text-primary transition-colors text-sm">
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
      <div className="max-w-7xl mx-auto px-8 pt-12 mt-12 border-t border-outline-variant/10 flex flex-col md:flex-row justify-between items-center gap-4">
        <p className="text-xs uppercase tracking-widest text-on-surface-variant">
          &copy; {new Date().getFullYear()} Matchura. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
