using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public static class BogusDataStoreExtensions
    {
        public static void AddEntityType<T>(Func<Faker<T>> fakerFactory, int count = 10) where T : BaseEntity
        {
            var faker = fakerFactory();
            var entities = faker.Generate(count);
            
            foreach (var entity in entities)
            {
                BogusDataStore.Add(entity);
            }
        }

        public static void AddEntityType<T>(List<T> entities) where T : BaseEntity
        {
            foreach (var entity in entities)
            {
                BogusDataStore.Add(entity);
            }
        }

        public static List<T> GetByPredicate<T>(Func<T, bool> predicate) where T : BaseEntity
        {
            return BogusDataStore.GetAll<T>().Where(predicate).ToList();
        }

        public static T? GetFirstByPredicate<T>(Func<T, bool> predicate) where T : BaseEntity
        {
            return BogusDataStore.GetAll<T>().FirstOrDefault(predicate);
        }

        public static int Count<T>() where T : BaseEntity
        {
            return BogusDataStore.GetAll<T>().Count;
        }

        public static bool Any<T>(Func<T, bool> predicate) where T : BaseEntity
        {
            return BogusDataStore.GetAll<T>().Any(predicate);
        }

        public static bool All<T>(Func<T, bool> predicate) where T : BaseEntity
        {
            return BogusDataStore.GetAll<T>().All(predicate);
        }
    }
} 