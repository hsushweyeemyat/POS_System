namespace POS_System.Data.Entities;

public partial class TblSale
{
    public virtual ICollection<TblSaleItem> SaleItems { get; set; } = new List<TblSaleItem>();

    public virtual TblUser? User { get; set; }
}
