using System.Collections.Generic;
using System.Threading.Tasks;
using RollUp.Core.Models;

namespace RollUp.Core.Interfaces;

public interface IMenuService
{
    Task<List<MenuItem>> GetAllItemsAsync(int? outletId = null);
    Task<List<MenuItem>> GetItemsByCategoryAsync(string category, int? outletId = null);
    Task<List<string>> GetCategoriesAsync();
    Task<MenuItem?> GetItemByIdAsync(int id, int? outletId = null);
    Task AddItemAsync(MenuItem item);
    Task UpdateItemAsync(MenuItem item);
    Task DeleteItemAsync(int id);
    Task ToggleAvailabilityAsync(int id, int? outletId = null);
    Task AddCategoryAsync(string name);
    Task DeleteCategoryAsync(string name);
    Task<List<string>> GetArchivedCategoriesAsync();
    Task RestoreCategoryAsync(string name);
    Task PermanentlyDeleteCategoryAsync(string name);
}
