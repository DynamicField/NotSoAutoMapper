using System;

namespace HandmadeMapper.Extensions.Ioc.Base
{
    /// <summary>
    ///     Specifies the mapper factory method as being lazily evaluated,
    ///     which means that the expression will be transformed once it is requested.
    /// </summary>
    /// <seealso cref="MapperOptions{TSource,TTarget}.IsLazy" />
    [AttributeUsage(AttributeTargets.Method)]
    public class LazyAttribute : Attribute
    {
    }
}