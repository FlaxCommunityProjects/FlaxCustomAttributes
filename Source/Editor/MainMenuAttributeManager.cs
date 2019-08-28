using FlaxEngine;
using FlaxEditor.GUI;
using FlaxEngine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlaxEditor;

namespace CustomAttrbutes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MainMenuAttribute : Attribute
    {
        public readonly string Path;

        public MainMenuAttribute(string path) => Path = path;
    }


    internal class MainMenuAttributeManager : IDisposable
    {
        private Dictionary<string, MainMenuButton> _buttons = new Dictionary<string, MainMenuButton>();
        private List<ContextMenuButton> _items = new List<ContextMenuButton>();
        private List<ContextMenuChildMenu> _menus = new List<ContextMenuChildMenu>();

        private Editor _editor;

        public MainMenuAttributeManager(Editor editor)
        {
            _editor = editor;

            IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ad => ad.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(m => m.GetCustomAttributes<MainMenuAttribute>().Count() > 0);

            foreach (MethodInfo method in methods)
                CreateButton(method.GetCustomAttribute<MainMenuAttribute>().Path, method);
        }

        private void CreateButton(string path, MethodInfo method, ContextMenu parent = null)
        {
            string[] parts = path.Split(new char[] { '/', '\\' }, 2);

            if (parts.Length == 0) // No route = no item
                return;

            if (parts.Length == 1)
            {
                if (parent != null)
                {
                    ContextMenuButton button = parent.AddButton(parts[0], () => { try { method?.Invoke(null, null); } catch (Exception e) { Debug.LogErrorFormat("Exception calling action.\n{0}", e); } });
                    _items.Add(button);
                }
                return;
            }

            CreateButton(parts[1], method, parent == null ? GetOrAddButton(parts[0]).ContextMenu : GetOrAddChildMenu(parts[0], parent).ContextMenu);
        }

        private ContextMenuChildMenu GetOrAddChildMenu(string name, ContextMenu parent)
        {
            ContextMenuChildMenu item = parent.GetChildMenu(name);
            if (item == null)
            {
                item = parent.AddChildMenu(name);
                _menus.Add(item);
            }

            return item;
        }

        private MainMenuButton GetOrAddButton(string name)
        {
            if (_buttons.ContainsKey(name))
                return _buttons[name];

            MainMenuButton btn = _editor.UI.MainMenu.GetButton(name);

            if (btn != null)
                return btn;

            btn = _editor.UI.MainMenu.AddButton(name);
            _buttons.Add(name, btn);

            return btn;
        }

        public void Dispose()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i]?.DisposeChildren();
                _items[i]?.Dispose();
            }

            _items.Clear();

            for (int i = 0; i < _menus.Count; i++)
            {
                _menus[i]?.DisposeChildren();
                _menus[i]?.Dispose();
            }

            _menus.Clear();

            foreach (MainMenuButton item in _buttons.Values)
            {
                item?.ContextMenu?.DisposeChildren();
                item?.ContextMenu?.Dispose();
                item?.Dispose();
            }

            _buttons.Clear();
        }
    }
}
