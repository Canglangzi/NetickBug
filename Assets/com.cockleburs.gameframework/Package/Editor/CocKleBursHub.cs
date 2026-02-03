using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CocKleBurs.Editor
{
    public class CocKleBursHub : EditorWindow
    {
        private const string MainPackageUrl = "https://github.com/Canglangzi/CocKleBursDevelopmentKit.git";
        private static readonly Dictionary<string, string> Dependencies = new Dictionary<string, string>
        {
            { "com.whinarn.unitymeshsimplifier", "https://github.com/Whinarn/UnityMeshSimplifier.git" },
            { "com.karrar.netick", "https://github.com/karrarrahim/NetickProForUnity.git" },
            { "com.cysharp.unitask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
            { "com.code-philosophy.hybridclr", "https://github.com/focus-creative-games/hybridclr.git" },
            { "com.kyrylokuzyk.primetween", "https://github.com/Canglangzi/PrimeTween.git" }
        };
        
        private static readonly Dictionary<string, string> RecommendedVersions = new Dictionary<string, string>
        {
            { "com.cysharp.unitask", "2.4.0" },
            { "com.code-philosophy.hybridclr", "v4.0.2" }
        };
        
        private Vector2 scrollPosition;
        private bool includeVersionTags = true;
        private bool showAdvancedOptions;
        private Dictionary<string, bool> installationStatus = new Dictionary<string, bool>();
        private bool macroEnabled;
        private bool showMacroSection = true;
        
        // 新增：选择状态字典
        private Dictionary<string, bool> selectionStatus = new Dictionary<string, bool>();
        private bool selectAllState = true; // 全选状态

        [MenuItem("CocKleBurs.GameFrameWork/Hub")]
        public static void ShowWindow()
        {
            var window = GetWindow<CocKleBursHub>("CocKleBurs Hub", true);
            window.minSize = new Vector2(500, 600);
            window.RefreshInstallationStatus();
        }

        private void OnEnable()
        {
            RefreshInstallationStatus();
            CheckMacroStatus();
            
            // 初始化选择状态
            foreach (var dep in Dependencies)
            {
                if (!selectionStatus.ContainsKey(dep.Key))
                {
                    selectionStatus[dep.Key] = true; // 默认选中
                }
            }
            
        }

        private void CheckMacroStatus()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            macroEnabled = defines.Contains("COCKLEBURS_GAMEFRAMEWORK");
        }

        private void ToggleFrameworkMacro()
        {
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            
            StringBuilder newDefines = new StringBuilder();
            bool found = false;
            
            foreach (string define in currentDefines.Split(';'))
            {
                if (define == "COCKLEBURS_GAMEFRAMEWORK")
                {
                    found = true;
                    continue;
                }
                
                if (!string.IsNullOrEmpty(define))
                {
                    if (newDefines.Length > 0) newDefines.Append(';');
                    newDefines.Append(define);
                }
            }
            
            if (!macroEnabled)
            {
                if (newDefines.Length > 0) newDefines.Append(';');
                newDefines.Append("COCKLEBURS_GAMEFRAMEWORK");
            }
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, 
                newDefines.ToString());
            
            AssetDatabase.Refresh();
            CheckMacroStatus();
            ImportRequiredPackages();
            Debug.Log($"Framework macros {(macroEnabled ? "disabled" : "enabled")}");
        }
        private void ImportRequiredPackages()
        {
            string[] requiredPackages = new string[]
            {
                "Packages/com.cockleburs.gameframework/Package/CocKleBurs.GameFrameWork/CocKleBurs.Resources/Resources/UnityPackage/Hot Reload Edit Code Without Compiling.unitypackage",
                "Packages/com.cockleburs.gameframework/Package/CocKleBurs.GameFrameWork/CocKleBurs.Resources/Resources/UnityPackage/Odin Inspector and Serializer.unitypackage"
            };
            
            foreach (var packagePath in requiredPackages)
            {
                if (File.Exists(packagePath))
                {
                    AssetDatabase.ImportPackage(packagePath, false);
                    Debug.Log($"Imported required package: {Path.GetFileName(packagePath)}");
                }
                else
                {
                    Debug.LogWarning($"Required package not found: {packagePath}");
                }
            }
            
            // 刷新安装状态
            RefreshInstallationStatus();
        }

        private void RefreshInstallationStatus()
        {
            string manifestContent = GetManifestContent();
            installationStatus.Clear();

            foreach (var dep in Dependencies)
            {
                string searchPattern = $"\"{dep.Key}\":";
                installationStatus[dep.Key] = manifestContent.Contains(searchPattern);
            }
            
            installationStatus["com.cockleburs.gameframework"] = 
                manifestContent.Contains("\"com.cockleburs.gameframework\":");
        }

        private void OnGUI()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0f, 0.3f) }
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.3f, 0.3f, 0.3f) }
            };
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 30,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("CocKleBurs Game Framework", headerStyle);
            EditorGUILayout.Space(15);
            
            // Macro Section
            showMacroSection = EditorGUILayout.BeginFoldoutHeaderGroup(showMacroSection, "Framework Macros");
            if (showMacroSection)
            {
                EditorGUILayout.BeginVertical("Box");
                
                GUIStyle macroStatusStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = macroEnabled ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.8f, 0.2f, 0.2f) }
                };
                
                EditorGUILayout.LabelField("Current Status: " + 
                                           (macroEnabled ? "ENABLED" : "DISABLED"), macroStatusStyle);
                
                EditorGUILayout.HelpBox("The 'COCKLEBURS_GAMEFRAMEWORK' macro controls framework-specific " +
                                        "code compilation. Enable for full framework features, disable for compatibility mode.",
                    MessageType.Info);
                
                if (GUILayout.Button(macroEnabled ? "Disable Framework Macros" : "Enable Framework Macros", 
                        GUILayout.Height(30)))
                {
                    ToggleFrameworkMacro();
                }
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Main package info
            EditorGUILayout.LabelField("Core Framework", titleStyle);
            DrawPackageInfo("com.cockleburs.gameframework", MainPackageUrl, installationStatus.GetValueOrDefault("com.cockleburs.gameframework", false));
            EditorGUILayout.Space(10);
            
            // Dependencies section with selection
            EditorGUILayout.LabelField("Required Dependencies", titleStyle);
            EditorGUILayout.HelpBox("Select the dependencies you want to install.", MessageType.Info);
            
            // 新增：全选/取消全选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(selectAllState ? "Deselect All" : "Select All", GUILayout.Width(100)))
            {
                ToggleSelectAll();
            }
            
            EditorGUILayout.LabelField($"Selected: {GetSelectedCount()} of {Dependencies.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var dep in Dependencies)
            {
                DrawPackageInfoWithSelection(dep.Key, dep.Value, installationStatus.GetValueOrDefault(dep.Key, false));
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(15);
            
            // Options
            EditorGUILayout.BeginVertical("Box");
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true);
            
            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                includeVersionTags = EditorGUILayout.Toggle("Add Version Tags", includeVersionTags);
                EditorGUILayout.HelpBox("Version tags ensure dependency stability by locking to specific versions.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            
            // Action buttons
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();
            
            // 新增：安装所选按钮
            if (GUILayout.Button("Install Selected", buttonStyle, GUILayout.Height(40)))
            {
                InstallSelectedDependencies();
            }
            
            if (GUILayout.Button("Install All", buttonStyle, GUILayout.Height(40)))
            {
                InstallAllDependencies();
            }
            
            if (GUILayout.Button("Refresh Status", buttonStyle, GUILayout.Height(40)))
            {
                RefreshInstallationStatus();
            }
            
            GUILayout.EndHorizontal();
            
            // Status message
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("After installation, Unity will automatically resolve dependencies. " +
                                    "This may take a few minutes depending on your internet connection.", 
                                    MessageType.Warning);
        }

        // 新增：带选择框的包信息显示
        private void DrawPackageInfoWithSelection(string packageName, string url, bool isInstalled)
        {
            EditorGUILayout.BeginVertical("Box");
            
            GUILayout.BeginHorizontal();
            
            // 选择框
            bool newSelectionState = EditorGUILayout.Toggle(selectionStatus[packageName], GUILayout.Width(20));
            if (newSelectionState != selectionStatus[packageName])
            {
                selectionStatus[packageName] = newSelectionState;
                UpdateSelectAllState();
            }
            
            // 状态指示器
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = isInstalled ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.8f, 0.2f, 0.2f) }
            };
            
            EditorGUILayout.LabelField(isInstalled ? "✓ INSTALLED" : "● NOT INSTALLED", statusStyle, GUILayout.Width(100));
            
            // 包名
            EditorGUILayout.LabelField(packageName, EditorStyles.boldLabel);
            
            GUILayout.EndHorizontal();
            
            // URL
            EditorGUILayout.LabelField(url, EditorStyles.miniLabel);
            
            // 推荐版本
            if (RecommendedVersions.ContainsKey(packageName))
            {
                EditorGUILayout.LabelField($"Recommended Version: {RecommendedVersions[packageName]}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawPackageInfo(string packageName, string url, bool isInstalled)
        {
            EditorGUILayout.BeginVertical("Box");
            
            GUILayout.BeginHorizontal();
            
            // 状态指示器
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = isInstalled ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.8f, 0.2f, 0.2f) }
            };
            
            EditorGUILayout.LabelField(isInstalled ? "✓ INSTALLED" : "● NOT INSTALLED", statusStyle, GUILayout.Width(100));
            
            // 包名
            EditorGUILayout.LabelField(packageName, EditorStyles.boldLabel);
            
            GUILayout.EndHorizontal();
            
            // URL
            EditorGUILayout.LabelField(url, EditorStyles.miniLabel);
            
            // 推荐版本
            if (RecommendedVersions.ContainsKey(packageName))
            {
                EditorGUILayout.LabelField($"Recommended Version: {RecommendedVersions[packageName]}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        // 新增：安装所选依赖项
        private void InstallSelectedDependencies()
        {
            int selectedCount = GetSelectedCount();
            if (selectedCount == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select at least one dependency to install.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Install Selected Dependencies", 
                $"This will install {selectedCount} selected dependencies to your project's manifest.json. Continue?",
                "Install", "Cancel"))
            {
                string manifestContent = GetManifestContent();
                string newManifest = AddOrUpdateSelectedDependencies(manifestContent);
                SaveManifest(newManifest);
                RefreshInstallationStatus();
                
                EditorUtility.DisplayDialog("Installation Complete", 
                    "Selected dependencies have been added to manifest.json.\n\n" +
                    "Unity will now start resolving packages. This may take a few minutes.", 
                    "OK");
            }
        }

        private void InstallAllDependencies()
        {
            if (EditorUtility.DisplayDialog("Install Dependencies", 
                "This will modify your project's manifest.json file. Make sure you have a backup. Continue?",
                "Install", "Cancel"))
            {
                string manifestContent = GetManifestContent();
                string newManifest = AddOrUpdateDependencies(manifestContent);
                SaveManifest(newManifest);
                RefreshInstallationStatus();
                
                EditorUtility.DisplayDialog("Installation Complete", 
                    "Dependencies have been added to manifest.json.\n\n" +
                    "Unity will now start resolving packages. This may take a few minutes.", 
                    "OK");
            }
        }

        // 新增：只添加选中的依赖项
        private string AddOrUpdateSelectedDependencies(string manifestContent)
        {
            // 确保依赖部分存在
            if (!manifestContent.Contains("\"dependencies\":"))
            {
                manifestContent = manifestContent.Replace("{", "{\n  \"dependencies\": {\n  },");
            }
            
            // 添加/更新主框架
            manifestContent = AddOrUpdateDependency(manifestContent, "com.cockleburs.gameframework", MainPackageUrl);
            
            // 添加/更新选中的依赖项
            foreach (var dep in Dependencies)
            {
                if (selectionStatus[dep.Key])
                {
                    string url = dep.Value;
                    
                    // 添加版本标签（如果可用且请求）
                    if (includeVersionTags && RecommendedVersions.TryGetValue(dep.Key, out string version))
                    {
                        url += "#" + version;
                    }
                    
                    manifestContent = AddOrUpdateDependency(manifestContent, dep.Key, url);
                }
            }
            
            return manifestContent;
        }

        private string AddOrUpdateDependencies(string manifestContent)
        {
            // 确保依赖部分存在
            if (!manifestContent.Contains("\"dependencies\":"))
            {
                manifestContent = manifestContent.Replace("{", "{\n  \"dependencies\": {\n  },");
            }
            
            // 添加/更新主框架
            manifestContent = AddOrUpdateDependency(manifestContent, "com.cockleburs.gameframework", MainPackageUrl);
            
            // 添加/更新所有依赖项
            foreach (var dep in Dependencies)
            {
                string url = dep.Value;
                
                // 添加版本标签（如果可用且请求）
                if (includeVersionTags && RecommendedVersions.TryGetValue(dep.Key, out string version))
                {
                    url += "#" + version;
                }
                
                manifestContent = AddOrUpdateDependency(manifestContent, dep.Key, url);
            }
            
            return manifestContent;
        }

        private string AddOrUpdateDependency(string manifestContent, string packageName, string packageUrl)
        {
            string dependencyLine = $"\"{packageName}\": \"{packageUrl}\"";
            string searchPattern = $"\"{packageName}\":";
            
            // 如果已经存在，更新它
            if (manifestContent.Contains(searchPattern))
            {
                string pattern = $"\"{packageName}\":\\s*\"[^\"]+\"";
                return Regex.Replace(manifestContent, pattern, dependencyLine, RegexOptions.Multiline);
            }
            
            // 如果不存在，添加新依赖项
            int dependenciesIndex = manifestContent.IndexOf("\"dependencies\":");
            if (dependenciesIndex == -1) return manifestContent;
            
            int startIndex = manifestContent.IndexOf('{', dependenciesIndex);
            if (startIndex == -1) return manifestContent;
            
            // 找到依赖对象的结束位置
            int braceCount = 1;
            int currentIndex = startIndex + 1;
            while (braceCount > 0 && currentIndex < manifestContent.Length)
            {
                if (manifestContent[currentIndex] == '{') braceCount++;
                else if (manifestContent[currentIndex] == '}') braceCount--;
                currentIndex++;
            }
            
            if (braceCount == 0)
            {
                int insertPosition = currentIndex - 1;
                
                // 检查依赖对象是否为空
                bool isEmptyDependencyBlock = manifestContent.Substring(startIndex + 1, (currentIndex - 1) - (startIndex + 1))
                    .Trim().Length == 0;
                
                // 添加新依赖项（根据是否为空对象决定是否添加逗号）
                string insertion;
                if (isEmptyDependencyBlock)
                {
                    insertion = $"\n    {dependencyLine}";
                }
                else
                {
                    // 检查最后一个字符是否是逗号
                    char lastChar = manifestContent[insertPosition - 1];
                    bool needsComma = lastChar != ',' && !char.IsWhiteSpace(lastChar);
                    
                    insertion = (needsComma ? "," : "") + $"\n    {dependencyLine}";
                }
                
                return manifestContent.Insert(insertPosition, insertion);
            }
            
            return manifestContent;
        }

        // 新增：切换全选状态
        private void ToggleSelectAll()
        {
            selectAllState = !selectAllState;
            foreach (var key in selectionStatus.Keys)
            {
                selectionStatus[key] = selectAllState;
            }
        }

        // 新增：更新全选状态
        private void UpdateSelectAllState()
        {
            int selectedCount = 0;
            foreach (var value in selectionStatus.Values)
            {
                if (value) selectedCount++;
            }
            
            selectAllState = selectedCount == selectionStatus.Count;
        }

        // 新增：获取选中数量
        private int GetSelectedCount()
        {
            int count = 0;
            foreach (var value in selectionStatus.Values)
            {
                if (value) count++;
            }
            return count;
        }

        private string GetManifestContent()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            return File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "{\n}";
        }

        private void SaveManifest(string content)
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            File.WriteAllText(manifestPath, content);
            AssetDatabase.Refresh();
        }
    }
}