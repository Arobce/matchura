"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { AuthFormWrapper } from "@/components/features/auth";
import { Input, Button, Alert } from "@/components/ui";
import { RoleToggle } from "@/components/composed";
import { useAuth } from "@/hooks/useAuth";
import Link from "next/link";

export default function RegisterPage() {
  const router = useRouter();
  const { register } = useAuth();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    role: "Candidate" as "Candidate" | "Employer",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await register(form);
      router.push(form.role === "Employer" ? "/employer/dashboard" : "/dashboard");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  const update = (field: string, value: string) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  return (
    <AuthFormWrapper
      title="Create your account"
      subtitle="Join Matchura to get started"
      footer={
        <div className="text-center mt-6">
          <p className="text-sm text-on-surface-variant font-medium">
            Already have an account?{" "}
            <Link href="/login" className="text-primary hover:text-primary-dim font-bold transition-colors">
              Sign in
            </Link>
          </p>
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {error && <Alert variant="error">{error}</Alert>}
        <RoleToggle value={form.role} onChange={(role) => update("role", role)} />
        <div className="grid grid-cols-2 gap-4">
          <Input
            label="First Name"
            value={form.firstName}
            onChange={(e) => update("firstName", e.target.value)}
            placeholder="Jane"
            required
          />
          <Input
            label="Last Name"
            value={form.lastName}
            onChange={(e) => update("lastName", e.target.value)}
            placeholder="Doe"
            required
          />
        </div>
        <Input
          label="Email Address"
          type="email"
          value={form.email}
          onChange={(e) => update("email", e.target.value)}
          placeholder="name@company.com"
          required
        />
        <Input
          label="Password"
          type="password"
          value={form.password}
          onChange={(e) => update("password", e.target.value)}
          placeholder="Minimum 8 characters"
          required
          minLength={8}
        />
        <Button type="submit" loading={loading} fullWidth>
          Create Account
        </Button>
      </form>
    </AuthFormWrapper>
  );
}
