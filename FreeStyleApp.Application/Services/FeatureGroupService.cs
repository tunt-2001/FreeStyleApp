using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.Application.Services
{
    public class FeatureGroupService : IFeatureGroupService
    {
        private readonly IAppDbContext _context;

        public FeatureGroupService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FeatureGroupDto>> GetAllFeatureGroupsAsync()
        {
            var groups = await _context.FeatureGroups
                .Include(fg => fg.Features)
                .OrderBy(fg => fg.DisplayOrder)
                .ThenBy(fg => fg.Name)
                .ToListAsync();

            return groups.Select(fg => new FeatureGroupDto
            {
                Id = fg.Id,
                Name = fg.Name,
                Description = fg.Description,
                Icon = fg.Icon,
                DisplayOrder = fg.DisplayOrder,
                IsActive = fg.IsActive,
                Features = fg.Features
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new FeatureDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Controller = f.Controller,
                        Action = f.Action,
                        Icon = f.Icon,
                        FeatureGroupId = f.FeatureGroupId,
                        DisplayOrder = f.DisplayOrder,
                        IsActive = f.IsActive
                    })
                    .ToList()
            }).ToList();
        }

        public async Task<FeatureGroupDto?> GetFeatureGroupByIdAsync(string id)
        {
            var group = await _context.FeatureGroups
                .Include(fg => fg.Features)
                .FirstOrDefaultAsync(fg => fg.Id == id);

            if (group == null) return null;

            return new FeatureGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Icon = group.Icon,
                DisplayOrder = group.DisplayOrder,
                IsActive = group.IsActive,
                Features = group.Features
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new FeatureDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Controller = f.Controller,
                        Action = f.Action,
                        Icon = f.Icon,
                        FeatureGroupId = f.FeatureGroupId,
                        DisplayOrder = f.DisplayOrder,
                        IsActive = f.IsActive
                    })
                    .ToList()
            };
        }

        public async Task<FeatureGroupDto> CreateFeatureGroupAsync(FeatureGroupCreateDto dto)
        {
            var group = new FeatureGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Description = dto.Description,
                Icon = dto.Icon,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.FeatureGroups.Add(group);
            await _context.SaveChangesAsync();

            return new FeatureGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Icon = group.Icon,
                DisplayOrder = group.DisplayOrder,
                IsActive = group.IsActive,
                Features = new List<FeatureDto>()
            };
        }

        public async Task<FeatureGroupDto> UpdateFeatureGroupAsync(FeatureGroupUpdateDto dto)
        {
            var group = await _context.FeatureGroups.FindAsync(dto.Id);
            if (group == null)
                throw new Exception("Không tìm thấy nhóm chức năng");

            group.Name = dto.Name;
            group.Description = dto.Description;
            group.Icon = dto.Icon;
            group.DisplayOrder = dto.DisplayOrder;
            group.IsActive = dto.IsActive;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetFeatureGroupByIdAsync(dto.Id) ?? throw new Exception("Lỗi khi cập nhật nhóm chức năng");
        }

        public async Task DeleteFeatureGroupAsync(string id)
        {
            var group = await _context.FeatureGroups
                .Include(fg => fg.Features)
                .Include(fg => fg.UserFeatureGroups)
                .FirstOrDefaultAsync(fg => fg.Id == id);

            if (group == null)
                throw new Exception("Không tìm thấy nhóm chức năng");

            if (group.Features.Any())
                throw new Exception("Không thể xóa nhóm chức năng đang có chức năng. Vui lòng xóa các chức năng trước.");

            if (group.UserFeatureGroups.Any())
                throw new Exception("Không thể xóa nhóm chức năng đang được gán cho người dùng. Vui lòng gỡ gán trước.");

            _context.FeatureGroups.Remove(group);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeatureDto>> GetFeaturesByGroupIdAsync(string groupId)
        {
            return await _context.Features
                .Where(f => f.FeatureGroupId == groupId)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new FeatureDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Controller = f.Controller,
                    Action = f.Action,
                    Icon = f.Icon,
                    FeatureGroupId = f.FeatureGroupId,
                    FeatureGroupName = f.FeatureGroup.Name,
                    DisplayOrder = f.DisplayOrder,
                    IsActive = f.IsActive
                })
                .ToListAsync();
        }

        public async Task<FeatureDto> CreateFeatureAsync(FeatureCreateDto dto)
        {
            var feature = new Feature
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Controller = dto.Controller,
                Action = dto.Action,
                Icon = dto.Icon,
                FeatureGroupId = dto.FeatureGroupId,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Features.Add(feature);
            await _context.SaveChangesAsync();

            var group = await _context.FeatureGroups.FindAsync(dto.FeatureGroupId);

            return new FeatureDto
            {
                Id = feature.Id,
                Name = feature.Name,
                Controller = feature.Controller,
                Action = feature.Action,
                Icon = feature.Icon,
                FeatureGroupId = feature.FeatureGroupId,
                FeatureGroupName = group?.Name ?? "",
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive
            };
        }

        public async Task<FeatureDto> UpdateFeatureAsync(FeatureUpdateDto dto)
        {
            var feature = await _context.Features.FindAsync(dto.Id);
            if (feature == null)
                throw new Exception("Không tìm thấy chức năng");

            feature.Name = dto.Name;
            feature.Controller = dto.Controller;
            feature.Action = dto.Action;
            feature.Icon = dto.Icon;
            feature.FeatureGroupId = dto.FeatureGroupId;
            feature.DisplayOrder = dto.DisplayOrder;
            feature.IsActive = dto.IsActive;
            feature.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var group = await _context.FeatureGroups.FindAsync(dto.FeatureGroupId);

            return new FeatureDto
            {
                Id = feature.Id,
                Name = feature.Name,
                Controller = feature.Controller,
                Action = feature.Action,
                Icon = feature.Icon,
                FeatureGroupId = feature.FeatureGroupId,
                FeatureGroupName = group?.Name ?? "",
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive
            };
        }

        public async Task DeleteFeatureAsync(string id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null)
                throw new Exception("Không tìm thấy chức năng");

            _context.Features.Remove(feature);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeatureGroupDto>> GetFeatureGroupsForUserAsync(string userId)
        {
            var userFeatureGroups = await _context.UserFeatureGroups
                .Where(ufg => ufg.UserId == userId)
                .Include(ufg => ufg.FeatureGroup)
                    .ThenInclude(fg => fg.Features)
                .Select(ufg => ufg.FeatureGroup)
                .Where(fg => fg.IsActive)
                .OrderBy(fg => fg.DisplayOrder)
                .ToListAsync();

            return userFeatureGroups.Select(fg => new FeatureGroupDto
            {
                Id = fg.Id,
                Name = fg.Name,
                Description = fg.Description,
                Icon = fg.Icon,
                DisplayOrder = fg.DisplayOrder,
                IsActive = fg.IsActive,
                Features = fg.Features
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new FeatureDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Controller = f.Controller,
                        Action = f.Action,
                        Icon = f.Icon,
                        FeatureGroupId = f.FeatureGroupId,
                        DisplayOrder = f.DisplayOrder,
                        IsActive = f.IsActive
                    })
                    .ToList()
            }).ToList();
        }

        public async Task AssignFeatureGroupsToUserAsync(string userId, List<string> featureGroupIds)
        {
            // Xóa các gán hiện tại
            var existingAssignments = await _context.UserFeatureGroups
                .Where(ufg => ufg.UserId == userId)
                .ToListAsync();

            _context.UserFeatureGroups.RemoveRange(existingAssignments);

            // Thêm các gán mới
            var newAssignments = featureGroupIds.Select(groupId => new UserFeatureGroup
            {
                UserId = userId,
                FeatureGroupId = groupId,
                AssignedAt = DateTime.UtcNow
            }).ToList();

            _context.UserFeatureGroups.AddRange(newAssignments);
            await _context.SaveChangesAsync();
        }
    }
}

