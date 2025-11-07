using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace FreeStyleApp.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyGuidToStringConversion(this ModelBuilder modelBuilder)
        {
            var guidToStringConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, Guid>(
                v => Guid.Parse(v),
                v => v.ToString());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && 
                        (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || 
                         property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)))
                    {
                        property.SetValueConverter(guidToStringConverter);
                    }
                }
            }
        }
    }
}


