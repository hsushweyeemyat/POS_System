namespace POS_System.Data.Entities;

public partial class TblProduct
{
    public virtual TblCategory? Category { get; set; }

    public virtual ICollection<TblSaleItem> SaleItems { get; set; } = new List<TblSaleItem>();
}
