using System.Collections.Generic;
using System.Reflection;

namespace System.Mvc
{
    public partial interface IView
    {
        void Render(object model);
        object Content { get; }
    }
    public interface IAsyncView
    {
    }
}

namespace System.Mvc
{
    public class ViewCollection : TypeCollection<IView>
    {
        public ViewCollection(Type baseViewType) : base(baseViewType) { }
        protected override string CreateKey(RequestContext context)
        {
            return $"{context.ControllerName}.{context.ActionName}";
        }
        protected override string CreateKey(Type type)
        {
            var items = type.FullName.Split('.');
            var n = items.Length - 1;

            return $"{items[n - 1]}.{items[n]}";
        }
    }
}
