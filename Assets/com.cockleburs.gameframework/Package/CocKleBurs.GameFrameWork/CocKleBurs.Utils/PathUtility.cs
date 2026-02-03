
using UnityEngine;
using System.IO;

namespace CockleBurs.GameFramework.Utility
{
    public static class PathUtility
    {
        private static readonly string codeGenFolder = Path.Combine(Application.dataPath, "CodeGen");
    
        internal const string CLz_GameFrameWorkPath = "CocKleBurs.GameFrameWork/";
        internal const string ToolPath =  CLz_GameFrameWorkPath+ "Tool/";
        internal const string TutorialPath =  CLz_GameFrameWorkPath+ "Tutorial/";
        
        internal const string optimizationPaht =  CLz_GameFrameWorkPath+ "Optimization/";
      
      
        public const string   AutoLodCreateasimplifiedversionPath = optimizationPaht + "AutoLod/Create a simplified version";
        public const string   AutoLodCreateaLODGroupPath = optimizationPaht + "AutoLod/Create a LOD Group on the object";
        public const string LODGeneratorPath  =  optimizationPaht +  " LODGeneratorWindow";

        
        
        public const string SelectionHistory = CLz_GameFrameWorkPath+ "SelectionHistory";
        public const string SkinnedMesh = CLz_GameFrameWorkPath+ "SkinnedMesh";
        public const string AutoReduceMesh = CLz_GameFrameWorkPath+ "AutoReduceMesh";
        public const string OpenDataPath = CLz_GameFrameWorkPath+ "OpenDataPath(Unity)";
        public const string AutoCollider = CLz_GameFrameWorkPath+ "AutoCollider";
        public const string AutoMeshCollider = CLz_GameFrameWorkPath + "AutoMeshCollider";
        public const string GameFrameworkPackageManagerPath =  CLz_GameFrameWorkPath + "GameFrameworkPackageManager";
        public const string TodoListPath = CLz_GameFrameWorkPath + "TodoList";
        public const string ProjectInitializerPath =CLz_GameFrameWorkPath + "ProjectInitializer";
        public const string AutoBundles = CLz_GameFrameWorkPath + "AutoBundles";
        public const string GameFramework =CLz_GameFrameWorkPath + "CocKleBurs Framework";
        public const string NetworkPrefabsPath  =CLz_GameFrameWorkPath + " NetworkPrefabs";

        
        
        
        
        
        internal const string BuildConfigSwitcher =  CLz_GameFrameWorkPath + "BuildConfig/";
        public const string BuildSwitchToClient=  BuildConfigSwitcher  + "Switch to CLIENT";
        public const string BuildSwitchToServer=  BuildConfigSwitcher  + "Switch to SERVER";
        public const string BuildSwitchToDeveloper=  BuildConfigSwitcher  + " Switch To Developer";
        
        public const string AddNamespaceToolEditorPath =  ToolPath+"AddNamespaceTool";
        public const string MissingScriptsMenuFolderPath =  ToolPath+"Missing Scripts/";
        public const string ScreenshotMenuItemsPath = ToolPath +"Screenshot"+ " Screenshot";
        public const string OpenScreenshotMenuItemsPath = ToolPath +"Screenshot"+ "OpenScreenshotDirectory";
        public const string SceneShortcutPath = ToolPath +"SceneShortcut";
        public const string IconReplacerPath  = ToolPath +"IconReplacer";
        public const string ReplaceGameObjectsPath = ToolPath +"ReplaceGameObjects";
        public const string FolderShortcutPath = ToolPath +"FolderShortcut";
        public const string ManualRefreshPath = ToolPath +"ManualRefresh";
        public const string ExportPathCommand = ToolPath +"Export Command List to Markdown";
        public const string PhysicsSimulatorWindowPath = ToolPath + "Physics Simulator Window";
        public const string PrefabSpawnerWindowPath = ToolPath + "Prefab Spawner Window";

        
        public const string ComponentTabPath = ToolPath + "ComponentTab";
        
        
        public const string ExportSceneToObjPath   = ToolPath + " ExportSceneOBJ"+"ExportSceneToObj";
        public const string ExportSceneToObjAutoCuPath  = ToolPath + " ExportSceneOBJ"+"ExportSceneToObj(AutoCut)";
        public const string ExportSelectedObjPath   = ToolPath + " ExportSceneOBJ"+"ExportSelectedObj";

  
        public const string F1ManualRefreshPath = " Edit/Active GameObject _F1 ";


        public const string SceneLoaderTutorialWindowPath=  TutorialPath+ "SceneLoaderTutorial";
        public const string EventRelayTutorialWindowPath =  TutorialPath+ "EventRelayTutorial";
        public const string CoroutineTutorialWindowPath =  TutorialPath+ "CoroutineTutorial";
    
        


       public  const string AssetsFolderPath =  "Assets/" + CLz_GameFrameWorkPath;
       public const string ScriptCreatorPath = AssetsFolderPath+ "ScriptCreator";
       public const string SetIconPath = AssetsFolderPath+ "SetIcon";
       public const string PrefabVariantHelperPath =AssetsFolderPath +  "Prefab Variant Helper";
       public const string PackageManifestConfigPath = AssetsFolderPath+ "PackageManifestConfig";


       //   public const string Com
       

       /// <summary>
        ///     Gets the path where the game's exe is
        ///     <para>(Or with the editor it is under the `/Game` folder)</para>
        /// </summary>
        /// <returns></returns>
        
        public static string GetGameExecutePath()
        {
#if UNITY_EDITOR
            return Directory.GetParent(Application.dataPath)?.FullName + "/Game";
#else
			return Directory.GetParent(Application.dataPath).FullName;
#endif
        }
        
        // 获取 CodeGen 文件夹路径
        public static string GetCodeGenFolderPath()
        {
            EnsureCodeGenFolderExists();
            return codeGenFolder;
        }
    
        // 确保 CodeGen 文件夹存在
        private static void EnsureCodeGenFolderExists()
        {
            if (!Directory.Exists(codeGenFolder))
            {
                Directory.CreateDirectory(codeGenFolder);
                Debug.Log("CodeGen 文件夹已创建。");
            }
        }
    
        // 获取脚本文件的完整路径
        public static string GetScriptPath(string scriptName)
        {
            return Path.Combine(GetCodeGenFolderPath(), scriptName);
        }
    }
}
