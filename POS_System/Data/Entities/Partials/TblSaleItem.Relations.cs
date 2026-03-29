namespace POS_System.Data.Entities;

public partial class TblSaleItem
{
    public virtual TblProduct? Product { get; set; }

    public virtual TblSale? Sale { get; set; }
}
