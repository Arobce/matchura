import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const protectedPrefixes = ["/dashboard", "/profile", "/resumes", "/applications", "/skill-gap", "/company", "/analytics"];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Check if the path needs protection
  const isProtected = protectedPrefixes.some((prefix) => pathname.startsWith(prefix));
  if (!isProtected) return NextResponse.next();

  // Check for JWT token in cookies or authorization header
  const token = request.cookies.get("token")?.value;

  if (!token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/profile/:path*",
    "/resumes/:path*",
    "/applications/:path*",
    "/skill-gap/:path*",
    "/company/:path*",
    "/analytics/:path*",
    "/jobs/manage/:path*",
    "/employer/postjob/:path*",
  ],
};
