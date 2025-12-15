using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TresManos.FrontEnd.Pages
{
    public class GanadorBase : ComponentBase
    {
        [Parameter] public string? GanadorNombre { get; set; }

        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected string NombreGanador { get; set; } = "1";

        protected override void OnInitialized()
        {
            // Si se pasa el nombre del ganador por parámetro de ruta
            if (!string.IsNullOrWhiteSpace(GanadorNombre))
            {
                NombreGanador = GanadorNombre;
            }
        }

        /// <summary>
        /// Navega a una revancha con los mismos jugadores
        /// </summary>
        protected void Revancha()
        {
            Snackbar.Add("Iniciando revancha...", Severity.Info);

            // Aquí puedes navegar a la página de partida con los mismos jugadores
            // Por ejemplo, si tienes guardado el ID de la última partida:
            // Navigation.NavigateTo($"/partida/{ultimaPartidaId}");

            // O simplemente ir a nueva partida:
            Navigation.NavigateTo("/nueva-partida");
        }

        /// <summary>
        /// Navega a un nuevo juego (registrar nuevos jugadores)
        /// </summary>
        protected void NuevoJuego()
        {
            Snackbar.Add("Iniciando nuevo juego...", Severity.Success);
            Navigation.NavigateTo("/registro-jugadores");
        }
    }
}