using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Models;
using ClinicaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfissionaisController : ControllerBase
    {
        private readonly IProfissionalRepository _repository;

        public ProfissionaisController(IProfissionalRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profissional>>> GetProfissionais()
        {
            var profissionais = await _repository.GetAllAsync();
            return Ok(profissionais);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Profissional>> GetProfissional(int id)
        {
            var profissional = await _repository.GetByIdAsync(id);
            if (profissional == null) return NotFound();
            return Ok(profissional);
        }

        [HttpPost]
        public async Task<ActionResult<Profissional>> PostProfissional(Profissional profissional)
        {
            if (profissional == null) return BadRequest();
            await _repository.AddAsync(profissional);
            return CreatedAtAction(nameof(GetProfissional), new { id = profissional.Id }, profissional);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfissional(int id, Profissional profissional)
        {
            if (id != profissional.Id) return BadRequest();
            await _repository.UpdateAsync(profissional);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfissional(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
