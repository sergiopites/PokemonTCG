namespace PokemonTCG.SDK.Features.CacheManager
{
    using Infrastructure.HttpClients;
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for a cache manager implementation.
    /// </summary>
    /// <remarks>
    /// Main goal of the class is to cache the enumeration of types that are subclasses of <see cref="ResourceBase"/> in the assembly.
    /// </remarks>
    internal abstract class BaseCacheManager : IDisposable
    {
        protected static readonly ImmutableHashSet<Type> ResourceTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ApiResource))).ToImmutableHashSet();

        protected bool IsTypeSupported(Type type) => ResourceTypes.Contains(type);

        public abstract void Dispose();

        public abstract void ClearAll();
    }
}
