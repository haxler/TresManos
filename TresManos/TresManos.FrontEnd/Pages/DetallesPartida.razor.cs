using System.Net.Http.Json;         // Extensiones para enviar/recibir JSON con HttpClient
using Microsoft.AspNetCore.Components; // Tipos base de Blazor (ComponentBase, Parameter, Inject)
using MudBlazor;                     // Componentes y tipos de MudBlazor (ISnackbar, Color, Icons)

namespace TresManos.FrontEnd.Pages
{
    /// <summary>
    /// Clase base (code-behind) para la página DetallesPartida.razor.
    /// Contiene toda la lógica para cargar la partida, mostrar rondas y manejar la revancha.
    /// </summary>
    public class DetallesPartidaBase : ComponentBase
    {
        // ---------- PARÁMETROS E INYECCIONES ----------

        /// <summary>
        /// Id de la partida recibido desde la ruta (definido en @page).
        /// Ejemplo: /partida/5/detalles  ->  PartidaId = 5
        /// </summary>
        [Parameter]
        public int PartidaId { get; set; }

        /// <summary>
        /// Cliente HTTP inyectado, configurado en Program.cs con la BaseAddress del backend.
        /// Se usa para llamar a la API.
        /// </summary>
        [Inject]
        protected HttpClient Http { get; set; } = default!;

        /// <summary>
        /// Servicio de navegación de Blazor para cambiar de página por código.
        /// </summary>
        [Inject]
        protected NavigationManager Nav { get; set; } = default!;

        /// <summary>
        /// Servicio de MudBlazor para mostrar notificaciones tipo Snackbar.
        /// </summary>
        [Inject]
        protected ISnackbar Snackbar { get; set; } = default!;

        // ---------- ESTADO DEL COMPONENTE ----------

        /// <summary>
        /// Información detallada de la partida cargada desde el backend.
        /// Puede ser null si hay error al cargar.
        /// </summary>
        protected PartidaDetalleDto? Partida { get; set; }

        /// <summary>
        /// Lista de rondas asociadas a la partida.
        /// </summary>
        protected List<RondaDto> Rondas { get; set; } = new();

        /// <summary>
        /// Indica si se están cargando los datos de la API.
        /// </summary>
        protected bool IsLoading { get; set; } = true;

        /// <summary>
        /// Indica si se está procesando la creación de una revancha.
        /// </summary>
        protected bool IsRevanchaLoading { get; set; } = false;

        // ---------- ESTADÍSTICAS CALCULADAS ----------

        /// <summary>
        /// Cantidad de rondas ganadas por el Jugador 1.
        /// Se cuenta cuántas rondas tienen Resultado == "1".
        /// </summary>
        protected int RondasGanadasJugador1 => Rondas.Count(r => r.Resultado == "1");

        /// <summary>
        /// Cantidad de rondas ganadas por el Jugador 2.
        /// Se cuenta cuántas rondas tienen Resultado == "2".
        /// </summary>
        protected int RondasGanadasJugador2 => Rondas.Count(r => r.Resultado == "2");

        /// <summary>
        /// Cantidad de rondas empatadas (Resultado == "E").
        /// </summary>
        protected int RondasEmpatadas => Rondas.Count(r => r.Resultado == "E");

        /// <summary>
        /// Determina si el botón de revancha debe estar deshabilitado.
        /// Está deshabilitado si:
        ///  - Se está creando una revancha (IsRevanchaLoading == true), o
        ///  - La partida no está FINALIZADA.
        /// </summary>
        protected bool BotonRevanchaDeshabilitado =>
            IsRevanchaLoading || Partida?.Estado != "FINALIZADA";

        // ---------- CICLO DE VIDA DEL COMPONENTE ----------

        /// <summary>
        /// Método llamado automáticamente por Blazor cuando el componente se inicializa.
        /// Aquí disparamos la carga inicial de los datos.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await CargarPartida();
        }

        // ---------- MÉTODOS PRINCIPALES ----------

