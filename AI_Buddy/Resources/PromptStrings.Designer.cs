﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AI_Buddy.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PromptStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PromptStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AI_Buddy.Resources.PromptStrings", typeof(PromptStrings).Assembly);
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
        ///   Looks up a localized string similar to Write a method in {0}, using the following text: {1}.
        /// </summary>
        internal static string FnxFromHighlightedTextPrompt {
            get {
                return ResourceManager.GetString("FnxFromHighlightedTextPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How can this {0} code be improved: {1}.
        /// </summary>
        internal static string SuggestCodeImprovements {
            get {
                return ResourceManager.GetString("SuggestCodeImprovements", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Comment the following {0} code: {1}.
        /// </summary>
        internal static string SuggestCommentsForCode {
            get {
                return ResourceManager.GetString("SuggestCommentsForCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Write a unit test in {0} for the testing framework {1}, for this code: {2}.
        /// </summary>
        internal static string UnitTestPrompt {
            get {
                return ResourceManager.GetString("UnitTestPrompt", resourceCulture);
            }
        }
    }
}
