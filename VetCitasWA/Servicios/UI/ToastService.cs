using System;

namespace VetCitasWA.Servicios.UI
{
    public enum ToastTipo
    {
        Exito,
        Error,
        Info
    }

    public class ToastMensaje
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Texto { get; set; } = "";
        public ToastTipo Tipo { get; set; } = ToastTipo.Info;
    }

    // Servicio compartido (scoped por circuito) para emitir toasts desde cualquier vista.
    public class ToastService
    {
        public event Action<ToastMensaje>? OnMostrar;

        public void Mostrar(string texto, ToastTipo tipo = ToastTipo.Info)
            => OnMostrar?.Invoke(new ToastMensaje { Texto = texto, Tipo = tipo });

        public void Exito(string texto) => Mostrar(texto, ToastTipo.Exito);
        public void Error(string texto) => Mostrar(texto, ToastTipo.Error);
        public void Info(string texto) => Mostrar(texto, ToastTipo.Info);
    }
}
