import type { NextConfig } from "next";
import { withSentryConfig } from "@sentry/nextjs";

const nextConfig: NextConfig = {
  output: "standalone",
};

export default withSentryConfig(nextConfig, {
  org: "roshan-chapagain",
  project: "matchura-frontend",
  silent: !process.env.CI,
  sourcemaps: {
    disable: !process.env.CI,
  },
  tunnelRoute: "/monitoring",
});
