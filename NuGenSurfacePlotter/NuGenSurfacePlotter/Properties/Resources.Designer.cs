﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.21006.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGenSurfacePlotter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NuGenSurfacePlotter.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Name = {0}, value = {1}, min = {2}, max = {3}.
        /// </summary>
        internal static string ColorRgb_CheckFloatException {
            get {
                return ResourceManager.GetString("ColorRgb_CheckFloatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using System;
        ///using {1};
        ///public class Eval
        ///{{
        ///	public static double acot(double x)
        ///	{{
        ///		return System.Math.PI/2 - System.Math.Atan(x);
        ///	}}
        ///	public static double abs(double x)
        ///	{{
        ///		return System.Math.Abs(x);
        ///	}}
        ///	public static double __eval(params double[] __X)
        ///	{{
        ///		{0}
        ///		return y;
        ///	}}
        ///	public static CompiledFunction __get()
        ///	{{
        ///		return __eval;
        ///	}}
        ///}}.
        /// </summary>
        internal static string FunctionCompiler_EvalClass {
            get {
                return ResourceManager.GetString("FunctionCompiler_EvalClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Function string cannot contain semicolon.
        /// </summary>
        internal static string FunctionCompiler_SemicolonException {
            get {
                return ResourceManager.GetString("FunctionCompiler_SemicolonException", resourceCulture);
            }
        }
    }
}
