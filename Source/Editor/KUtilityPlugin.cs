using FlaxEditor;

namespace CustomAttrbutes
{
   
    public class KUtilityPlugin : EditorPlugin
    {
        private MainMenuAttributeManager _mainMenuManager;
        private ScriptTemplateAttributeManager _scriptTemplateManager;

        public override void InitializeEditor()
        {
            base.InitializeEditor();
            _mainMenuManager = new MainMenuAttributeManager(Editor);
            _scriptTemplateManager = new ScriptTemplateAttributeManager(Editor);
        }
        public override void Deinitialize()
        {
            _mainMenuManager?.Dispose();
            _mainMenuManager = null;

            _scriptTemplateManager?.Dispose();
            _scriptTemplateManager = null;

            base.Deinitialize();
        }
    }

    public class Test
    {
        [ScriptTemplate("Test")]
        public static string TestScriptTemplate = "using System;";

        [ScriptTemplate("Test absolute path", IsPath = true)]
        public static string TestScriptTemplate2 = "D:\\Test.cs";

        [ScriptTemplate("Test relative path - work directory", IsPath = true)]
        public static string TestScriptTemplate3 = "Test.cs";

        [ScriptTemplate("Test relative path - plugin directory", IsPath = true)]
        public static string TestScriptTemplate4 = "Test.cs";

        [ScriptTemplate("Test method")]
        public static string TestScriptTemplate5() => "using System;";


        // Can be used to append custom data like current date etc... (or based on a state of the program... only limit is that it needs to be static)
        [ScriptTemplate("Test absolute path method", IsPath = true)]
        public static string TestScriptTemplate6() => "D:\\Test.cs";

        // Doesn't work when using method as a source

        [ScriptTemplate("Test relative path method", IsPath = true)]
        public static string TestScriptTemplate7() => Utilities.GetPluginLocation<KUtilityPlugin>() + "Test.cs";
    }
}