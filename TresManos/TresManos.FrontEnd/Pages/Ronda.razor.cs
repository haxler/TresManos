using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TresManos.FrontEnd.Pages;

public class RondaBase : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected int NumeroRonda { get; set; } = 1;
    protected string MovimientoJ1 { get; set; } = string.Empty;
    protected string MovimientoJ2 { get; set; } = string.Empty;

    protected bool IsSubmitting { get; set; }
    protected string Mensaje { get; set; } = string.Empty;
    protected Severity MensajeSeverity { get; set; } = Severity.Info;

    protected bool MostrarResultado { get; set; }
    protected string TextoResultado { get; set; } = string.Empty;
    protected Color ColorResultado { get; set; } = Color.Default;

    protected void JugarRonda()
    {
        // Validar que ambos jugadores hayan seleccionado
        if (string.IsNullOrWhiteSpace(MovimientoJ1) || string.IsNullOrWhiteSpace(MovimientoJ2))
        {
            Mensaje = "Ambos jugadores deben seleccionar un movimiento.";
            MensajeSeverity = Severity.Warning;
            MostrarResultado = false;
            return;
        }

        IsSubmitting = true;
        Mensaje = string.Empty;

        // Simular procesamiento
        Task.Delay(500).ContinueWith(_ =>
        {
            InvokeAsync(() =>
            {
                // Determinar ganador
                var resultado = DeterminarGanador(MovimientoJ1, MovimientoJ2);

                switch (resultado)
                {
                    case 0: // Empate
                        TextoResultado = "¡Empate!";
                        ColorResultado = Color.Warning;
                        Mensaje = "Es un empate. Jueguen otra ronda.";
                        MensajeSeverity = Severity.Info;
                        break;
                    case 1: // Gana J1
                        TextoResultado = "🏆 ¡Gana Jugador 1!";
                        ColorResultado = Color.Primary;
                        Mensaje = "Jugador 1 gana esta ronda.";
                        MensajeSeverity = Severity.Success;
                        break;
                    case 2: // Gana J2
                        TextoResultado = "🏆 ¡Gana Jugador 2!";
                        ColorResultado = Color.Secondary;
                        Mensaje = "Jugador 2 gana esta ronda.";
                        MensajeSeverity = Severity.Success;
                        break;
                }

                MostrarResultado = true;
                IsSubmitting = false;
                NumeroRonda++;

                // Limpiar selecciones para la siguiente ronda
                MovimientoJ1 = string.Empty;
                MovimientoJ2 = string.Empty;

                StateHasChanged();
            });
        });
    }

    /// <summary>
    /// Determina el ganador de la ronda
    /// </summary>
    /// <returns>0 = Empate, 1 = Gana J1, 2 = Gana J2</returns>
    private int DeterminarGanador(string mov1, string mov2)
    {
        if (mov1 == mov2) return 0; // Empate

        return (mov1, mov2) switch
        {
            ("Piedra", "Tijera") => 1,
            ("Papel", "Piedra") => 1,
            ("Tijera", "Papel") => 1,
            _ => 2
        };
    }

    protected string ObtenerEmoji(string movimiento) => movimiento switch
    {
        "Piedra" => "🪨",
        "Papel" => "📄",
        "Tijera" => "✂️",
        _ => ""
    };
}