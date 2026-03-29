namespace POS_System.Data.Entities;

public partial class TblSaleItem
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int Qty { get; set; }

    public int Price { get; set; }
}
