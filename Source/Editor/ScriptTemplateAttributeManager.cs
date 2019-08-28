using FlaxEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomAttrbutes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public class ScriptTemplateAttribute : Attribute
    {
        public bool IsPath { get; set; } = false;
        public readonly string Name;
        public ScriptTemplateAttribute(string name) => Name = name;
    }
    internal delegate string ScriptTemplateGetterDelegate();

    public class ScriptTemplateAttributeManager : IDisposable
    {
        private Editor _editor;

        private List<CustomScriptTemplateProxy> _customTemplates = new List<CustomScriptTemplateProxy>();

        public ScriptTemplateAttributeManager(Editor editor)
        {
            _editor = editor;

            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(ad => ad.GetTypes());

            IEnumerable<FieldInfo> fields = types.SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
            IEnumerable<MethodInfo> methods = types.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            IEnumerable<FieldInfo> templateFields = fields.Where(m => m.GetCustomAttributes<ScriptTemplateAttribute>().Count() > 0);
            IEnumerable<MethodInfo> templateMethods = methods.Where(m => m.GetCustomAttributes<ScriptTemplateAttribute>().Count() > 0);

            foreach (var template in templateFields)
            {
                var attribute = template.GetCustomAttribute<ScriptTemplateAttribute>();

                // Unsafe conversion so it throws exception when invalid type is specified :P
                var proxy = new CustomScriptTemplateProxy(attribute.Name, (string)template.GetValue(null), attribute.IsPath);

                _editor.ContentDatabase.Proxy.Add(proxy);
                _customTemplates.Add(proxy);
            }

            foreach (var template in templateMethods)
            {
                var attribute = template.GetCustomAttribute<ScriptTemplateAttribute>();

                // Unsafe conversion so it throws exception when invalid type is specified :P
                var proxy = new CustomScriptTemplateProxy(attribute.Name, (ScriptTemplateGetterDelegate)Delegate.CreateDelegate(typeof(ScriptTemplateGetterDelegate), template), attribute.IsPath);

                _editor.ContentDatabase.Proxy.Add(proxy);
                _customTemplates.Add(proxy);
            }

        }

        public void Dispose()
        {
            for (int i = 0; i < _customTemplates.Count; i++)
            {
                int index = _editor.ContentDatabase.Proxy.IndexOf(_customTemplates[i]);

                if (index >= 0)
                    _editor.ContentDatabase.Proxy.RemoveAt(index);

                _customTemplates[i]?.Dispose();
                _customTemplates[i] = null;
            }

            _customTemplates.Clear();
        }
    }
}
