using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using System.Data;

namespace EMedicineBE.Pdf
{
    public static class InvoicePdfGenerator
    {
        public static byte[] Generate(DataSet ds)
        {
            using var ms = new MemoryStream();

            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            // ✅ SAFER: use table names
            var headerTable = ds.Tables["InvoiceHeader"];
            var itemsTable = ds.Tables["InvoiceItems"];

            if (headerTable == null || headerTable.Rows.Count == 0)
                throw new Exception("Invoice header not found");

            var orderRow = headerTable.Rows[0];

            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            doc.Add(
                new Paragraph("E-Medicine Invoice")
                    .SetFont(boldFont)
                    .SetFontSize(18)
            );

            doc.Add(new iText.Layout.Element.Paragraph($"Order No: {orderRow["order_no"]}"));
            doc.Add(new iText.Layout.Element.Paragraph($"Total: ₹ {orderRow["order_total"]}"));
            doc.Add(new iText.Layout.Element.Paragraph($"Status: {orderRow["order_status"]}"));
            doc.Add(new iText.Layout.Element.Paragraph(
                $"Placed On: {orderRow["placed_time"]}"
            ));

            doc.Add(new iText.Layout.Element.Paragraph("--------------------------------------------------"));

            foreach (DataRow item in itemsTable.Rows)
            {
                doc.Add(new iText.Layout.Element.Paragraph(
                    $"{item["medicine_name"]} | Qty: {item["qty"]} | Total: ₹ {item["total_price"]}"
                ));
            }

            doc.Close();
            return ms.ToArray();
        }
    }
}
