using EMedicineBE.Models;
using EMedicineBE.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


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
        public IActionResult DownloadInvoice(
            [FromQuery(Name = "user_id")] int userId,
            [FromQuery(Name = "order_id")] int orderId)
        {
            var ds = _service.GetInvoice(userId, orderId);

            if (ds == null || ds.Tables.Count == 0)
                return BadRequest("No data found");

            var header = ds.Tables["InvoiceHeader"].Rows[0];
            var items = ds.Tables["InvoiceItems"].Rows;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("INVOICE").FontSize(20).Bold().AlignCenter();

                        col.Item().Text($"Order No: {header["order_no"]}");
                        col.Item().Text($"Customer: {header["first_name"]} {header["last_name"]}");
                        col.Item().Text($"Email: {header["email"]}");
                        col.Item().Text($"Date: {header["placed_time"]}");

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Items").Bold();

                        foreach (DataRow row in items)
                        {
                            col.Item().Text(
                                $"{row["medicine_name"]} | Qty: {row["qty"]} | ₹{row["total_price"]}"
                            );
                        }

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Total: ₹{header["order_total"]}")
                            .FontSize(16)
                            .Bold();
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Invoice_{orderId}.pdf");
        }
    }
}
