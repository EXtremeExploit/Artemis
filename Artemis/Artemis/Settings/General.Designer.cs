﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Artemis.Settings {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class General : global::System.Configuration.ApplicationSettingsBase {
        
        private static General defaultInstance = ((General)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new General())));
        
        public static General Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TypeWave")]
        public string LastEffect {
            get {
                return ((string)(this["LastEffect"]));
            }
            set {
                this["LastEffect"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Razer BlackWidow Chroma")]
        public string LastKeyboard {
            get {
                return ((string)(this["LastKeyboard"]));
            }
            set {
                this["LastKeyboard"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnablePointersUpdate {
            get {
                return ((bool)(this["EnablePointersUpdate"]));
            }
            set {
                this["EnablePointersUpdate"] = value;
            }
        }
    }
}
