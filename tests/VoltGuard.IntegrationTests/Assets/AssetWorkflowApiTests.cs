using System.Net;
using System.Net.Http.Json;
using VoltGuard.Application.DTOs.Assets;
using VoltGuard.Application.DTOs.Customers;
using VoltGuard.Application.DTOs.Sites;
using VoltGuard.Domain.Constants;
using VoltGuard.IntegrationTests.Infrastructure;

namespace VoltGuard.IntegrationTests.Assets;

public class AssetWorkflowApiTests : IntegrationTestBase
{
    public AssetWorkflowApiTests(VoltGuardApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Assets_RequireAuthentication()
    {
        var response = await Client.GetAsync("/api/assets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanCreateCustomerSiteAndAsset()
    {
        await AuthenticateAsync();

        var customer = await CreateCustomerAsync("Northwind Energy");
        var site = await CreateSiteAsync(customer.Id, "Main Plant");

        var assetResponse = await Client.PostAsJsonAsync("/api/assets", new
        {
            siteId = site.Id,
            name = "Transformer TX-1",
            assetTag = "TX-1",
            serialNumber = "SN-001",
            assetType = "Transformer",
            manufacturer = "VoltGuard",
            model = "VG-500",
            locationDescription = "Substation A",
            ratedVoltage = 415m,
            ratedCurrent = 200m,
            notes = "Commissioned asset"
        });

        Assert.Equal(HttpStatusCode.Created, assetResponse.StatusCode);
        var asset = await ReadAsAsync<AssetDto>(assetResponse);
        Assert.Equal(site.Id, asset.SiteId);
        Assert.Equal(customer.Id, asset.CustomerId);
        Assert.Equal("Transformer TX-1", asset.Name);
        Assert.Equal("TX-1", asset.AssetTag);
        Assert.Equal(RiskLevelConstants.Unknown, asset.RiskLevel);
        Assert.True(asset.IsActive);
    }

    [Fact]
    public async Task DuplicateActiveAssetNameForSameSite_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        var customer = await CreateCustomerAsync("Duplicate Asset Customer");
        var site = await CreateSiteAsync(customer.Id, "Duplicate Asset Site");
        await CreateAssetAsync(site.Id, "Cable Run A", "CBL-A");

        var duplicateResponse = await Client.PostAsJsonAsync("/api/assets", new
        {
            siteId = site.Id,
            name = "Cable Run A",
            assetTag = "CBL-B"
        });

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    protected async Task<CustomerDto> CreateCustomerAsync(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/customers", new
        {
            name
        });

        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<CustomerDto>(response);
    }

    protected async Task<SiteDto> CreateSiteAsync(Guid customerId, string name)
    {
        var response = await Client.PostAsJsonAsync("/api/sites", new
        {
            customerId,
            name
        });

        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<SiteDto>(response);
    }

    protected async Task<AssetDto> CreateAssetAsync(Guid siteId, string name, string assetTag)
    {
        var response = await Client.PostAsJsonAsync("/api/assets", new
        {
            siteId,
            name,
            assetTag
        });

        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<AssetDto>(response);
    }
}
