﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AssemblyUpdater.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AssemblyUpdater.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The assemblies are already updated. No changes are necessary..
        /// </summary>
        internal static string ASSEMBLIES_ALREADY_UPDATED {
            get {
                return ResourceManager.GetString("ASSEMBLIES_ALREADY_UPDATED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot read current assembly version..
        /// </summary>
        internal static string CANNOT_READ_ASSEMBLY {
            get {
                return ResourceManager.GetString("CANNOT_READ_ASSEMBLY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please fill the version field in the form.
        /// </summary>
        internal static string FILL_VERSION_FIELD {
            get {
                return ResourceManager.GetString("FILL_VERSION_FIELD", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Path {0} is not correct.
        /// </summary>
        internal static string INCORRECT_PATH {
            get {
                return ResourceManager.GetString("INCORRECT_PATH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File: {0} does not contain any AssemblyInfo.
        /// </summary>
        internal static string NO_ASSEMBLY_INFO {
            get {
                return ResourceManager.GetString("NO_ASSEMBLY_INFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The selected directory does not contain .sln files.
        /// </summary>
        internal static string NO_SLN_FILES {
            get {
                return ResourceManager.GetString("NO_SLN_FILES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation completed successfully..
        /// </summary>
        internal static string OPERATION_COMPLETED {
            get {
                return ResourceManager.GetString("OPERATION_COMPLETED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a version mismatch between: {0}.
        /// </summary>
        internal static string VERSIONS_MISMATCH {
            get {
                return ResourceManager.GetString("VERSIONS_MISMATCH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a version mismatch in file {0} between AssemblyVersion: {1} and AssemblyFileVersion: {2}.
        /// </summary>
        internal static string VERSIONS_MISMATCH_FILE {
            get {
                return ResourceManager.GetString("VERSIONS_MISMATCH_FILE", resourceCulture);
            }
        }
    }
}
