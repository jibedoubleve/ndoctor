﻿/*
    This file is part of NDoctor.

    NDoctor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    NDoctor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with NDoctor.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace Probel.NDoctor.Domain.Test.Helpers
{
    using System;

    using AutoMapper;

    using NUnit.Framework;

    using Probel.NDoctor.Domain.DAL.Cfg;
    using Probel.NDoctor.Domain.DAL.Entities;
    using Probel.NDoctor.Domain.DTO.Objects;

    [TestFixture]
    [Category("Database")]
    public abstract class TestBase<T> : IDisposable
    {
        #region Constructors

        public TestBase()
        {
            this.Database = new InMemoryDatabase();
        }

        #endregion Constructors

        #region Properties

        protected T Component
        {
            get;
            private set;
        }

        protected InMemoryDatabase Database
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Database.Dispose();
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.ConfigureAutoMapper();
            new DalConfigurator().ConfigureInMemory();
            this.Component = this.GetComponentInstance();
            //Build.Database(this.Database.Session);
            DbBuilder.Create(this.Database.Session);

            this.PostFixtureSetup();
        }

        /// <summary>
        /// Gets an instance of the tested component.
        /// </summary>
        /// <returns></returns>
        protected abstract T GetComponentInstance();

        /// <summary>
        /// Action to execute after the setup of the fixture.
        /// </summary>
        protected virtual void PostFixtureSetup()
        {
            //By default does nothing
        }

        /// <summary>
        /// Execute the specified action in a transaction.
        /// </summary>
        /// <param name="action">The action.</param>
        protected void Transaction(Action action)
        {
            Transaction(() =>
            {
                action();
                return null;
            });
        }

        /// <summary>
        /// Execute the specified action in a transaction and returns the object
        /// resulting of the function.
        /// </summary>
        /// <param name="function">The action.</param>
        /// <returns></returns>
        protected object Transaction(Func<object> function)
        {
            object result = null;
            using (var tx = this.Database.Session.Transaction)
            {
                tx.Begin();
                result = function();
                tx.Commit();
            }
            return result;
        }

        private void ConfigureAutoMapper()
        {
            Mapper.CreateMap<User, UserDto>();
        }

        #endregion Methods
    }
}