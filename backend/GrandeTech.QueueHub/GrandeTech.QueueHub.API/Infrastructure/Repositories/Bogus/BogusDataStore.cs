using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bogus;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public static class BogusDataStore
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, BaseEntity>> _dataStore = new();
        private static readonly object _lock = new();
        private static bool _isInitialized = false;
        private static readonly string TestAdminUsername = "admin_test";
        private static readonly string TestAdminEmail = "admin@test.com";
        private static readonly string TestAdminPassword = "AdminPass123!";
        private static readonly string TestAdminRole = UserRoles.Owner;
        
        private static readonly string TestServiceAccountUsername = "service_test";
        private static readonly string TestServiceAccountEmail = "service@test.com";
        private static readonly string TestServiceAccountPassword = "ServicePass123!";
        private static readonly string TestServiceAccountRole = UserRoles.ServiceAccount;

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_isInitialized) return;
                
                // Initialize data stores for each entity type
                RegisterEntityType<User>();
                RegisterEntityType<Organization>();
                RegisterEntityType<Domain.Locations.Location>();
                RegisterEntityType<Domain.Queues.Queue>();
                RegisterEntityType<Domain.Customers.Customer>();
                RegisterEntityType<Domain.Staff.StaffMember>();
                RegisterEntityType<Domain.ServicesOffered.ServiceOffered>();
                RegisterEntityType<Domain.Subscriptions.SubscriptionPlan>();
                
                // Generate initial data
                GenerateInitialData();

                // Mark as initialized before creating admin user to avoid recursion
                _isInitialized = true;

                // Ensure test admin user exists
                CreateOrUpdateTestAdminUser();
                
                // Ensure test service account user exists
                CreateOrUpdateTestServiceAccountUser();
            }
        }

        public static void RegisterEntityType<T>() where T : BaseEntity
        {
            if (!_dataStore.ContainsKey(typeof(T)))
            {
                _dataStore.TryAdd(typeof(T), new ConcurrentDictionary<Guid, BaseEntity>());
            }
        }

        public static ConcurrentDictionary<Guid, T> GetDataStore<T>() where T : BaseEntity
        {
            Initialize();
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                return new ConcurrentDictionary<Guid, T>(
                    store.ToDictionary(kvp => kvp.Key, kvp => (T)kvp.Value)
                );
            }
            
            var newStore = new ConcurrentDictionary<Guid, BaseEntity>();
            _dataStore.TryAdd(typeof(T), newStore);
            return new ConcurrentDictionary<Guid, T>();
        }

        public static void Add<T>(T entity) where T : BaseEntity
        {
            Initialize();
            AddInternal(entity);
        }

        private static void AddInternal<T>(T entity) where T : BaseEntity
        {
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                store.TryAdd(entity.Id, entity);
            }
        }

        public static void Update<T>(T entity) where T : BaseEntity
        {
            Initialize();
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                store.AddOrUpdate(entity.Id, entity, (key, oldValue) => entity);
            }
        }

        public static bool Remove<T>(Guid id) where T : BaseEntity
        {
            Initialize();
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                return store.TryRemove(id, out _);
            }
            return false;
        }

        public static T? Get<T>(Guid id) where T : BaseEntity
        {
            Initialize();
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                if (store.TryGetValue(id, out var entity))
                {
                    return (T)entity;
                }
            }
            return null;
        }

        public static List<T> GetAll<T>() where T : BaseEntity
        {
            Initialize();
            if (_dataStore.TryGetValue(typeof(T), out var store))
            {
                return store.Values.Cast<T>().ToList();
            }
            return new List<T>();
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _dataStore.Clear();
                _isInitialized = false;
            }
        }

        private static void GenerateInitialData()
        {
            // Generate initial users
            var userFaker = new global::Bogus.Faker<User>()
                .CustomInstantiator(f => new User(
                    f.Internet.UserName(),
                    f.Internet.Email(),
                    BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
                    f.PickRandom(UserRoles.AllRoles.AsEnumerable())
                ));

            var users = userFaker.Generate(10);
            foreach (var user in users)
            {
                AddInternal(user);
            }

            // Generate initial organizations
            var orgFaker = new global::Bogus.Faker<Organization>()
                .CustomInstantiator(f => new Organization(
                    f.Company.CompanyName(),
                    f.Internet.UserName().ToLowerInvariant(),
                    f.Lorem.Sentence(),
                    f.Internet.Email(),
                    f.Phone.PhoneNumber("##########"),
                    f.Internet.Url(),
                    null,
                    Guid.NewGuid(),
                    "system"
                ));

            var organizations = orgFaker.Generate(5);
            foreach (var org in organizations)
            {
                AddInternal(org);
            }
        }

        public static void CreateOrUpdateTestAdminUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var adminUser = users.Values.Cast<User>().FirstOrDefault(u => u.Username == TestAdminUsername);
            if (adminUser == null)
            {
                adminUser = new User(
                    TestAdminUsername,
                    TestAdminEmail,
                    BCrypt.Net.BCrypt.HashPassword(TestAdminPassword),
                    TestAdminRole
                );
                adminUser.Activate();
                adminUser.DisableTwoFactor();
                users.TryAdd(adminUser.Id, adminUser);
            }
            else
            {
                adminUser.Role = TestAdminRole;
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestAdminPassword);
                adminUser.Activate();
                adminUser.DisableTwoFactor();
                users[adminUser.Id] = adminUser;
            }
        }

        public static void CreateOrUpdateTestServiceAccountUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var serviceUser = users.Values.Cast<User>().FirstOrDefault(u => u.Username == TestServiceAccountUsername);
            if (serviceUser == null)
            {
                serviceUser = new User(
                    TestServiceAccountUsername,
                    TestServiceAccountEmail,
                    BCrypt.Net.BCrypt.HashPassword(TestServiceAccountPassword),
                    TestServiceAccountRole
                );
                serviceUser.Activate();
                serviceUser.DisableTwoFactor();
                users.TryAdd(serviceUser.Id, serviceUser);
            }
            else
            {
                serviceUser.Role = TestServiceAccountRole;
                serviceUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestServiceAccountPassword);
                serviceUser.Activate();
                serviceUser.DisableTwoFactor();
                users[serviceUser.Id] = serviceUser;
            }
        }
    }
} 