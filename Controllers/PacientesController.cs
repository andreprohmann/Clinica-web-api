using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Models;
using ClinicaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteRepository _repository;

        public PacientesController(IPacienteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paciente>>> GetPacientes()
        {
            var pacientes = await _repository.GetAllAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Paciente>> GetPaciente(int id)
        {
            var paciente = await _repository.GetByIdAsync(id);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        [HttpPost]
        public async Task<ActionResult<Paciente>> PostPaciente(Paciente paciente)
        {
            if (paciente == null) return BadRequest();
            await _repository.AddAsync(paciente);
            return CreatedAtAction(nameof(GetPaciente), new { id = paciente.Id }, paciente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaciente(int id, Paciente paciente)
        {
            if (id != paciente.Id) return BadRequest();
            await _repository.UpdateAsync(paciente);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaciente(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
