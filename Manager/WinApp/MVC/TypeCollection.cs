using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Mvc
{
    public abstract class TypeCollection<T>
    {
        Dictionary<string, Type> map = new Dictionary<string, Type>();
        public TypeCollection() : this(typeof(T)) { }
        public TypeCollection(Type baseType)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (!type.IsAbstract && type.IsSubclassOf(baseType))
                    {
                        map.Add(CreateKey(type).ToLower(), type);
                    }
                }
            }
        }
        public T CreateInstance(string name)
        {
            Type type;
            if (map.TryGetValue(name.ToLower(), out type))
                return (T)Activator.CreateInstance(type);
            return default(T);
        }
        protected abstract string CreateKey(Type type);
        protected abstract string CreateKey(RequestContext context);
        public T CreateInstance(RequestContext context) => (T)CreateInstance(CreateKey(context));
    }
}
