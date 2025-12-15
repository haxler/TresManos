using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TresManos.FrontEnd.Pages;

public class JugarPartidaBase : ComponentBase
{
    [Parameter] public int PartidaId { get; set; }

    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected PartidaDetalleDto? Partida { get; set; }
    protected List<RondaDto> Rondas { get; set; } = new();

    protected string MovimientoJugador1 { get; set; } = string.Empty;
    protected string MovimientoJugador2 { get; set; } = string.Empty;

    protected bool IsLoading { get; set; } = true;
    protected bool IsSubmitting { get; set; } = false;
    protected bool MostrandoResultado { get; set; } = false;

    protected string UltimoResultado { get; set; } = string.Empty;
    protected string UltimoMovimientoJ1 { get; set; } = string.Empty;
    protected string UltimoMovimientoJ2 { get; set; } = string.Empty;

    protected int RondasGanadasJ1 => Rondas?.Count(r => r.Resultado == "1") ?? 0;
    protected int RondasGanadasJ2 => Rondas?.Count(r => r.Resultado == "2") ?? 0;

    protected override async Task OnInitializedAsync()
    {
        await CargarPartida();
    }

    protected async Task CargarPartida()
    {
        try
        {
            IsLoading = true;

            // Cargar información de la partida
            var responsePartida = await Http.GetAsync($"api/partidas/{PartidaId}/completa");
            if (responsePartida.IsSuccessStatusCode)
            {
                Partida = await responsePartida.Content.ReadFromJsonAsync<PartidaDetalleDto>();
            }
            else
            {
                Snackbar.Add("No se pudo cargar la partida", Severity.Error);
                return;
            }

            // Cargar rondas jugadas
            var responseRondas = await Http.GetAsync($"api/rondas/partida/{PartidaId}");
            if (responseRondas.IsSuccessStatusCode)
            {
                Rondas = await responseRondas.Content.ReadFromJsonAsync<List<RondaDto>>() ?? new();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar la partida: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task JugarRonda()
    {
        try
        {
            if (string.IsNullOrEmpty(MovimientoJugador1) || string.IsNullOrEmpty(MovimientoJugador2))
            {
                Snackbar.Add("Ambos jugadores deben seleccionar un movimiento", Severity.Warning);
                return;
            }

            IsSubmitting = true;
            MostrandoResultado = false;

            var nuevaRonda = new RondaCreateDto
            {
                PartidaId = PartidaId,
                MovimientoJugador1 = MovimientoJugador1,
                MovimientoJugador2 = MovimientoJugador2
            };

            var response = await Http.PostAsJsonAsync("api/rondas", nuevaRonda);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error al crear la ronda: {error}", Severity.Error);
                return;
            }

            var rondaCreada = await response.Content.ReadFromJsonAsync<RondaDto>();
            if (rondaCreada != null)
            {
                Rondas.Add(rondaCreada);

                // Guardar resultado para mostrarlo
                UltimoResultado = rondaCreada.Resultado;
                UltimoMovimientoJ1 = rondaCreada.MovimientoJugador1;
                UltimoMovimientoJ2 = rondaCreada.MovimientoJugador2;
                MostrandoResultado = true;

                // Limpiar selecciones
                MovimientoJugador1 = string.Empty;
                MovimientoJugador2 = string.Empty;

                // Recargar partida para verificar si finalizó
                await CargarPartida();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error inesperado: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    protected string ObtenerNombreMovimiento(string movimiento) => movimiento switch
    {
        "P" => "Piedra",
        "A" => "Papel",
        "T" => "Tijera",
        _ => "Desconocido"
    };

    // DTOs

    public class PartidaDetalleDto
    {
        public int PartidaId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string NombreJugador1 { get; set; } = string.Empty;
        public string NombreJugador2 { get; set; } = string.Empty;
        public string? NombreGanador { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class RondaDto
    {
        public int RondaId { get; set; }
        public int PartidaId { get; set; }
        public int NumeroRonda { get; set; }
        public string MovimientoJugador1 { get; set; } = string.Empty;
        public string MovimientoJugador2 { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class RondaCreateDto
    {
        public int PartidaId { get; set; }
        public string MovimientoJugador1 { get; set; } = string.Empty;
        public string MovimientoJugador2 { get; set; } = string.Empty;
    }
}