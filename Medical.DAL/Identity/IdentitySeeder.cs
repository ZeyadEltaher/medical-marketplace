using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Medical.DAL.Identity
{
    public class IdentitySeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public IdentitySeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminAsync();
        }

        public async Task SeedRolesAsync()
        {
            foreach (RoleName role in Enum.GetValues<RoleName>())
            {
                string roleName = role.ToString();

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    IdentityResult result = await _roleManager.CreateAsync(
                        new IdentityRole(roleName)
                    );

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Failed to create role '{roleName}': " +
                            string.Join(", ", result.Errors.Select(e => e.Description))
                        );
                    }
                }
            }
        }

        public async Task SeedAdminAsync()
        {
            string? userName = _configuration["Admin:Username"];
            string? email = _configuration["Admin:Email"];

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "Admin username and email are not configured."
                );
            }

            ApplicationUser? admin = await _userManager.FindByEmailAsync(email);

            if (admin is null)
            {
                string? password = _configuration["Admin:Password"];

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "Admin password is not configured."
                    );
                }

                admin = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                };

                IdentityResult createAdminResult =
                    await _userManager.CreateAsync(admin, password);

                if (!createAdminResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create admin user: " +
                        string.Join(", ", createAdminResult.Errors.Select(e => e.Description))
                    );
                }

                IdentityResult assignRoleResult =
                    await _userManager.AddToRoleAsync(admin, RoleName.Admin.ToString());

                if (!assignRoleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign Admin role: " +
                        string.Join(", ", assignRoleResult.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }
}