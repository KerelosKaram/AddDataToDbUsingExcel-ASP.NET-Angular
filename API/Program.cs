namespace MessagingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create and configure the host
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Use Startup class for configuration
                    webBuilder.UseStartup<Startup>();
                });
    }
}
