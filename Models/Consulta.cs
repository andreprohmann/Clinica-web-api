using System;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    public class Consulta
    {
        public int Id { get; set; }
        
        public int PacienteId { get; set; }
        [JsonIgnore]
        public Paciente? Paciente { get; set; }

        public int ProfissionalId { get; set; }
        [JsonIgnore]
        public Profissional? Profissional { get; set; }

        public DateTime DataHora { get; set; }
        public StatusConsulta Status { get; set; }
    }
}
