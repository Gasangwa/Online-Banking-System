using ThesisProject.Data;
using ThesisProject.Models;

namespace ThesisProject.Areas.Identity.Data
{
    public class EnsureCreate
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var _context = serviceScope.ServiceProvider.GetService<ThesisProjectContext>();
                if (_context != null)
                {
                    _context.Database.EnsureCreated();
                }
            }
        }
    }
}
