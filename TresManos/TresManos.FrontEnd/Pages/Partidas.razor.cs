using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TresManos.FrontEnd.Pages;

/// <summary>
/// Clase base para la página de historial de partidas.
/// Muestra todas las partidas jugadas con opciones para ver detalles o continuar jugando.
/// </summary>
public class PartidasBase : ComponentBase
{
    // ---------- INYECCIONES ----------

    /// <summary>
    /// Cliente HTTP para realizar peticiones al backend.
    /// </summary>
    [Inject]
    protected HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Servicio de navegación para cambiar de página.
    /// </summary>
    [Inject]
    protected NavigationManager Nav { get; set; } = default!;

    /// <summary>
    /// Servicio de notificaciones de MudBlazor.
    /// </summary>
    [Inject]
    protected ISnackbar Snackbar { get; set; } = default!;

    // ---------- ESTADO DEL COMPONENTE ----------

    /// <summary>
    /// Lista de partidas cargadas desde el backend.
    /// </summary>
    protected List<PartidaDto> Partidas { get; set; } = new();

    /// <summary>
    /// Indica si se están cargando los datos.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    // ---------- ESTADÍSTICAS CALCULADAS ----------

    /// <summary>
    /// Cantidad de partidas finalizadas.
    /// </summary>
    protected int PartidasFinalizadas => Partidas.Count(p => p.Estado == "FINALIZADA");

    /// <summary>
    /// Cantidad de partidas en curso.
    /// </summary>
    protected int PartidasEnCurso => Partidas.Count(p => p.Estado == "EN_CURSO");

    // ---------- CICLO DE VIDA ----------

    /// <summary>
    /// Método llamado al inicializar el componente.
    /// Carga la lista de partidas desde el backend.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await CargarPartidas();
    }

    // ---------- MÉTODOS PRINCIPALES ----------

    /// <summary>
    /// Obtiene todas las partidas desde el backend.
    /// Endpoint esperado: GET api/partidas/completas
    /// </summary>
    protected async Task CargarPartidas()
    {
        try
        {
            IsLoading = true;

            // Ajuste: llamar al endpoint real GET api/partidas
            var response = await Http.GetAsync("api/partidas");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"No se pudo cargar el historial de partidas. " +
                             $"Status: {(int)response.StatusCode} - {response.ReasonPhrase}", Severity.Error);
                Console.WriteLine($"[Historial] Error {(int)response.StatusCode}: {body}");
                return;
            }

            // Deserializa directamente lo que devuelve tu API (Partida)
            var partidasRaw = await response.Content.ReadFromJsonAsync<List<PartidaBackendDto>>() ?? new();

            // Mapea al DTO que usa la UI
            Partidas = partidasRaw
                .Select(p => new PartidaDto
                {
                    PartidaId = p.PartidaId,
                    Estado = p.Estado,
                    // si todavía no tienes nombres de jugadores en la entidad,
                    // por ahora solo muestra los ids como texto
                    NombreJugador1 = p.UsuarioId_Jugador1.ToString(),
                    NombreJugador2 = p.UsuarioId_Jugador2.ToString(),
                    // si manejas UsuarioId_Ganador, lo puedes mapear a texto simple
                    NombreGanador = p.UsuarioId_Ganador.HasValue
                                      ? p.UsuarioId_Ganador.Value.ToString()
                                      : null,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin
                })
                .OrderByDescending(p => p.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar las partidas: {ex.Message}", Severity.Error);
            Console.WriteLine($"[Historial] Excepción: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navega a la página de detalles de una partida finalizada.
    /// </summary>
    /// <param name="partidaId">ID de la partida</param>
    protected void VerDetalles(int partidaId)
    {
        Nav.NavigateTo($"/partida/{partidaId}/detalles");
    }

    /// <summary>
    /// Navega a la página de juego para continuar una partida en curso.
    /// </summary>
    /// <param name="partidaId">ID de la partida</param>
    protected void ContinuarJugando(int partidaId)
    {
        Nav.NavigateTo($"/partida/{partidaId}/jugar");
    }

    // ---------- DTOs ----------

    /// <summary>
    /// DTO que representa la entidad Partida tal como la devuelve el backend.
    /// (propiedades deben coincidir con tu clase Partida del modelo EF).
    /// </summary>
    public class PartidaBackendDto
    {
        public int PartidaId { get; set; }
        public int UsuarioId_Jugador1 { get; set; }
        public int UsuarioId_Jugador2 { get; set; }
        public int? UsuarioId_Ganador { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    /// <summary>
    /// DTO que representa una partida en el historial.
    /// </summary>
    public class PartidaDto
    {
        /// <summary>
        /// Identificador único de la partida.
        /// </summary>
        public int PartidaId { get; set; }

        /// <summary>
        /// Estado de la partida (EN_CURSO, FINALIZADA).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del Jugador 1.
        /// </summary>
        public string NombreJugador1 { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del Jugador 2.
        /// </summary>
        public string NombreJugador2 { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del ganador (null si hay empate o está en curso).
        /// </summary>
        public string? NombreGanador { get; set; }

        /// <summary>
        /// Fecha de inicio de la partida.
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de finalización (null si está en curso).
        /// </summary>
        public DateTime? FechaFin { get; set; }
    }
}