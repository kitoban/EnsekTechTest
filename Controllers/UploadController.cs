using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsekTechTest.Models;
using EnsekTechTest.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnsekTechTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private DatabaseContext _context;
        private UploadService _uploadService;

        public UploadController(ILogger<UploadController> logger, DatabaseContext context, UploadService uploadService)
        {
            _uploadService = uploadService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Account>> Get()
        {
            return await _context.Accounts.ToListAsync();
        }


        [HttpPost("UploadFile")]
        public async Task<IAsyncEnumerable<LineUploadReport>> UploadFile([FromForm] UploadFileData uploadFileData)
        {
            if (uploadFileData.File != null)
            {
                return _uploadService.ProcessFile(uploadFileData.File);
            }

            throw new InvalidOperationException("File missing from upload");
        }

        public class UploadFileData
        {
            public IFormFile File { set; get; }

        }
    }
}
