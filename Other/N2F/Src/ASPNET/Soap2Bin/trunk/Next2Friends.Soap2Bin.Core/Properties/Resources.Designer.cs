﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Next2Friends.Soap2Bin.Core.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Next2Friends.Soap2Bin.Core.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Stream is not readable..
        /// </summary>
        internal static string Argument_StreamNotReadable {
            get {
                return ResourceManager.GetString("Argument_StreamNotReadable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream is not writable..
        /// </summary>
        internal static string Argument_StreamNotWritable {
            get {
                return ResourceManager.GetString("Argument_StreamNotWritable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Too many bytes in what should have been a 7 bit encoded Int32..
        /// </summary>
        internal static string Format_Bad7BitInt32 {
            get {
                return ResourceManager.GetString("Format_Bad7BitInt32", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The string was not in the proper format. Expected format is &quot;YYYYMMDDHHMMSS&quot; but the string was {0}..
        /// </summary>
        internal static string Format_DateTimeString {
            get {
                return ResourceManager.GetString("Format_DateTimeString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DataInputStream encountered an invalid string length of {0} characters..
        /// </summary>
        internal static string IO_InvalidStringLength {
            get {
                return ResourceManager.GetString("IO_InvalidStringLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read beyond the end of the stream..
        /// </summary>
        internal static string IO_ReadBeyondEOF {
            get {
                return ResourceManager.GetString("IO_ReadBeyondEOF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot access a closed file..
        /// </summary>
        internal static string ObjectDisposed_FileClosed {
            get {
                return ResourceManager.GetString("ObjectDisposed_FileClosed", resourceCulture);
            }
        }
    }
}
