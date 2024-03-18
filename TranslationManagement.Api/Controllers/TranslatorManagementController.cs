using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Domain.Models;

namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/TranslatorsManagement/[action]")]
    public class TranslatorManagementController : ControllerBase
    {
        private readonly ILogger<TranslatorManagementController> _logger;
        private readonly AppDbContext _context;

        public TranslatorManagementController(IServiceScopeFactory scopeFactory, ILogger<TranslatorManagementController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTranslators()
        {
            return Ok(await _context.Translators.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetTranslatorsByName(string name)
        {
            return Ok(await _context.Translators.Where(t => t.Name == name).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddTranslator(Translator translator)
        {
            await _context.Translators.AddAsync(translator);
            return CreatedAtAction(nameof(GetTranslatorsByName), new {translator.Id}, translator);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTranslatorStatus(int translatorId, string newStatus = "")
        {
            _logger.LogInformation("User status update request: {newStatus} for user {translatorId}", newStatus, translatorId);

            if (typeof(TranslatorStatus).GetProperties().Any(prop => prop.Name == newStatus))
            {
                return BadRequest("unknown status");
            }

            var translator = await  _context.Translators.FirstOrDefaultAsync(t => t.Id == translatorId);

            if (translator == default)
            {
                return NotFound();
            }

            translator.Status = newStatus;
            _context.SaveChanges();

            return NoContent();
        }
    }
}