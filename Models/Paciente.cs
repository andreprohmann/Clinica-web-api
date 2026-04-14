using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // A classe Paciente é uma entidade na base de dados (Reflete uma tabela "Pacientes")
    public class Paciente
    {
        // Chave Secundária / ID autoincrementado do banco de dados
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string CPF { get; set; } = string.Empty;
        
        // Propriedade Computada: Retorna a idade baseada na DataNascimento.
        // Ela não existe no banco de dados como coluna, é montada sempre que solicitada pelo sistema.
        public int Idade
        {
            get
            {
                var today = DateTime.Today; // Pega o dia de hoje
                var age = today.Year - DataNascimento.Year; // Diferença em anos
                
                // Se a data do nascimento neste ano ainda for maior que hoje (ou seja, ainda não fez aniversário)
                // Subtrai-se 1 da idade.
                if (DataNascimento.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        // Relacionamento (1 para N): Um Paciente pode ter recebido N Consultas
        // JsonIgnore oculta esta lista na hora de mostrar o resultado pro usuário e evita referência circular infinita.
        [JsonIgnore]
        public ICollection<Consulta>? Consultas { get; set; }
    }
}
