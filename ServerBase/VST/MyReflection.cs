using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    public static class MyReflection
    {
        static public string ToCodeName(this string s)
        {
            var v = new char[s.Length];
            var i = 0;
            foreach (char c in s)
            {
                if (c == '_' || char.IsDigit(c))
                {
                    v[i++] = c;
                    continue;
                }
                if (char.IsLetter(c))
                    v[i++] = char.ToLower(c);
            }
            return new string(v, 0, i);
        }

        static public MethodInfo FindMethod(this Type type, string name)
        {
            name = name.ToCodeName();
            foreach (var method in type.GetMethods())
            {
                if (method.Name.ToLower() == name)
                {
                    return method;
                }
            }
            return null;
        }
        static public MethodInfo FindMethod(this object any, string name) => FindMethod(any.GetType(), name);
    }
}
