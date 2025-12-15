using Microsoft.AspNetCore.Components;

namespace TresManos.FrontEnd.Pages;

public class IndexBase : ComponentBase
{
    // Aquí puedes agregar lógica si necesitas cargar estadísticas,
    // usuarios recientes, etc.

    protected override async Task OnInitializedAsync()
    {
        // Ejemplo: cargar datos iniciales si es necesario
        await Task.CompletedTask;
    }
}