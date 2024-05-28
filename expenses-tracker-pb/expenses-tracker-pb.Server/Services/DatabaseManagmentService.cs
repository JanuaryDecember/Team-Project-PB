using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace expenses_tracker_pb.Server.Services
{
    public class DatabaseManagmentService
    {
        public static void MigrationInitialisation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ETDbContext>().Database.Migrate();
            }
        }

        public static string getConnectionString(WebApplicationBuilder builder)
        {
            string connectionString;

            var server = builder.Configuration["server"] ?? null;
            var database = builder.Configuration["database"] ?? null;
            var port = builder.Configuration["port"] ?? null;
            var pass = builder.Configuration["pass"] ?? null;
            var user = builder.Configuration["dbuser"] ?? null;

            if (server != null && server !=null && port !=null && pass != null && user != null)
            {
                connectionString = $"Server={server}, {port};Initial Catalog={database};User ID={user};Password={pass};Trust Server Certificate=True";
            }
            else
            {
                connectionString = builder.Configuration.GetConnectionString("DawidConnection");
            }
            
            return connectionString;
        }
    }
}