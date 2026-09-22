namespace RollUp.Infrastructure.Authentication;

public interface ILocalStorageService
{
    ValueTask<string?> GetItemAsync(string key);
    ValueTask SetItemAsync(string key, string value);
    ValueTask RemoveItemAsync(string key);
}
