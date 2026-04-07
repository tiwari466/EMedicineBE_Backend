using EMedicineBE.Models;
using EMedicineBE.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoiceController(IInvoiceService service)
        {
            _service = service;
        }


        [HttpGet("downloadInvoice")]
       public IActionResult DownloadInvoice(int userId, int orderId)
        => Ok(_service.GetInvoice(userId, orderId));
    }
}
