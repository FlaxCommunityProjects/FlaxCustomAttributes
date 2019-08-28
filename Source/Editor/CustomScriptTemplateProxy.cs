using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Windows;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAttrbutes
{
    /// <summary>
    /// Context proxy object for script files (represented by <see cref="ScriptItem"/>).
    /// </summary>
    /// <seealso cref="FlaxEditor.Content.ContentProxy" />
    public class CustomScriptTemplateProxy : ContentProxy
    {
        private string _path;
        private readonly string _name;
        private readonly bool _isPath;
        private readonly ScriptTemplateGetterDelegate _pathGetter;
        /// <summary>
        /// The script files extension.
        /// </summary>
        public static readonly string Extension = "cs";

        /// <summary>
        /// The script files extension filter.
        /// </summary>
        public static readonly string ExtensionFiler = "*.cs";

        /// <inheritdoc />
        public override string Name => _name;

        /// <inheritdoc />
        public override bool IsProxyFor(ContentItem item)
        {
            // This is used only for template spawning, which means that this is handled by default ScriptProxy proxy
            return false;
        }

        /// <inheritdoc />
        public override bool CanCreate(ContentFolder targetLocation)
        {
            return targetLocation.CanHaveScripts;
        }

        internal CustomScriptTemplateProxy(string name, string path, bool isPath)
        {
            _name = name;
            _path = path;
            _isPath = isPath;
            _pathGetter = null;
        }

        internal CustomScriptTemplateProxy(string name, ScriptTemplateGetterDelegate path, bool isPath)
        {
            _name = name;
            _pathGetter = path;
            _isPath = isPath;
            _path = null;
        }

        /// <inheritdoc />
        public override void Create(string outputPath, object arg)
        {
            // Load template

            if(_pathGetter != null)
                _path = _pathGetter?.Invoke();

            // TODO: better naming :)
            var templatePath = _isPath ? (Path.IsPathRooted(_path) ? _path : Path.GetFullPath(_path)) : _path;
            var scriptTemplate = _isPath ? File.ReadAllText(templatePath) : templatePath;

            var scriptNamespace = Editor.Instance.ProjectInfo.Name.Replace(" ", "");

            // Get directories
            var sourceDirectory = Globals.ProjectFolder.Replace('\\', '/') + "/Source/";
            var outputDirectory = new FileInfo(outputPath).DirectoryName.Replace('\\', '/');

            // Generate "sub" namespace from relative path between source root and output path
            // NOTE: Could probably use Replace instead substring, but this is faster :)
            var subNamespaceStr = outputDirectory.Substring(sourceDirectory.Length - 1).Replace(" ", "").Replace(".", "").Replace('/', '.');

            // Replace all namespace invalid characters
            // NOTE: Need to handle number sequence at the beginning since namespace which begin with numeric sequence are invalid
            string subNamespace = string.Empty;
            bool isStart = true;
            for (int pos = 0; pos < subNamespaceStr.Length; pos++)
            {
                var c = subNamespaceStr[pos];

                if (isStart)
                {
                    // Skip characters that cannot start the sub namespace
                    if (char.IsLetter(c))
                    {
                        isStart = false;
                        subNamespace += '.';
                        subNamespace += c;
                    }
                }
                else
                {
                    // Add only valid characters
                    if (char.IsLetterOrDigit(c) || c == '_')
                        subNamespace += c;
                    // Check for sub namespace start
                    else if (c == '.')
                        isStart = true;
                }
            }

            // Append if valid
            if (subNamespace.Length > 1)
                scriptNamespace += subNamespace;

            // Format
            var scriptName = ScriptItem.CreateScriptName(outputPath);
            scriptTemplate = scriptTemplate.Replace("%class%", scriptName);
            scriptTemplate = scriptTemplate.Replace("%namespace%", scriptNamespace);

            // Save
            File.WriteAllText(outputPath, scriptTemplate, Encoding.UTF8);
        }

        /// <inheritdoc />
        public override string FileExtension => Extension;

        /// <inheritdoc />
        public override EditorWindow Open(Editor editor, ContentItem item)
        {
            Editor.Instance.CodeEditing.OpenFile(item.Path);
            return null;
        }

        /// <inheritdoc />
        public override Color AccentColor => Color.FromRGB(0x1c9c2b);
    }
}
