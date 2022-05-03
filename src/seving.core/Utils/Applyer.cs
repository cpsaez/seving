using Lendsum.Crosscutting.Common.Extensions;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils
{
    public static class Applyer
    {
        public static Dictionary<Type, Dictionary<Type, MethodInfo?>> cache = new Dictionary<Type, Dictionary<Type, MethodInfo?>>();

        public static async Task Apply(StreamEvent @event, StreamRoot? streamRoot, Object target)
        {
            Type eventType = @event.GetType();
            Type targetType = target.GetType();

            var eventTypeCache = cache.GetOrAdd(eventType, () => new Dictionary<Type, MethodInfo?>());
            if (eventTypeCache == null) throw new ArgumentNullException(nameof(eventTypeCache));
            MethodInfo? methodInfo = eventTypeCache.GetOrAdd(targetType, () =>
              {
                  lock (cache)
                  {
                      var typedInterface = typeof(IApplyEvent<>).MakeGenericType(eventType);
                      var targetInterface = targetType.GetInterfaces().Where(x => x.IsAssignableFrom(typedInterface)).FirstOrDefault();
                      if (targetInterface == null) return null;
                      var methodInfo = targetInterface.GetMethod("ApplyEvent");
                      return methodInfo;
                  }
              });

            if (methodInfo != null)
            {
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                await (Task)methodInfo.Invoke(target, new object[] { @event, streamRoot });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8601 // Possible null reference assignment.
            }
        }
    }
}
