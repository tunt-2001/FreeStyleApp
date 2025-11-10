namespace FreeStyleApp.Application.DTOs
{
    public class UserFeatureGroupDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string FeatureGroupId { get; set; }
        public string FeatureGroupName { get; set; }
    }

    public class AssignFeatureGroupsToUserDto
    {
        public string UserId { get; set; }
        public List<string> FeatureGroupIds { get; set; } = new();
    }
}

