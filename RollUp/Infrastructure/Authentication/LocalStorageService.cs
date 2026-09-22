using Microsoft.JSInterop;

namespace RollUp.Infrastructure.Authentication;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch
        {
            // Ignore during prerendering
        }
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Ignore during prerendering
        }
    }
}
