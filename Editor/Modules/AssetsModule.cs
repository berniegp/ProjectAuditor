using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;

namespace Unity.ProjectAuditor.Editor.Modules
{
    class AssetsModule : ProjectAuditorModule
    {
        static readonly IssueLayout k_IssueLayout = new IssueLayout
        {
            category = IssueCategory.Asset,
            properties = new[]
            {
                new PropertyDefinition { type = PropertyType.Description, name = "Asset Name"},
                new PropertyDefinition { type = PropertyType.FileType, name = "File Type", longName = "File extension"},
                new PropertyDefinition { type = PropertyType.Path, name = "Path"},
                new PropertyDefinition { type = PropertyType.Directory, name = "Directory", defaultGroup = true}
            }
        };

        public override IEnumerable<ProblemDescriptor> GetDescriptors()
        {
            yield return null;
        }

        public override IEnumerable<IssueLayout> GetLayouts()
        {
            yield return k_IssueLayout;
        }

        public override void Initialize(ProjectAuditorConfig config)
        {
        }

        public override Task<IReadOnlyCollection<ProjectIssue>> AuditAsync(ProjectAuditorParams projectAuditorParams, IProgress progress = null)
        {
            var issues = new List<ProjectIssue>();
            AnalyzeResources(issues.Add);

            return Task.FromResult<IReadOnlyCollection<ProjectIssue>>(issues.AsReadOnly());
        }

        static void AnalyzeResources(Action<ProjectIssue> onIssueFound)
        {
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var allResources = allAssetPaths.Where(path => path.IndexOf("/resources/", StringComparison.OrdinalIgnoreCase) >= 0);
            var allPlayerResources = allResources.Where(path => path.IndexOf("/editor/", StringComparison.OrdinalIgnoreCase) == -1);

            var assetPathsDict = new Dictionary<string, DependencyNode>();
            foreach (var assetPath in allPlayerResources)
            {
                if ((File.GetAttributes(assetPath) & FileAttributes.Directory) == FileAttributes.Directory)
                    continue;

                var root = AddResourceAsset(assetPath, assetPathsDict, onIssueFound, null);
                var dependencies = AssetDatabase.GetDependencies(assetPath, true);
                foreach (var depAssetPath in dependencies)
                {
                    // skip self
                    if (depAssetPath.Equals(assetPath))
                        continue;

                    AddResourceAsset(depAssetPath, assetPathsDict, onIssueFound, root);
                }
            }
        }

        static DependencyNode AddResourceAsset(
            string assetPath, Dictionary<string, DependencyNode> assetPathsDict, Action<ProjectIssue> onIssueFound, DependencyNode parent)
        {
            // skip C# scripts
            if (Path.GetExtension(assetPath).Equals(".cs"))
                return null;

            if (assetPathsDict.ContainsKey(assetPath))
            {
                var dep = assetPathsDict[assetPath];
                if (parent != null)
                    dep.AddChild(parent);
                return dep;
            }

            var location = new Location(assetPath);
            var dependencyNode = new AssetDependencyNode
            {
                location = new Location(assetPath)
            };
            if (parent != null)
                dependencyNode.AddChild(parent);

            onIssueFound(new ProjectIssue
                (
                    Path.GetFileNameWithoutExtension(location.Path),
                    IssueCategory.Asset
                ) {
                    dependencies = dependencyNode,
                    location = location
                }
            );

            assetPathsDict.Add(assetPath, dependencyNode);

            return dependencyNode;
        }
    }
}
