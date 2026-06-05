
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce527.API.Utilities.DbSeeder
{
    public class DbInitializer : IDbInitializer 
    {
        private readonly ApplicationDbContext _context ;
        private readonly ILogger<DbInitializer> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;


        public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if(_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate(); 
                }
                if (!_roleManager.Roles.Any())
                {
                    await _roleManager.CreateAsync(new IdentityRole(CD.SUPER_ADMIN_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(CD.ADMIN_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(CD.EMPLOYEE_ROLE));
                    await _roleManager.CreateAsync(new IdentityRole(CD.CUSTOMER_ROLE));

                    await _userManager.CreateAsync(new ApplicationUser()
                    {
                        Name = "SuperAdmin",
                        UserName = "SuperAdmin",
                        Email = "superadmin@eraasoft.com",
                        EmailConfirmed = true , 
                        Address = "Cairo" , 
                    }, "SuperAdmin@123");

                    var user = await  _userManager.FindByNameAsync("SuperAdmin");

                    await  _userManager.AddToRoleAsync(user , CD.SUPER_ADMIN_ROLE);

                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
