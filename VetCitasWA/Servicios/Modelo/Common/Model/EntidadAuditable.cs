using System;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.Modelo.Common.Model
{
    public abstract class EntidadAuditable
    {
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Usuario.Usuario ModifiedBy { get; set; }
    }
}