using System;

namespace HandmadeMapper.Extensions.Ioc.Base
{
    /// <summary>
    ///     Excludes a static method from as being treated as a mapper factory (using <c>AddMappersFrom</c>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExcludeMapperAttribute : Attribute
    {
    }
}