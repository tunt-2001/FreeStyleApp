namespace FreeStyleApp.Application.DTOs
{
    public class FeatureGroupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<FeatureDto> Features { get; set; } = new();
    }

    public class FeatureGroupCreateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class FeatureGroupUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

