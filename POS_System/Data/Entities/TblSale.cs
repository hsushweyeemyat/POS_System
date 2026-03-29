namespace POS_System.Data.Entities;

public partial class TblSale
{
    public long Id { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public DateTime SaleDate { get; set; }

    public int TotalAmount { get; set; }

    public int UserId { get; set; }

    public string CashierName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
