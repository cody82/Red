using System;
using System.Collections.Generic;

namespace RedHttpServer
{
    /// <summary>
    /// Plugin manager that
    /// </summary>
    public sealed class RPluginCollection
    {
        internal RPluginCollection()
        {
        }

        private readonly Dictionary<Type, object> _plugins = new Dictionary<Type, object>();

        /// <summary>
        ///     Register a plugin to the collection.
        ///     Should be done before starting the server
        /// </summary>s
        /// <typeparam name="TKey">The type-key to look-up</typeparam>
        /// <returns>Whether the any plugin is registered to TPluginInterface</returns>
        public void Register<TKey, TImpl>(TImpl plugin) where TImpl : class, TKey
        {
            var type = typeof(TKey);
            if (_plugins.ContainsKey(type))
                throw new RedHttpServerException("You can only register one plugin to a plugin interface");
            _plugins.Add(type, plugin);
        }

        /// <summary>
        ///     Check whether a plugin is registered to the given type-key
        /// </summary>s
        /// <typeparam name="TKey">The type-key to look-up</typeparam>
        /// <returns>Whether the any plugin is registered to TPluginInterface</returns>
        public bool IsRegistered<TKey>() => _plugins.ContainsKey(typeof(TKey));

        /// <summary>
        ///     Returns the instance of the registered plugin
        /// </summary>
        /// <typeparam name="TKey">The type-key to look-up</typeparam>
        /// <exception cref="RedHttpServerException">Throws exception when trying to use a plugin that is not registered</exception>
        public TKey Use<TKey>()
        {
            object obj;
            if (_plugins.TryGetValue(typeof(TKey), out obj))
                return (TKey)obj;
            throw new RedHttpServerException($"No plugin registered for '{typeof(TKey).Name}'");
        }
    }
}