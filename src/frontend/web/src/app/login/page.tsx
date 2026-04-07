"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { AuthFormWrapper } from "@/components/features/auth";
import { Input, Button, Alert, Divider } from "@/components/ui";
import { useAuth } from "@/hooks/useAuth";
import Link from "next/link";

export default function LoginPage() {
  const router = useRouter();
  const { login, user } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login({ email, password });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user) {
      router.push(user.role === "Employer" ? "/employer/dashboard" : "/dashboard");
    }
  }, [user, router]);

  return (
    <AuthFormWrapper
      title="Welcome back"
      subtitle="Sign in to your account"
      footer={
        <div className="text-center mt-6">
          <p className="text-sm text-on-surface-variant font-medium">
            Don&apos;t have an account?{" "}
            <Link href="/register" className="text-primary hover:text-primary-dim font-bold transition-colors">
              Create one
            </Link>
          </p>
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {error && <Alert variant="error">{error}</Alert>}
        <Input
          label="Email Address"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="name@company.com"
          required
        />
        <Input
          label="Password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="••••••••"
          required
        />
        <Button type="submit" loading={loading} fullWidth>
          Sign In
        </Button>
      </form>
      <Divider label="OR CONTINUE WITH" />
      <div className="text-center">
        <p className="text-sm text-on-surface-variant">Social login coming soon</p>
      </div>
    </AuthFormWrapper>
  );
}
