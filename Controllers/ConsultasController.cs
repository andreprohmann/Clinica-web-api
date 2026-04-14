using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Models;
using ClinicaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaApi.Controllers
{
    // Define a rota base da API para este controlador. Ex: https://localhost:porta/api/Consultas
    [Route("api/[controller]")]
    // Indica que a classe é um controlador para responder requisições Web/APIs (Retorna JSON)
    [ApiController]
    public class ConsultasController : ControllerBase
    {
        private readonly IConsultaRepository _repository;

        // Injeção de dependência do Repositório
        public ConsultasController(IConsultaRepository repository)
        {
            _repository = repository;
        }

        // Rota [GET] api/consultas (Lista todas as consultas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consulta>>> GetConsultas()
        {
            var consultas = await _repository.GetAllAsync();
            return Ok(consultas); // HTTP 200 OK
        }

        // Rota [GET] api/consultas/5 (Busca consulta por ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Consulta>> GetConsulta(int id)
        {
            var consulta = await _repository.GetByIdAsync(id);
            if (consulta == null) return NotFound(); // HTTP 404 Not Found se não existir
            return Ok(consulta); // HTTP 200 OK
        }

        // Rota [POST] api/consultas (Criar nova consulta)
        [HttpPost]
        public async Task<ActionResult<Consulta>> PostConsulta(Consulta consulta)
        {
            if (consulta == null) return BadRequest("Dados inválidos.");
            
            // VERIFICAÇÃO DA REGRA DE NEGÓCIO: Conflito de agenda
            bool conflito = await _repository.ExisteConflitoHorarioAsync(consulta.ProfissionalId, consulta.PacienteId, consulta.DataHora);
            if (conflito)
            {
                // Se der conflito, rejeita e não salva no banco (HTTP 400 Bad Request)
                return BadRequest("Conflito de horário! O profissional ou paciente já possui consulta agendada neste horário.");
            }

            // Se a agenda estiver livre, salva.
            await _repository.AddAsync(consulta);
            
            // Retorna HTTP 201 Created apontando o link de onde achar a nova consulta criada
            return CreatedAtAction(nameof(GetConsulta), new { id = consulta.Id }, consulta);
        }

        // Rota [PUT] api/consultas/5 (Atualiza consulta existente)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsulta(int id, Consulta consulta)
        {
            // Segurança básica: o ID da URL deve ser o mesmo do objeto enviado no JSON
            if (id != consulta.Id) return BadRequest("IDs não conferem.");
            
            var existente = await _repository.GetByIdAsync(id);
            if (existente == null) return NotFound();

            // Se estão tentando mudar a data/hora ou o médico, verifica o conflito de agenda novamente!
            if (existente.DataHora != consulta.DataHora || existente.ProfissionalId != consulta.ProfissionalId)
            {
                bool conflito = await _repository.ExisteConflitoHorarioAsync(consulta.ProfissionalId, consulta.PacienteId, consulta.DataHora);
                if (conflito)
                {
                    return BadRequest("Conflito de horário! O profissional ou paciente já possui consulta agendada neste novo horário.");
                }
            }

            // Atualiza os campos em memória
            existente.DataHora = consulta.DataHora;
            existente.ProfissionalId = consulta.ProfissionalId;
            existente.PacienteId = consulta.PacienteId;
            existente.Status = consulta.Status;

            // Manda salvar as alterações
            await _repository.UpdateAsync(existente);
            return NoContent(); // HTTP 204 No Content (Deu certo, mas não tenho texto pra devolver)
        }

        // Rota [DELETE] api/consultas/5 (Apagar)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsulta(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent(); // Retorna 204
        }
    }
}
