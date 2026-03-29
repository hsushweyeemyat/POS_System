namespace POS_System.Data.Entities;

public partial class TblCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public byte IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