        /// <summary>
        /// Llama al backend para obtener los datos de la partida y sus rondas.
        /// Actualiza las propiedades Partida y Rondas.
        /// </summary>
        protected async Task CargarPartida()
        {
            try
            {
                // Activamos el indicador de carga
                IsLoading = true;

                // 1) Obtener información detallada de la partida
                // Endpoint esperado en el backend: GET api/partidas/{id}/completa
                var respPartida = await Http.GetAsync($"api/partidas/{PartidaId}/completa");

                if (!respPartida.IsSuccessStatusCode)
                {
                    // Si la respuesta no fue exitosa, mostramos un mensaje de error
                    Snackbar.Add("No se pudo cargar la información de la partida.", Severity.Error);
                    return;
                }

                // Deserializamos el JSON en un PartidaDetalleDto
                Partida = await respPartida.Content.ReadFromJsonAsync<PartidaDetalleDto>();

                // 2) Obtener las rondas de la partida
                // Endpoint esperado en el backend: GET api/rondas/partida/{partidaId}
                var respRondas = await Http.GetAsync($"api/rondas/partida/{PartidaId}");

                if (respRondas.IsSuccessStatusCode)
                {
                    // Si la respuesta es OK, deserializamos la lista de rondas
                    Rondas = await respRondas.Content.ReadFromJsonAsync<List<RondaDto>>() ?? new();
                }
            }
            catch (Exception ex)
            {
                // Cualquier excepción se muestra como error en Snackbar
                Snackbar.Add($"Error al cargar los datos de la partida: {ex.Message}", Severity.Error);
            }
            finally
            {
                // Siempre desactivamos el indicador de carga
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navega a la página que muestra la lista de partidas.
        /// Ruta: /partidas
        /// </summary>
        protected void VolverAPartidas()
        {
            Nav.NavigateTo("/partidas");
        }

        /// <summary>
        /// Crea una nueva partida a modo de revancha, usando los mismos jugadores,
        /// y navega automáticamente a la pantalla de juego de la nueva partida.
        /// </summary>
        protected async Task IniciarRevancha()
        {
            // Validación: necesitamos tener la información de la partida original
            if (Partida is null)
            {
                Snackbar.Add("No hay datos de la partida para crear una revancha.", Severity.Error);
                return;
            }

            try
            {
                // Indicamos que la revancha se está procesando
                IsRevanchaLoading = true;

                // ----- Opción principal: endpoint específico de revancha -----
                // Endpoint esperado en backend: POST api/partidas/{id}/revancha
                var response = await Http.PostAsync($"api/partidas/{Partida.PartidaId}/revancha", null);

                // ----- Opción alternativa (si no tienes el endpoint de revancha) -----
                // Descomenta este bloque y comenta el POST anterior:
                /*
                var nuevaPartidaRequest = new PartidaCreateDto
                {
                    UsuarioId_Jugador1 = Partida.UsuarioId_Jugador1,
                    UsuarioId_Jugador2 = Partida.UsuarioId_Jugador2
                };
                var response = await Http.PostAsJsonAsync("api/partidas", nuevaPartidaRequest);
                */

                // Si la respuesta no fue exitosa, mostramos el mensaje devuelto por la API
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"No se pudo crear la revancha: {error}", Severity.Error);
                    return;
                }

                // Deserializamos la nueva partida creada
                var nuevaPartida = await response.Content.ReadFromJsonAsync<PartidaDto>();

                if (nuevaPartida is null)
                {
                    Snackbar.Add("No se pudo leer la partida de revancha creada.", Severity.Error);
                    return;
                }

                // Mostramos mensaje de éxito
                Snackbar.Add("¡Revancha creada! A jugar...", Severity.Success);

                // Navegamos directamente a la pantalla de juego de la nueva partida
                Nav.NavigateTo($"/partida/{nuevaPartida.PartidaId}/jugar");
            }
            catch (Exception ex)
            {
                // Cualquier error inesperado se notifica
                Snackbar.Add($"Error al crear la revancha: {ex.Message}", Severity.Error);
            }
            finally
            {
                // Desactivamos el estado de "cargando revancha"
                IsRevanchaLoading = false;
            }
        }

        // ---------- MÉTODOS DE AYUDA PARA LA PRESENTACIÓN ----------

        /// <summary>
        /// Devuelve el nombre legible de un movimiento según su código.
        /// P = Piedra, A = Papel, T = Tijera.
        /// </summary>
        protected string ObtenerNombreMovimiento(string movimiento) => movimiento switch
        {
            "P" => "Piedra",
            "A" => "Papel",
            "T" => "Tijera",
            _ => "Desconocido"
        };

        /// <summary>
        /// Devuelve un color de MudBlazor asociado a cada movimiento.
        /// Se usa para colorear los chips en la tabla de rondas.
        /// </summary>
        protected Color ObtenerColorMovimiento(string movimiento) => movimiento switch
        {
            "P" => Color.Error,    // Rojo para Piedra
            "A" => Color.Info,     // Azul para Papel
            "T" => Color.Warning,  // Amarillo para Tijera
            _ => Color.Default
        };

