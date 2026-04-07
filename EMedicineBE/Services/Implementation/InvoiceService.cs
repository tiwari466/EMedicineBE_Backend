using EMedicineBE.Data.Repositories;
using System.Data;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repo;

    public InvoiceService(IInvoiceRepository repo)
    {
        _repo = repo;
    }

    public DataSet GetInvoice(int userId, int orderId)
        => _repo.GetInvoiceData(userId, orderId);
}
