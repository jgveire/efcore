// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relationship where a foreign key composed of properties on the dependent entity type
    ///     references a corresponding primary or alternate key on the principal entity type.
    /// </summary>
    public interface IForeignKey : IReadOnlyForeignKey, IAnnotatable
    {
        /// <summary>
        ///     Gets the foreign key properties in the dependent entity.
        /// </summary>
        new IReadOnlyList<IProperty> Properties { get; }

        /// <summary>
        ///     Gets the primary or alternate key that the relationship targets.
        /// </summary>
        new IKey PrincipalKey { get; }

        /// <summary>
        ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the principal entity type that this relationship targets. This may be different from the type that
        ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
        ///     hierarchy (since the key is defined on the base type of the hierarchy).
        /// </summary>
        new IEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        new INavigation? DependentToPrincipal { get; }

        /// <summary>
        ///     Gets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        new INavigation? PrincipalToDependent { get; }

        /// <summary>
        ///     Gets all skip navigations using this foreign key.
        /// </summary>
        /// <returns> The skip navigations using this foreign key. </returns>
        new IEnumerable<ISkipNavigation> GetReferencingSkipNavigations()
            => ((IReadOnlyForeignKey)this).GetReferencingSkipNavigations().Cast<ISkipNavigation>();

        /// <summary>
        ///     Gets the entity type related to the given one.
        /// </summary>
        /// <param name="entityType"> One of the entity types related by the foreign key. </param>
        /// <returns> The entity type related to the given one. </returns>
        new IEntityType GetRelatedEntityType(IReadOnlyEntityType entityType)
            => (IEntityType)((IReadOnlyForeignKey)this).GetRelatedEntityType(entityType);

        /// <summary>
        ///     Returns a navigation associated with this foreign key.
        /// </summary>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <returns>
        ///     A navigation associated with this foreign key or <see langword="null" />.
        /// </returns>
        new INavigation? GetNavigation(bool pointsToPrincipal)
            => pointsToPrincipal ? DependentToPrincipal : PrincipalToDependent;

        /// <summary>
        ///     <para>
        ///         Creates a factory for key values based on the foreign key values taken
        ///         from various forms of entity data.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <typeparam name="TKey"> The type of key instances. </typeparam>
        /// <returns> A new factory. </returns>
        IDependentKeyValueFactory<TKey>? GetDependentKeyValueFactory<TKey>();

        /// <summary>
        ///     Finds the list of properties of this foreign key that are not also properties of another foreign key, if possible,
        ///     otherwise returns all properties.
        /// </summary>
        /// <param name="fkProperties"> The foreign key properties. </param>
        /// <param name="pkProperties"> The principal key properties. </param>
        void GetPropertiesWithMinimalOverlap(
            out IReadOnlyList<IProperty> fkProperties,
            out IReadOnlyList<IProperty> pkProperties)
        {
            fkProperties = Properties;
            pkProperties = PrincipalKey.Properties;

            var count = fkProperties.Count;
            if (count == 1)
            {
                return;
            }
            
            for (var i = 0; i < count; i++)
            {
                var dependentProperty = Properties[i];

                if (dependentProperty.GetContainingForeignKeys().Count() > 1)
                {
                    if (ReferenceEquals(fkProperties, Properties))
                    {
                        fkProperties = Properties.ToList();
                        pkProperties = PrincipalKey.Properties.ToList();
                    }
                    
                    ((List<IProperty>)fkProperties).Remove(dependentProperty);
                    ((List<IProperty>)pkProperties).Remove(PrincipalKey.Properties[i]);
                }
            }
        }
    }
}
