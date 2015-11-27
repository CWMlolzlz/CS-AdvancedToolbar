using System;
using ICities;
using UnityEngine;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;

namespace AdvancedToolbar{
	public class ToolbarCenterMod : IUserMod, ILoadingExtension{
		public static readonly string baseName = "Advanced Toolbar";
		public static readonly string modName = "CS-AdvancedToolbar";

		GameObject obj;
		public string Name{
			get{
				string output = baseName;
				return output;
			}
		}

		public string Description{
			get{return "Adds improved features to the games toolbar";}
		}

		public void OnSettingsUI(UIHelper helper){
			Options.CreateSettingsUI(helper);
		}

		public void OnCreated(ILoading loading) {
			Init();
		}
		public void OnReleased() {
			Debug.Print("Released");
		}

		public void OnLevelLoaded(LoadMode mode) {
			Init();
		}

		public void OnLevelUnloading() {
		}

		private void Init(){
			try{
				DestroyOld("AdvancedToolbar");

				//UIButton button = AddCustomButton("Test","ToolbarIcon");
				//button.tooltip = "Advanced Vehicle Options __modversion__";

				obj = new GameObject("AdvancedToolbar");
				obj.AddComponent<ToolbarCentre>();
				obj.AddComponent<ExpandableToolbar>();
			}catch(Exception e){
				Debug.ForcePrint(e);
			}
		}

		private UIButton AddCustomButton(string name,string spriteBase){
			MainToolbar toolbar = ToolsModifierControl.mainToolbar;
			UITabstrip tabstrip = toolbar.component as UITabstrip;
			MethodInfo[] spawnButtonEntryMethods = typeof(MainToolbar).GetMethods(BindingFlags.NonPublic|BindingFlags.Instance);
			foreach(MethodInfo method in spawnButtonEntryMethods){
				if(method.Name.Equals("SpawnButtonEntry") && method.GetParameters().Length == 5){
					return (UIButton)method.Invoke(toolbar,new object[]{tabstrip,name,"MAIN_TOOL",spriteBase,true});
				}
			}
			return null;
		}

		private void DestroyOld(string name) {
			while (true) {
				try {
					GameObject.DestroyImmediate(GameObject.Find(name).gameObject);
					Debug.Print("Removed " + name);
				} catch {
					break; 
				}
			}
		}
	}
}

