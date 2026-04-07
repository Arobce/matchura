using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Candidate", "Employer", "Admin"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Skip if users already exist
        if (userManager.Users.Any(u => u.Email == "alice@example.com")) return;

        var users = new (ApplicationUser User, string Password, string Role)[]
        {
            // ── Candidates ──
            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000001",
                UserName = "alice@example.com",
                Email = "alice@example.com",
                FullName = "Alice Johnson",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000002",
                UserName = "bob@example.com",
                Email = "bob@example.com",
                FullName = "Bob Williams",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000003",
                UserName = "carol@example.com",
                Email = "carol@example.com",
                FullName = "Carol Martinez",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000004",
                UserName = "david@example.com",
                Email = "david@example.com",
                FullName = "David Chen",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000005",
                UserName = "emma@example.com",
                Email = "emma@example.com",
                FullName = "Emma Davis",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000006",
                UserName = "frank@example.com",
                Email = "frank@example.com",
                FullName = "Frank Nguyen",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000007",
                UserName = "grace@example.com",
                Email = "grace@example.com",
                FullName = "Grace Kim",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            (new ApplicationUser
            {
                Id = "c1000000-0000-0000-0000-000000000008",
                UserName = "henry@example.com",
                Email = "henry@example.com",
                FullName = "Henry Patel",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Candidate"),

            // ── Employers ──
            (new ApplicationUser
            {
                Id = "e1000000-0000-0000-0000-000000000001",
                UserName = "hr@techcorp.com",
                Email = "hr@techcorp.com",
                FullName = "Sarah Thompson",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Employer"),

            (new ApplicationUser
            {
                Id = "e1000000-0000-0000-0000-000000000002",
                UserName = "jobs@dataflow.io",
                Email = "jobs@dataflow.io",
                FullName = "Michael Rivera",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Employer"),

            (new ApplicationUser
            {
                Id = "e1000000-0000-0000-0000-000000000003",
                UserName = "careers@cloudnine.dev",
                Email = "careers@cloudnine.dev",
                FullName = "Jennifer Lee",
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active,
            }, "Matchura1!", "Employer"),
        };

        foreach (var (user, password, role) in users)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
