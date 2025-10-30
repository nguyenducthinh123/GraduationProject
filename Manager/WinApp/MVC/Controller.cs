using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace System.Mvc
{
    public class ControllerCollection : TypeCollection<Controller>
    {
        protected override string CreateKey(Type type)
        {
            var name = type.Name.ToLower();
            if (name.Contains("controller"))
                name = name.Substring(0, name.Length - 10);
            return name;
        }
        protected override string CreateKey(RequestContext context)
        {
            return context.ControllerName;
        }
    }

    public abstract partial class Controller
    {
        public RequestContext RequestContext { get; set; }

        public string ControllerName
        {
            get
            {
                var name = this.GetType().Name;
                return name.Substring(0, name.Length - 10);
            }
        }

        public MethodInfo GetMethod(string name)
        {
            name = name.ToLower();
            foreach (var method in this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name.ToLower() == name)
                {
                    return method;
                }
            }
            return null;
        }

        protected virtual bool CheckMethodParams(MethodInfo method, object[] values)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != values.Length) return false;

            for (int i = 0; i < values.Length; i++)
            {
                var type = parameters[i].ParameterType;
                object v = values[i];
                if (v == null)
                {
                    if (type.IsValueType)
                    {
                        return false;
                    }
                    continue;
                }

                if (v.GetType() != type)
                {
                    try
                    {
                        values[i] = Convert.ChangeType(v, type);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public virtual MethodInfo GetMethod(string name, object[] values)
        {
            name = name.ToLower();
            foreach (var method in this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name.ToLower() != name)
                {
                    continue;
                }

                if (CheckMethodParams(method, values))
                    return method;
            }
            return null;
        }

        protected virtual object GetActionResult(string actionName, RequestValues values)
        {
            var param = values.ToArray();
            var method = GetMethod(actionName, param);

            if (method == null) 
            {
                return View(Engine.Views.CreateInstance(RequestContext), null);
            }

            return method.Invoke(this, param);
        }

        protected virtual void ExecuteCore(string actionName, RequestValues values, Action<ActionResult> callBack)
        {
            if (RequestContext == null)
                RequestContext = new RequestContext();
            RequestContext.ActionName = actionName;

            try
            {
                var result = (ActionResult)GetActionResult(actionName, values);
                if (result != null && result.Handled == false)
                {
                    if (callBack == null)
                        callBack = Engine.ValidateActionResult;

                    callBack?.Invoke(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Execute(RequestContext requestContext, Action<ActionResult> callBack)
        {
            RequestContext = requestContext;
            var name = requestContext.ActionName;
            if (string.IsNullOrEmpty(name))
            {
                name = "Index";
            }

            ExecuteCore(name, requestContext.Values, callBack);
        }

        public void Execute(string actionName, params object[] values)
        {
            ExecuteCore(actionName, values, null);
        }

        protected ActionResult View(string name, object model)
        {
            var view = Engine.Views.CreateInstance(name);
            return View(view, model);
        }

        protected ActionResult View(object model)
        {
            var view = Engine.Views.CreateInstance(Engine.RequestContext);
            return View(view, model);
        }
        protected virtual ActionResult View(IView view, object model)
        {
            if (view == null)
            {
                RequestContext.ErrorCode = "VIEW";
                return null;
            }
            view.Render(model);
            return new ActionResult {
                View = view,
                Controller = this,
            };
        }
        protected ActionResult View()
        {
            return View(null);
        }

        protected ActionResult RedirectToAction(string actionName)
        {
            return Redirect(RequestContext.ControllerName + "/" + actionName);
        }

        protected ActionResult Redirect(string url)
        {
            Engine.Execute(url);
            return new ActionResult { Handled = true };
        }

        public virtual ActionResult Done()
        {
            return new ActionResult { Handled = true };
        }

        protected virtual object Error(int code, string message)
        {
            RequestContext.ErrorCode = code.ToString();
            return new ActionResult { };
        }
        protected object Error(string message) => Error(1, message);
        protected object Error(int code) => Error(code, null);
    }
}
