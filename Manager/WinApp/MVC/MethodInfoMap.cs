using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    // Map của các phương thức protected trả về Document

    public class MethodCollection : Dictionary<string, MethodInfo>
    {
        public Type Type { get; private set; }
        public MethodCollection(Type type)
        {
            Type = type;
        }
        public MethodCollection Load(Type returnType, BindingFlags flags)
        {
            foreach (var e in Type.GetMethods(flags))
            {
                if (e.ReturnType == returnType)
                {
                    Add(e.Name.ToLower(), e);
                }
            }
            return this;
        }
        public MethodInfo GetMethod(string name)
        {
            MethodInfo m;
            TryGetValue(name.ToLower(), out m);
            return m;
        }
    }
    public class MethodInfoMap : Dictionary<string, MethodCollection>
    {
        public void Add(Type type)
        {
            if (type.IsAbstract == false)
            {
                var name = type.Name.ToLower();

                MethodCollection map;
                if (!TryGetValue(name, out map))
                {
                    base.Add(name, map = new MethodCollection(type));
                }
                map.Load(typeof(Document), BindingFlags.NonPublic | BindingFlags.Instance);
            }
            foreach (var child in type.Assembly.GetTypes())
            {
                if (child.BaseType == type)
                {
                    Add(child);
                }
            }
        }
        public MethodCollection FindMethods(string typeName)
        {
            MethodCollection map;
            TryGetValue(typeName.ToLower(), out map);
            return map;
        }
        public MethodInfo Find(string typeName, string methodName)
        {
            MethodCollection map = FindMethods(typeName);
            if (map != null)
            {
                MethodInfo method;
                if (map.TryGetValue(methodName, out method)) return method;
            }
            return null;
        }
        public MethodInfo Find(Type type, string name) => Find(type.Name, name);
    }

    public class MethodInfoMap<T> : MethodInfoMap
    {
        public MethodInfoMap()
        {
            Add(typeof(T));
        }
    }
}