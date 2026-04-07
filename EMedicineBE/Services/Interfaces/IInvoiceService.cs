using System.Data;

public interface IInvoiceService
{
    DataSet GetInvoice(int userId, int orderId);
}
