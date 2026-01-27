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
        private readonly IConfiguration _configuration;

        public InvoiceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("downloadInvoice")]
        public IActionResult DownloadInvoice(int user_id, int order_id)
        {
            string cs = _configuration.GetConnectionString("PostgresCS");
            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            DAL dal = new DAL();

            using var connection = new NpgsqlConnection(cs);
            var ds = dal.getInvoiceData(user_id, order_id, connection);

            if (ds.Tables.Count < 2 || ds.Tables["InvoiceHeader"].Rows.Count == 0)
                return BadRequest("Invoice not found");

            // Generate PDF bytes
            byte[] pdfBytes = InvoicePdfGenerator.Generate(ds);

            return File(pdfBytes, "application/pdf", $"Invoice_{order_id}.pdf");
        }
    }
}
