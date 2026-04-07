using System.Data;

namespace EMedicineBE.Data.Repositories
{
    public interface IInvoiceRepository
    {
        DataSet GetInvoiceData(int userId, int orderId);
    }
}
