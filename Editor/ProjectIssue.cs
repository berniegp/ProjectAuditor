using System;
using Unity.ProjectAuditor.Editor.CodeAnalysis;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEngine;

namespace Unity.ProjectAuditor.Editor
{
    /// <summary>
    /// Affected area
    /// </summary>
    public enum Area
    {
        /// <summary>
        /// CPU Performance
        /// </summary>
        CPU,

        /// <summary>
        /// GPU Performance
        /// </summary>
        GPU,

        /// <summary>
        /// Memory consumption
        /// </summary>
        Memory,

        /// <summary>
        /// Application size
        /// </summary>
        BuildSize,

        /// <summary>
        /// Load times
        /// </summary>
        LoadTimes,

        /// <summary>
        /// All areas
        /// </summary>
        All
    }

    public enum IssueCategory
    {
        Assets,
        Shaders,
        ShaderCompilationLog,
        ShaderVariants,
        Code,
        ProjectSettings,
        NumCategories
    }

    /// <summary>
    /// ProjectAuditor Issue found in the current project
    /// </summary>
    [Serializable]
    public class ProjectIssue
    {
        public DependencyNode dependencies;
        public IssueCategory category;

        public string description;
        public ProblemDescriptor descriptor;
        public Location location;

        [SerializeField] string[] customProperties;

        /// <summary>
        /// ProjectIssue constructor
        /// </summary>
        /// <param name="descriptor"> descriptor </param>
        /// <param name="description"> Issue-specific description of the problem </param>
        /// <param name="category"> Issue category </param>
        /// <param name="location"> Issue address </param>
        public ProjectIssue(ProblemDescriptor descriptor,
                            string description,
                            IssueCategory category,
                            Location location = null)
        {
            this.descriptor = descriptor;
            this.description = description;
            this.category = category;
            this.location = location;
        }

        public ProjectIssue(ProblemDescriptor descriptor,
                            string description,
                            IssueCategory category,
                            CallTreeNode dependenciesNode)
        {
            this.descriptor = descriptor;
            this.description = description;
            this.category = category;
            dependencies = dependenciesNode;
        }

        public string filename
        {
            get
            {
                return location == null ? string.Empty : location.Filename;
            }
        }

        public string relativePath
        {
            get
            {
                return location == null ? string.Empty : location.Path;
            }
        }

        public int line
        {
            get
            {
                return location == null ? 0 : location.Line;
            }
        }

        public string callingMethod
        {
            get
            {
                if (dependencies == null)
                    return string.Empty;
                if (!dependencies.HasChildren())
                    return string.Empty;

                var callTree = dependencies.GetChild() as CallTreeNode;
                if (callTree == null)
                    return string.Empty;
                return callTree.name;
            }
        }

        public bool isPerfCriticalContext
        {
            get
            {
                return descriptor.critical || (dependencies != null && dependencies.IsPerfCritical());
            }
        }

        public Rule.Severity severity
        {
            get
            {
                if (descriptor.critical || (dependencies != null && dependencies.IsPerfCritical()))
                    return Rule.Severity.Warning;
                return descriptor.severity;
            }
        }

        public string name
        {
            get
            {
                if (dependencies == null)
                    return string.Empty;
                var prettyName = dependencies.prettyName;
                if (prettyName.Equals(descriptor.description))
                    // if name matches the descriptor's name, use caller's name instead
                    return string.IsNullOrEmpty(callingMethod) ? string.Empty : dependencies.GetChild().prettyName;
                return prettyName;
            }
        }

        public int GetNumCustomProperties()
        {
            return customProperties != null ? customProperties.Length : 0;
        }

        public string GetCustomProperty(int index)
        {
            return customProperties != null ? customProperties[index] : string.Empty;
        }

        internal int GetCustomPropertyAsInt(int index)
        {
            var valueAsString = GetCustomProperty(index);
            var value = 0;
            if (!int.TryParse(valueAsString, out value))
                return 0;
            return value;
        }

        public void SetCustomProperties(string[] properties)
        {
            customProperties = properties;
        }
    }
}
