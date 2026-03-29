namespace POS_System.ViewModels.Categories;

public class CategoryListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
}
