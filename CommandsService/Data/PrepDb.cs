using CommandsService.Models;

namespace CommandsService.Data
{
    public class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SeedData(AppDbContext context)
        {
            if (!context.Platforms.Any())
            {
                Console.WriteLine("--> Seeding Data...");

                context.Platforms.AddRange(
                    new Platform() { Name = "Dot Net", ExternalID = 1 },
                    new Platform() { Name = "SQL Server Express", ExternalID = 2 },
                    new Platform() { Name = "Kubernetes", ExternalID = 3 }
                );
                context.SaveChanges();

                context.Commands.Add(new Command() { PlatformId = 1, HowTo = "Build a .net project", CommandLine = ".net build" });
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}
