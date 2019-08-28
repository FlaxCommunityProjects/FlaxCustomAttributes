using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAttrbutes
{
    public class Utilities
    {
        /// <summary>
        /// Gets the plugin location on file system.
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <returns>The path to the plugins assembly ending with '\' </returns>
        public static string GetPluginLocation<T>() where T : Plugin => Path.GetDirectoryName(typeof(T).Assembly.Location) + '\\';
    }
}
