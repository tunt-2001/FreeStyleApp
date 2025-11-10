using FreeStyleApp.Application.DTOs;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IFeatureGroupService
    {
        Task<List<FeatureGroupDto>> GetAllFeatureGroupsAsync();
        Task<FeatureGroupDto?> GetFeatureGroupByIdAsync(string id);
        Task<FeatureGroupDto> CreateFeatureGroupAsync(FeatureGroupCreateDto dto);
        Task<FeatureGroupDto> UpdateFeatureGroupAsync(FeatureGroupUpdateDto dto);
        Task DeleteFeatureGroupAsync(string id);
        Task<List<FeatureDto>> GetFeaturesByGroupIdAsync(string groupId);
        Task<FeatureDto> CreateFeatureAsync(FeatureCreateDto dto);
        Task<FeatureDto> UpdateFeatureAsync(FeatureUpdateDto dto);
        Task DeleteFeatureAsync(string id);
        Task<List<FeatureGroupDto>> GetFeatureGroupsForUserAsync(string userId);
        Task AssignFeatureGroupsToUserAsync(string userId, List<string> featureGroupIds);
    }
}

