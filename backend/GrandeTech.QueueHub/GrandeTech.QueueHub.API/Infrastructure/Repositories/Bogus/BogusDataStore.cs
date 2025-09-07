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
        
        // Test user constants for each role
        private static readonly string TestPlatformAdminFullName = "Platform Admin Test";
        private static readonly string TestPlatformAdminEmail = "platformadmin@test.com";
        private static readonly string TestPlatformAdminPassword = "PlatformAdminPass123!";
        private static readonly string TestPlatformAdminPhone = "+1555123456";
        private static readonly string TestPlatformAdminRole = UserRoles.PlatformAdmin;

        private static readonly string TestOwnerFullName = "Owner Test";
        private static readonly string TestOwnerEmail = "owner@test.com";
        private static readonly string TestOwnerPhone = "+1555123457";
        private static readonly string TestOwnerPassword = "OwnerPass123!";
        private static readonly string TestOwnerRole = UserRoles.Owner;
        
        private static readonly string TestStaffFullName = "Staff Test";
        private static readonly string TestStaffEmail = "staff@test.com";
        private static readonly string TestStaffPhone = "+1555123458";
        private static readonly string TestStaffPassword = "StaffPass123!";
        private static readonly string TestStaffRole = UserRoles.Staff;

        private static readonly string TestCustomerFullName = "Customer Test";
        private static readonly string TestCustomerEmail = "customer@test.com";
        private static readonly string TestCustomerPhone = "+1555123459";
        private static readonly string TestCustomerPassword = "CustomerPass123!";
        private static readonly string TestCustomerRole = UserRoles.Customer;

        private static readonly string TestServiceAccountFullName = "Service Account Test";
        private static readonly string TestServiceAccountEmail = "service@test.com";
        private static readonly string TestServiceAccountPhone = "+1555123460";
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

                // Ensure all test users exist
                CreateOrUpdateTestPlatformAdminUser();
                CreateOrUpdateTestOwnerUser();
                CreateOrUpdateTestStaffUser();
                CreateOrUpdateTestCustomerUser();
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
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Phone.PhoneNumber("##########"),
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

        public static void CreateOrUpdateTestPlatformAdminUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var platformAdminUser = users.Values.Cast<User>().FirstOrDefault(u => u.Role == TestPlatformAdminRole);
            if (platformAdminUser == null)
            {
                platformAdminUser = new User(
                    TestPlatformAdminFullName,
                    TestPlatformAdminEmail,
                    TestPlatformAdminPhone,
                    BCrypt.Net.BCrypt.HashPassword(TestPlatformAdminPassword),
                    TestPlatformAdminRole
                );
                platformAdminUser.Activate();
                platformAdminUser.DisableTwoFactor();
                users.TryAdd(platformAdminUser.Id, platformAdminUser);
            }
            else
            {
                platformAdminUser.Role = TestPlatformAdminRole;
                platformAdminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPlatformAdminPassword);
                platformAdminUser.Activate();
                platformAdminUser.DisableTwoFactor();
                users[platformAdminUser.Id] = platformAdminUser;
            }
        }

        public static void CreateOrUpdateTestOwnerUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var ownerUser = users.Values.Cast<User>().FirstOrDefault(u => u.Role == TestOwnerRole);
            if (ownerUser == null)
            {
                ownerUser = new User(
                    TestOwnerFullName,
                    TestOwnerEmail,
                    TestOwnerPhone,
                    BCrypt.Net.BCrypt.HashPassword(TestOwnerPassword),
                    TestOwnerRole
                );
                ownerUser.Activate();
                ownerUser.DisableTwoFactor();
                ownerUser.AddPermission(Permission.CreateStaff);
                ownerUser.AddPermission(Permission.UpdateStaff);
                users.TryAdd(ownerUser.Id, ownerUser);
            }
            else
            {
                ownerUser.Role = TestOwnerRole;
                ownerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestOwnerPassword);
                ownerUser.Activate();
                ownerUser.DisableTwoFactor();
                ownerUser.AddPermission(Permission.CreateStaff);
                ownerUser.AddPermission(Permission.UpdateStaff);
                users[ownerUser.Id] = ownerUser;
            }
        }

        public static void CreateOrUpdateTestStaffUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var staffUser = users.Values.Cast<User>().FirstOrDefault(u => u.Role == TestStaffRole);
            if (staffUser == null)
            {
                staffUser = new User(
                    TestStaffFullName,
                    TestStaffEmail,
                    TestStaffPhone,
                    BCrypt.Net.BCrypt.HashPassword(TestStaffPassword),
                    TestStaffRole
                );
                staffUser.Activate();
                staffUser.DisableTwoFactor();
                users.TryAdd(staffUser.Id, staffUser);
            }
            else
            {
                staffUser.Role = TestStaffRole;
                staffUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestStaffPassword);
                staffUser.Activate();
                staffUser.DisableTwoFactor();
                users[staffUser.Id] = staffUser;
            }
        }

        public static void CreateOrUpdateTestCustomerUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var customerUser = users.Values.Cast<User>().FirstOrDefault(u => u.Role == TestCustomerRole);
            if (customerUser == null)
            {
                customerUser = new User(
                    TestCustomerFullName,
                    TestCustomerEmail,
                    TestCustomerPhone,
                    BCrypt.Net.BCrypt.HashPassword(TestCustomerPassword),
                    TestCustomerRole
                );
                customerUser.Activate();
                customerUser.DisableTwoFactor();
                users.TryAdd(customerUser.Id, customerUser);
            }
            else
            {
                customerUser.Role = TestCustomerRole;
                customerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestCustomerPassword);
                customerUser.Activate();
                customerUser.DisableTwoFactor();
                users[customerUser.Id] = customerUser;
            }
        }

        public static void CreateOrUpdateTestServiceAccountUser()
        {
            Initialize();
            var users = _dataStore[typeof(User)];
            var serviceUser = users.Values.Cast<User>().FirstOrDefault(u => u.Role == TestServiceAccountRole);
            if (serviceUser == null)
            {
                serviceUser = new User(
                    TestServiceAccountFullName,
                    TestServiceAccountEmail,
                    TestServiceAccountPhone,
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