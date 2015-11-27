using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Plugins;

namespace AdvancedToolbar{
	internal class Debug {
		private static readonly string title = "Toolbar Centre";

		static bool usingModTools = false;
		public static void Print() { }
		public static void Print(IEnumerable<object> input) {
			foreach (object o in input) {
				Print(o);
			}
		}
		public static void Print(params object[] input) {
			foreach (object o in input) {
				Print(o);
			}
		}
		public static void Print(object obj) {
			//#if DEBUG
			ForcePrint(obj);
			//#endif
		}

		public static void ForcePrint(IEnumerable<object> input) {
			foreach (object o in input) {
				ForcePrint(o);
			}
		}

		public static void ForcePrint(params object[] input) {
			foreach (object o in input) {
				ForcePrint(o);
			}
		}

		public static void ForcePrint(object obj) {
			string output = obj == null ? "null" : obj.ToString();
			if (usingModTools) {
				UnityEngine.Debug.Log(output);
			} else {
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "[" + title + "] " + output);
			}
		}
	}
}