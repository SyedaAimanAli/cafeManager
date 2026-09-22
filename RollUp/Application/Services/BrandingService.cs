using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Persistence;

namespace RollUp.Application.Services;

public class BrandingService : IBrandingService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public BrandingService(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<TenantBrandingDto> GetBrandingAsync()
    {
        var tenantId = _tenantContext.CurrentTenantId;
        Tenant? tenant = null;

        if (tenantId.HasValue && tenantId.Value > 0)
        {
            tenant = await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(_tenantContext.CurrentTenantSlug))
        {
            tenant = await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Slug == _tenantContext.CurrentTenantSlug);
        }

        if (tenant == null)
        {
            // Pick most recently updated or active tenant
            tenant = await _db.Tenants.IgnoreQueryFilters()
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (tenant == null)
        {
            return new TenantBrandingDto
            {
                BusinessName = "RollUp Cafe",
                Tagline = "Artisan Cafe & Bakery",
                ThemeTemplate = "bistro",
                ColorScheme = "espresso",
                FontFamily = "inter",
                Currency = "USD"
            };
        }

        return new TenantBrandingDto
        {
            BusinessName = tenant.Name,
            Tagline = tenant.Tagline ?? "Artisan Cafe & Quick Orders",
            LogoUrl = tenant.LogoUrl,
            Address = tenant.Address,
            ContactPhone = tenant.ContactPhone,
            ContactEmail = tenant.ContactEmail,
            Currency = string.IsNullOrWhiteSpace(tenant.Currency) ? "USD" : tenant.Currency,
            ThemeTemplate = string.IsNullOrWhiteSpace(tenant.ThemeTemplate) ? "bistro" : tenant.ThemeTemplate,
            ColorScheme = string.IsNullOrWhiteSpace(tenant.ColorScheme) ? "espresso" : tenant.ColorScheme,
            FontFamily = string.IsNullOrWhiteSpace(tenant.FontFamily) ? "inter" : tenant.FontFamily,
            CustomPrimaryColor = tenant.CustomPrimaryColor,
            CustomAccentColor = tenant.CustomAccentColor
        };
    }

    public async Task<bool> UpdateBrandingAsync(TenantBrandingDto dto)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        Tenant? tenant = null;

        if (tenantId.HasValue && tenantId.Value > 0)
        {
            tenant = await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId.Value);
        }

        if (tenant == null)
        {
            tenant = await _db.Tenants.IgnoreQueryFilters()
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (tenant == null) return false;

        tenant.Name = dto.BusinessName.Trim();
        tenant.Tagline = dto.Tagline?.Trim();
        tenant.LogoUrl = dto.LogoUrl;
        tenant.Address = dto.Address.Trim();
        tenant.ContactPhone = dto.ContactPhone.Trim();
        tenant.ContactEmail = dto.ContactEmail.Trim();
        tenant.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency.ToUpper();
        tenant.ThemeTemplate = dto.ThemeTemplate;
        tenant.ColorScheme = dto.ColorScheme;
        tenant.FontFamily = dto.FontFamily;
        tenant.CustomPrimaryColor = dto.CustomPrimaryColor;
        tenant.CustomAccentColor = dto.CustomAccentColor;
        tenant.UpdatedAt = DateTime.UtcNow;

        _db.Tenants.Update(tenant);
        await _db.SaveChangesAsync();
        return true;
    }
}
