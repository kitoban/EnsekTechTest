using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsekTechTest.Models;
using Microsoft.EntityFrameworkCore;

namespace EnsekTechTest.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DatabaseContext context)
        {
            context.Database.EnsureCreated();

            if (context.Accounts.Any())
            {
                return;
            }


            var accounts = Properties.Resources.Test_Accounts.Split(Environment.NewLine);

            for (var index = 1; index < accounts.Length; index++)
            {
                var account = accounts[index];

                if (string.IsNullOrEmpty(account))
                {
                    continue;
                }

                var values = account.Split(',');

                var newAccount = new Account()
                {
                    Id = int.Parse(values[0]),
                    FirstName = values[1],
                    LastName = values[2]
                };

                context.Accounts.Add(newAccount);
            }

            context.SaveChanges();
        }
    }
}
