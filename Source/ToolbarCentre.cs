using System;
using ICities;
using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedToolbar{
	public class ToolbarCentre : MonoBehaviour{
		private static readonly Vector2 defaultPosition = new Vector2(610.8f,-1);
		UITabstrip strip;

		public void Awake(){
			strip = (UITabstrip)ToolsModifierControl.mainToolbar.component;
			strip.eventComponentAdded += (container, child) => UpdatePosition();
			strip.eventComponentRemoved += (container, child) => UpdatePosition();
			Options.eventAutoCentreChanged += (value) => UpdatePosition();
			UpdatePosition();
		}

		public void OnDisable(){
			strip.relativePosition = defaultPosition;
		}

		private void UpdatePosition(){
			if(Options.autoCentre){
				float childrenTotalWidth = 0;
				foreach(UIComponent comp in strip.tabs){
					childrenTotalWidth += comp.width;
				}
				strip.relativePosition = new Vector2(strip.parent.width / 2 - childrenTotalWidth / 2 + 42, strip.relativePosition.y);
			}else{
				strip.relativePosition = defaultPosition;
			}

		}
	}
}

