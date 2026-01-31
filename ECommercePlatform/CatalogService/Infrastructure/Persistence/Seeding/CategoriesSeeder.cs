using CatalogService.Domain.Aggregates;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence.Seeding
{
    public static class CategoriesSeeder
    {
        public static readonly List<Category> CategoriesToSeed = new()
        {
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000002"), "Home & Kitchen", "Appliances, cookware, home decor and kitchen tools."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000003"), "Clothing", "Apparel for men, women and children, plus accessories."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000004"), "Books", "Fiction, non-fiction, textbooks and e-books."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000005"), "Health & Personal Care", "Vitamins, personal hygiene, and wellness products."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000006"), "Sports & Outdoors", "Sporting goods, fitness equipment and outdoor gear."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000007"), "Toys & Games", "Toys, puzzles, board games and hobby items for kids."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000008"), "Beauty", "Cosmetics, skincare and haircare products."),
            new Category(Guid.Parse("11111111-0000-0000-0000-000000000009"), "Automotive", "Auto parts, tools and vehicle accessories."),
            new Category(Guid.Parse("11111111-0000-0000-0000-00000000000A"), "Grocery & Gourmet", "Pantry staples, specialty and gourmet food items."),
            new Category(Guid.Parse("11111111-0000-0000-0000-00000000000B"), "Office Supplies", "Stationery, office equipment and supplies."),
            new Category(Guid.Parse("11111111-0000-0000-0000-00000000000C"), "Pet Supplies", "Food, toys and health products for pets."),
            new Category(Guid.Parse("11111111-0000-0000-0000-00000000000D"), "Tools & Home Improvement", "Hand tools, power tools and hardware for DIY."),
            new Category(Guid.Parse("11111111-0000-0000-0000-00000000000E"), "Baby", "Diapers, feeding, nursery and baby care essentials."),
        };

        public static async Task SeedCategoriesAsync(CatalogDbContext dbContext)
        {
            if (dbContext is null) throw new ArgumentNullException(nameof(dbContext));

            // Collect the IDs we intend to seed
            List<Guid> seedIds = CategoriesToSeed.Select(c => c.Id).ToList();

            // Load existing IDs in a single roundtrip
            List<Guid> existingIds = await dbContext
                .Categories
                .Where(c => seedIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            // Determine which categories are missing
            List<Category> missing = CategoriesToSeed
                .Where(c => !existingIds.Contains(c.Id))
                .ToList();

            if (!missing.Any())
                return;

            // Add all missing at once and persist in a single SaveChanges call
            await dbContext.Categories.AddRangeAsync(missing);
            await dbContext.SaveChangesAsync();
        }
    }
}
