using System;
using VetCitasWA.Servicios.Modelo.Common.Model;

namespace VetCitasWA.Servicios.Modelo.Usuario
{
    public class HorarioVeterinario : EntidadAuditable
    {
        public int Id { get; set; }
        public Veterinario Veterinario { get; set; }
        public int DiaSemana { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public TimeOnly HoraDescansoInicio { get; set; }
        public TimeOnly HoraDescansoFin { get; set; }
        public bool Activo { get; set; }
    }
}