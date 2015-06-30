using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Numero3.EntityFramework.Demo.DatabaseContext;
using Numero3.EntityFramework.Demo.DomainModel;
using Numero3.EntityFramework.Demo.Repositories;
using Numero3.EntityFramework.Interfaces;

namespace Numero3.EntityFramework.Demo.BusinessLogicServices
{
	/*
	 * Example business logic service implementing query functionalities (i.e. read actions).
	 */
	public class UserQueryService
	{
		private readonly IDbContextScopeFactory _dbContextScopeFactory;
		private readonly IUserRepository _userRepository;

		public UserQueryService(IDbContextScopeFactory dbContextScopeFactory, IUserRepository userRepository)
		{
			if (dbContextScopeFactory == null) throw new ArgumentNullException("dbContextScopeFactory");
			if (userRepository == null) throw new ArgumentNullException("userRepository");
			_dbContextScopeFactory = dbContextScopeFactory;
			_userRepository = userRepository;
		}

		public User GetUser(Guid userId)
		{
			/*
			 * An example of using DbContextScope for read-only queries. 
			 * Here, we access the Entity Framework DbContext directly from 
			 * the business logic service class.
			 * 
			 * Calling SaveChanges() is not necessary here (and in fact not 
			 * possible) since we created a read-only scope.
			 */
			using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
			{
				var dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
				var user = dbContext.Users.Find(userId);

				if (user == null)
					throw new ArgumentException(String.Format("Invalid value provided for userId: [{0}]. Couldn't find a user with this ID.", userId));

				return user;
			}
		}

		public IEnumerable<User> GetUsers(params Guid[] userIds)
		{
			using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
			{
				var dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
				return dbContext.Users.Where(u => userIds.Contains(u.Id)).ToList();
			}
		}

		public User GetUserViaRepository(Guid userId)
		{
			/*
			 * Same as GetUsers() but using a repository layer instead of accessing the 
			 * EF DbContext directly.
			 * 
			 * Note how we don't have to worry about knowing what type of DbContext the 
			 * repository will need, about creating the DbContext instance or about passing
			 * DbContext instances around. 
			 * 
			 * The DbContextScope will take care of creating the necessary DbContext instances
			 * and making them available as ambient contexts for our repository layer to use.
			 * It will also guarantee that only one instance of any given DbContext type exists
			 * within its scope ensuring that all persistent entities managed within that scope
			 * are attached to the same DbContext. 
			 */
			using (_dbContextScopeFactory.CreateReadOnly())
			{
				var user = _userRepository.Get(userId);

				if (user == null)
					throw new ArgumentException(String.Format("Invalid value provided for userId: [{0}]. Couldn't find a user with this ID.", userId));

				return user;
			}
		}

	    public User GetUserUncommitted(Guid userId)
		{
			/*
			 * An example of explicit database transaction. 
			 * 
			 * Read the comment for CreateReadOnlyWithTransaction() before using this overload
			 * as there are gotchas when doing this!
			 */
			using (_dbContextScopeFactory.CreateReadOnlyWithTransaction(IsolationLevel.ReadUncommitted))
			{
				return _userRepository.Get(userId);
			}
		}
	}
}
