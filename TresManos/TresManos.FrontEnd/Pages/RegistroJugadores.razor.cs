using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TresManos.FrontEnd.Pages;

/// <summary>
/// Clase base para el componente de registro de jugadores.
/// Maneja la lógica de creación de usuarios y partidas.
/// </summary>
public class RegistroJugadoresBase : ComponentBase
{
    /// <summary>
    /// Cliente HTTP para realizar peticiones al backend.
    /// </summary>
    [Inject] protected HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Servicio de navegación para redirigir entre páginas.
    /// </summary>
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    /// <summary>
    /// Servicio de notificaciones de MudBlazor para mostrar mensajes al usuario.
    /// </summary>
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    /// <summary>
    /// Nombre del primer jugador ingresado en el formulario.
    /// </summary>
    protected string Jugador1Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del segundo jugador ingresado en el formulario.
    /// </summary>
    protected string Jugador2Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el formulario está en proceso de envío (para deshabilitar botones).
    /// </summary>
    protected bool IsSubmitting { get; set; } = false;

    /// <summary>
    /// Mensaje de retroalimentación para mostrar al usuario (opcional, si no usas Snackbar).
    /// </summary>
    protected string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Color del mensaje de retroalimentación (opcional).
    /// </summary>
    protected Color MensajeColor { get; set; } = Color.Default;