        /// <summary>
        /// Devuelve el icono Material Design asociado a cada movimiento.
        /// </summary>
        protected string ObtenerIconoMovimiento(string movimiento) => movimiento switch
        {
            "P" => Icons.Material.Filled.Circle,       // Icono para Piedra
            "A" => Icons.Material.Filled.Description,  // Icono para Papel
            "T" => Icons.Material.Filled.ContentCut,   // Icono para Tijera
            _ => Icons.Material.Filled.Help
        };

        // ---------- DTOs USADOS POR EL FRONTEND ----------

        /// <summary>
        /// DTO con la información detallada de una partida
        /// que el backend devuelve al frontend.
        /// </summary>
        public class PartidaDetalleDto
        {
            /// <summary>
            /// Identificador único de la partida.
            /// </summary>
            public int PartidaId { get; set; }

            /// <summary>
            /// Estado de la partida (EN_CURSO, FINALIZADA, etc.).
            /// </summary>
            public string Estado { get; set; } = string.Empty;

            /// <summary>
            /// Id del usuario que juega como Jugador 1.
            /// </summary>
            public int UsuarioId_Jugador1 { get; set; }

            /// <summary>
            /// Id del usuario que juega como Jugador 2.
            /// </summary>
            public int UsuarioId_Jugador2 { get; set; }

            /// <summary>
            /// Nombre del Jugador 1.
            /// </summary>
            public string NombreJugador1 { get; set; } = string.Empty;

            /// <summary>
            /// Nombre del Jugador 2.
            /// </summary>
            public string NombreJugador2 { get; set; } = string.Empty;

            /// <summary>
            /// Nombre del ganador de la partida.
            /// Puede ser null si hubo empate.
            /// </summary>
            public string? NombreGanador { get; set; }

            /// <summary>
            /// Fecha y hora en que se inició la partida.
            /// </summary>
            public DateTime FechaInicio { get; set; }

            /// <summary>
            /// Fecha y hora en que se finalizó la partida (si ya terminó).
            /// Puede ser null si la partida aún está en curso.
            /// </summary>
            public DateTime? FechaFin { get; set; }
        }

        /// <summary>
        /// DTO que representa una ronda de la partida.
        /// </summary>
        public class RondaDto
        {
            /// <summary>
            /// Identificador único de la ronda.
            /// </summary>
            public int RondaId { get; set; }

            /// <summary>
            /// Id de la partida a la que pertenece esta ronda.
            /// </summary>
            public int PartidaId { get; set; }

            /// <summary>
            /// Número de ronda (1, 2, 3, ...).
            /// </summary>
            public int NumeroRonda { get; set; }

            /// <summary>
            /// Movimiento del Jugador 1 (P, A, T).
            /// </summary>
            public string MovimientoJugador1 { get; set; } = string.Empty;

            /// <summary>
            /// Movimiento del Jugador 2 (P, A, T).
            /// </summary>
            public string MovimientoJugador2 { get; set; } = string.Empty;

            /// <summary>
            /// Resultado de la ronda: 
            /// "1" gana jugador 1, "2" gana jugador 2, "E" empate.
            /// </summary>
            public string Resultado { get; set; } = string.Empty;

            /// <summary>
            /// Momento en que se creó / jugó esta ronda.
            /// </summary>
            public DateTime FechaCreacion { get; set; }
        }

        /// <summary>
        /// DTO usado para crear una nueva partida (por ejemplo, revancha).
        /// </summary>
        public class PartidaCreateDto
        {
            /// <summary>
            /// Id del Jugador 1 para la nueva partida.
            /// </summary>
            public int UsuarioId_Jugador1 { get; set; }

            /// <summary>
            /// Id del Jugador 2 para la nueva partida.
            /// </summary>
            public int UsuarioId_Jugador2 { get; set; }
        }

        /// <summary>
        /// DTO que representa una partida sencilla devuelta por la API.
        /// (Usado cuando se crea una nueva partida).
        /// </summary>
        public class PartidaDto
        {
            /// <summary>
            /// Id de la partida.
            /// </summary>
            public int PartidaId { get; set; }

            /// <summary>
            /// Id del Jugador 1.
            /// </summary>
            public int UsuarioId_Jugador1 { get; set; }

            /// <summary>
            /// Id del Jugador 2.
            /// </summary>
            public int UsuarioId_Jugador2 { get; set; }

            /// <summary>
            /// Estado de la partida.
            /// </summary>
            public string Estado { get; set; } = string.Empty;

            /// <summary>
            /// Fecha de inicio de la partida.
            /// </summary>
            public DateTime FechaInicio { get; set; }
        }
    }
}