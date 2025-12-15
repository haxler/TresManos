using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace TresManos.FrontEnd;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // HttpClient apuntando a tu backend (API)
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7029/")
        });

        // Registrar MudBlazor
        builder.Services.AddMudServices();

        await builder.Build().RunAsync();
    }
}