    /// <summary>
    /// Método que se ejecuta al enviar el formulario.
    /// Crea dos usuarios y una partida, luego redirige a la pantalla de juego.
    /// </summary>
    protected async Task OnValidSubmit()
    {
        // Validación 1: Verificar que ambos nombres estén completos
        if (string.IsNullOrWhiteSpace(Jugador1Nombre) || string.IsNullOrWhiteSpace(Jugador2Nombre))
        {
            Snackbar.Add("Ambos jugadores deben tener un nombre", Severity.Warning);
            return;
        }

        // Validación 2: Verificar que los nombres sean diferentes
        if (Jugador1Nombre.Trim().Equals(Jugador2Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add("Los nombres de los jugadores deben ser diferentes", Severity.Warning);
            return;
        }

        try
        {
            // Activar indicador de carga
            IsSubmitting = true;
            Mensaje = string.Empty;

            // ========== PASO 1: Crear Jugador 1 ==========

            // Crear el DTO con el nombre del jugador 1
            var usuario1 = new UsuarioCreateDto { NombreUsuario = Jugador1Nombre.Trim() };

            // Enviar petición POST al endpoint de usuarios
            var responseUsuario1 = await Http.PostAsJsonAsync("api/usuarios", usuario1);

            // Verificar si la petición fue exitosa
            if (!responseUsuario1.IsSuccessStatusCode)
            {
                // Leer el mensaje de error del servidor
                var error = await responseUsuario1.Content.ReadAsStringAsync();
                Snackbar.Add($"Error al crear Jugador 1: {error}", Severity.Error);
                return;
            }

            // Deserializar la respuesta para obtener el usuario creado
            var jugador1Creado = await responseUsuario1.Content.ReadFromJsonAsync<UsuarioDto>();

            // ========== PASO 2: Crear Jugador 2 ==========

            // Crear el DTO con el nombre del jugador 2
            var usuario2 = new UsuarioCreateDto { NombreUsuario = Jugador2Nombre.Trim() };

            // Enviar petición POST al endpoint de usuarios
            var responseUsuario2 = await Http.PostAsJsonAsync("api/usuarios", usuario2);

            // Verificar si la petición fue exitosa
            if (!responseUsuario2.IsSuccessStatusCode)
            {
                // Leer el mensaje de error del servidor
                var error = await responseUsuario2.Content.ReadAsStringAsync();
                Snackbar.Add($"Error al crear Jugador 2: {error}", Severity.Error);
                return;
            }

            // Deserializar la respuesta para obtener el usuario creado
            var jugador2Creado = await responseUsuario2.Content.ReadFromJsonAsync<UsuarioDto>();

            // ========== PASO 3: Validar que ambos usuarios se crearon correctamente ==========

            if (jugador1Creado == null || jugador2Creado == null)
            {
                Snackbar.Add("Error al obtener los datos de los jugadores creados", Severity.Error);
                return;
            }

            // ========== PASO 4: Crear la Partida ==========

            // Crear el DTO de la partida con los IDs de ambos jugadores
            var nuevaPartida = new PartidaCreateDto
            {
                UsuarioId_Jugador1 = jugador1Creado.UsuarioId,
                UsuarioId_Jugador2 = jugador2Creado.UsuarioId
            };

            // Enviar petición POST al endpoint de partidas
            var responsePartida = await Http.PostAsJsonAsync("api/partidas", nuevaPartida);

            // Verificar si la petición fue exitosa
            if (!responsePartida.IsSuccessStatusCode)
            {
                // Leer el mensaje de error del servidor
                var error = await responsePartida.Content.ReadAsStringAsync();
                Snackbar.Add($"Error al crear la partida: {error}", Severity.Error);
                return;
            }

            // Deserializar la respuesta para obtener la partida creada
            var partidaCreada = await responsePartida.Content.ReadFromJsonAsync<PartidaDto>();

            // Validar que la partida se creó correctamente
            if (partidaCreada == null)
            {
                Snackbar.Add("Error al obtener los datos de la partida creada", Severity.Error);
                return;
            }

            // ========== PASO 5: Mostrar mensaje de éxito ==========

            Snackbar.Add("¡Jugadores registrados y partida creada exitosamente!", Severity.Success);

            // ========== PASO 6: Redirigir a la pantalla de juego ==========

            Nav.NavigateTo($"/partida/{partidaCreada.PartidaId}/jugar");
        }
        catch (Exception ex)
        {
            // Capturar cualquier error inesperado y mostrarlo al usuario
            Snackbar.Add($"Error inesperado: {ex.Message}", Severity.Error);
        }
        finally
        {
            // Desactivar indicador de carga sin importar el resultado
            IsSubmitting = false;
        }
    }

    /// <summary>
    /// Convierte un Color de MudBlazor a un Severity para usar en alertas.
    /// </summary>
    /// <param name="color">Color de MudBlazor</param>
    /// <returns>Severity correspondiente</returns>
    protected Severity ColorToSeverity(Color color) => color switch
    {
        Color.Success => Severity.Success,
        Color.Error => Severity.Error,
        Color.Warning => Severity.Warning,
        Color.Info => Severity.Info,
        _ => Severity.Normal
    };

    // ========== DTOs (Data Transfer Objects) ==========

    /// <summary>
    /// DTO para crear un nuevo usuario.
    /// Debe coincidir con el DTO esperado por el backend.
    /// </summary>
    public class UsuarioCreateDto
    {
        /// <summary>
        /// Nombre de usuario único.
        /// IMPORTANTE: Debe llamarse "NombreUsuario" para coincidir con el backend.
        /// </summary>
        public string NombreUsuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO que representa un usuario devuelto por el backend.
    /// </summary>
    public class UsuarioDto
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de registro del usuario.
        /// </summary>
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva partida.
    /// </summary>
    public class PartidaCreateDto
    {
        /// <summary>
        /// ID del usuario que será el Jugador 1.
        /// </summary>
        public int UsuarioId_Jugador1 { get; set; }

        /// <summary>
        /// ID del usuario que será el Jugador 2.
        /// </summary>
        public int UsuarioId_Jugador2 { get; set; }
    }

    /// <summary>
    /// DTO que representa una partida devuelta por el backend.
    /// </summary>
    public class PartidaDto
    {
        /// <summary>
        /// Identificador único de la partida.
        /// </summary>
        public int PartidaId { get; set; }

        /// <summary>
        /// ID del Jugador 1.
        /// </summary>
        public int UsuarioId_Jugador1 { get; set; }

        /// <summary>
        /// ID del Jugador 2.
        /// </summary>
        public int UsuarioId_Jugador2 { get; set; }

        /// <summary>
        /// Estado actual de la partida (EN_CURSO, FINALIZADA, etc.).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de inicio de la partida.
        /// </summary>
        public DateTime FechaInicio { get; set; }
    }
}