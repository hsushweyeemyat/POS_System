namespace POS_System.Data.Entities;

public partial class TblCategory
{
    public virtual ICollection<TblProduct> Products { get; set; } = new List<TblProduct>();
}
