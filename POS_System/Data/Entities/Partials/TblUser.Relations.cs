namespace POS_System.Data.Entities;

public partial class TblUser
{
    public virtual ICollection<TblSale> Sales { get; set; } = new List<TblSale>();
}
