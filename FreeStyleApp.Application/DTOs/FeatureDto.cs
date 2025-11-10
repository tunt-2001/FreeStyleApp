namespace FreeStyleApp.Application.DTOs
{
    public class FeatureDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Icon { get; set; }
        public string FeatureGroupId { get; set; }
        public string FeatureGroupName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class FeatureCreateDto
    {
        public string Name { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Icon { get; set; }
        public string FeatureGroupId { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class FeatureUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Icon { get; set; }
        public string FeatureGroupId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

