using System.Collections.Generic;
using System.Threading.Tasks;
using RollUp.Core.Models;

namespace RollUp.Core.Interfaces;

public interface IMenuService
{
    Task<List<MenuItem>> GetAllItemsAsync();
    Task<List<MenuItem>> GetItemsByCategoryAsync(string category);
    Task<List<string>> GetCategoriesAsync();
    Task<MenuItem?> GetItemByIdAsync(int id);
    Task AddItemAsync(MenuItem item);
    Task UpdateItemAsync(MenuItem item);
    Task DeleteItemAsync(int id);
    Task ToggleAvailabilityAsync(int id);
    Task AddCategoryAsync(string name);
    Task DeleteCategoryAsync(string name);
}
