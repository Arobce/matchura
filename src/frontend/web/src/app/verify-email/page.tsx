"use client";

import { Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { AuthFormWrapper } from "@/components/features/auth";
import { Input, Button, Alert, Spinner } from "@/components/ui";
import { api } from "@/lib/api";

function VerifyEmailForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const emailParam = searchParams.get("email") || "";

  const [email, setEmail] = useState(emailParam);
  const [code, setCode] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      await api.post("/api/auth/verify-email", { email, code });
      setSuccess("Email verified! Redirecting to login...");
      setTimeout(() => router.push("/login"), 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verification failed");
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (!email) {
      setError("Please enter your email address");
      return;
    }
    setError("");
    setResending(true);
    try {
      await api.post("/api/auth/resend-verification", { email });
      setSuccess("New verification code sent! Check your email.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to resend code");
    } finally {
      setResending(false);
    }
  };

  return (
    <AuthFormWrapper
      title="Verify your email"
      subtitle="Enter the verification code sent to your email"
    >
      <form onSubmit={handleVerify} className="space-y-6">
        {error && <Alert variant="error">{error}</Alert>}
        {success && <Alert variant="success">{success}</Alert>}
        <Input
          label="Email Address"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="name@company.com"
          required
        />
        <Input
          label="Verification Code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          placeholder="Enter 6-digit code"
          required
        />
        <Button type="submit" loading={loading} fullWidth>
          Verify Email
        </Button>
      </form>
      <div className="mt-4 text-center">
        <button
          type="button"
          onClick={handleResend}
          disabled={resending}
          className="text-sm text-primary hover:text-primary-dim font-medium transition-colors disabled:opacity-50"
        >
          {resending ? "Sending..." : "Didn't receive a code? Resend"}
        </button>
      </div>
    </AuthFormWrapper>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={<Spinner size="lg" />}>
      <VerifyEmailForm />
    </Suspense>
  );
}
