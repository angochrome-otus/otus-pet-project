namespace SteelDesignerEngineer.Domain.Entities
{
    public class PageContent
    {
        public Guid Id { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}