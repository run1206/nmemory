﻿using System;
using System.Collections.Generic;
using System.Linq;
using NMemory.Exceptions;
using NMemory.Indexes;

namespace NMemory.Tables
{
    /// <summary>
    /// Represents a relation between two database tables.
    /// </summary>
    /// <typeparam name="TPrimary">The type of the entities of the referred table.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
    /// <typeparam name="TForeign">The type of the entities of the referring table.</typeparam>
    /// <typeparam name="TForeignKey">The type of the foreign key.</typeparam>
	public class Relation<
        TPrimary, 
        TPrimaryKey, 
        TForeign, 
        TForeignKey> :

        IRelation

        where TPrimary : class
        where TForeign : class
	{

		#region Members

        private IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex;
        private IIndex<TForeign, TForeignKey> foreignIndex;

		private Func<TForeignKey, TPrimaryKey> convertForeignToPrimary;
		private Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign;

		#endregion

		#region Ctor

		internal Relation(
            IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
            IIndex<TForeign, TForeignKey> foreignIndex,
            Func<TForeignKey, TPrimaryKey>  foreignToPrimary,
            Func<TPrimaryKey, TForeignKey> primaryToForeign)
		{
			this.primaryIndex = primaryIndex;
			this.foreignIndex = foreignIndex;

            this.convertForeignToPrimary = foreignToPrimary;
            this.convertPrimaryToForeign = primaryToForeign;
        }

        #endregion

        ITable IRelation.PrimaryTable
        {
            get { return this.primaryIndex.Table; }
        }

        ITable IRelation.ForeignTable
        {
            get { return this.foreignIndex.Table; }
        }

        IIndex IRelation.PrimaryIndex
        {
            get { return this.primaryIndex; }
        }

        IIndex IRelation.ForeignIndex
        {
            get { return this.foreignIndex; }
        }


        void IRelation.ValidateEntity(object foreign)
        {
            TForeign foreignEntity = (TForeign)foreign;
            TForeignKey foreignKey = this.foreignIndex.KeyInfo.SelectKey(foreignEntity);

            // Empty foreign key means, that it does not refer to anything
            if (this.foreignIndex.KeyInfo.IsEmptyKey(foreignKey))
            {
                return;
            }

            TPrimaryKey primaryKey = this.convertForeignToPrimary.Invoke(foreignKey);

            if (!this.primaryIndex.Contains(primaryKey))
            {
                // TODO: Proper message
                throw new ForeignKeyViolationException();
            }
        }


        IEnumerable<object> IRelation.GetReferringEntities(object primary)
        {
            TPrimary primaryEntity = (TPrimary)primary;
            TPrimaryKey primaryKey = this.primaryIndex.KeyInfo.SelectKey(primaryEntity);

            TForeignKey foreignKey = this.convertPrimaryToForeign.Invoke(primaryKey);

            return this.foreignIndex.Select(foreignKey);
        }

        IEnumerable<object> IRelation.GetReferredEntities(object foreign)
        {
            TForeign foreignEntity = (TForeign)foreign;
            TForeignKey foreignKey = this.foreignIndex.KeyInfo.SelectKey(foreignEntity);

            // Empty key means that there are no referred entity
            if (this.foreignIndex.KeyInfo.IsEmptyKey(foreignKey))
            {
                return Enumerable.Empty<object>();
            }

            TPrimaryKey primaryKey = this.convertForeignToPrimary.Invoke(foreignKey);

            return this.primaryIndex.Select(primaryKey);
        }
    }
}
