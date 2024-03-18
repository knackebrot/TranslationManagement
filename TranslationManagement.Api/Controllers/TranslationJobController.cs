using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using External.ThirdParty.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Domain.Models;

namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/jobs/[action]")]
    public class TranslationJobController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TranslationJobController> _logger;

        public TranslationJobController(IServiceScopeFactory scopeFactory, ILogger<TranslationJobController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            return Ok(await _context.TranslationJobs.ToListAsync());
        }

        [HttpPost]
        public bool CreateJob(TranslationJob job)
        {
            job.Status = JobStatus.New;
            SetPrice(job);
            _context.TranslationJobs.Add(job);
            bool success = _context.SaveChanges() > 0;
            if (success)
            {
                var notificationSvc = new UnreliableNotificationService();
                while (!notificationSvc.SendNotification("Job created: " + job.Id).Result)
                {
                }

                _logger.LogInformation("New job notification sent");
            }

            return success;
        }

        [HttpPost]
        public bool CreateJobWithFile(IFormFile file, string customer)
        {
            var reader = new StreamReader(file.OpenReadStream());
            string content;


            if (file.FileName.EndsWith(".txt"))
            {
                content = reader.ReadToEnd();
            }
            else if (file.FileName.EndsWith(".xml"))
            {
                var xdoc = XDocument.Parse(reader.ReadToEnd());
                content = xdoc.Root.Element("Content").Value;
                customer = xdoc.Root.Element("Customer").Value.Trim();
            }
            else
            {
                throw new NotSupportedException("unsupported file");
            }

            var newJob = new TranslationJob()
            {
                OriginalContent = content,
                TranslatedContent = "",
                CustomerName = customer,
            };

            SetPrice(newJob);

            return CreateJob(newJob);
        }

        [HttpPost]
        public string UpdateJobStatus(int jobId, int translatorId, string newStatus = "")
        {
            _logger.LogInformation("Job status update request received: {newStatus} for job {jobId} by translator {translatorId}", newStatus, jobId, translatorId);
            if (typeof(JobStatus).GetProperties().Any(prop => prop.Name == newStatus))
            {
                return "invalid status";
            }

            var job = _context.TranslationJobs.Single(j => j.Id == jobId);

            bool isInvalidStatusChange = (job.Status == JobStatus.New && newStatus == JobStatus.Completed) ||
                                         job.Status == JobStatus.Completed || newStatus == JobStatus.New;
            if (isInvalidStatusChange)
            {
                return "invalid status change";
            }

            job.Status = newStatus;
            _context.SaveChanges();
            return "updated";
        }

        private static void SetPrice(TranslationJob job)
        {
            const decimal PricePerCharacter = 0.01M;
            job.Price = job.OriginalContent.Length * PricePerCharacter;
        }
    }
}