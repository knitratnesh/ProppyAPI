using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TheProppyAPI.Models;

namespace TheProppyAPI.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
            new IdentityRole
            {
                Name = UserRoles.Admin,
                NormalizedName = UserRoles.Admin
            },
            new IdentityRole
            {
                Name = UserRoles.Agent,
                NormalizedName = UserRoles.Agent
            },
            new IdentityRole
            {
                Name = UserRoles.Customer,
                NormalizedName = UserRoles.Customer
            });
        }
    }
}
