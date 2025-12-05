using Microsoft.EntityFrameworkCore;
using SoftwareFromClick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Data
{
    public class ApiKeyService
    {
        public List<ApiKeys> GetAllKeys() 
        {
            using (var context = new AppDbContext())
            {
                return context.ApiKeys
                              .Include(k => k.Provider) // pobiera nazwę dostawcy
                              .OrderByDescending(k => k.CreatedAt) // mniej ważne
                              .ToList();
            }
        }
        public void AddApiKey(string keyString, string providerName)
        {
            using (var context = new AppDbContext())
            {
                // szukamy lub tworzymy user
                var user = GetOrCreateDefaultUser(context);

                // szukamy lub tworzymy providera
                var provider = GetOrCreateProvider(context, providerName);

                // towrzymy nowy rekord
                var apiKeyEntity = new ApiKeys
                {
                    ApiKey = keyString,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UserId = user.Id,
                    ProviderId = provider.Id
                };

                //  zapis w bazie
                context.ApiKeys.Add(apiKeyEntity);
                context.SaveChanges();
            }
        }

        // usuwanie
        public void DeleteApiKey(int id)
        {
            using (var context = new AppDbContext())
            {
                var keyToDelete = context.ApiKeys.Find(id);
                if (keyToDelete != null)
                {
                    context.ApiKeys.Remove(keyToDelete);
                    context.SaveChanges();
                }
            }
        }

        //trzeba dodać edytowanie?

        // --- Metody pomocnicze---

        private User GetOrCreateDefaultUser(AppDbContext context)
        {
            var user = context.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User
                {
                    Username = "DefaultUser",
                    Email = "user@local.app",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
            return user;
        }

        private Provider GetOrCreateProvider(AppDbContext context, string name)
        {
            var provider = context.Providers.FirstOrDefault(p => p.Name == name);
            if (provider == null)
            {
                provider = new Provider
                {
                    Name = name,
                    Url = "https://api.openai.com"
                };
                context.Providers.Add(provider);
                context.SaveChanges();
            }
            return provider;
        }

    }
}

