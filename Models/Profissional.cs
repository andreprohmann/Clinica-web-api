using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    public class Profissional
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Registro { get; set; } = string.Empty;
        public Especialidade Especialidade { get; set; }

        [JsonIgnore]
        public ICollection<Consulta>? Consultas { get; set; }
    }
}
