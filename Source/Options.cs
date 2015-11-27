using System;
using System.Xml;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedToolbar{
	public class Options{

		public delegate void OptionChangedEventHandler<T>(T value);
		public static event OptionChangedEventHandler<bool> eventAutoCentreChanged;
		public static event OptionChangedEventHandler<bool> eventScrollDirectionChanged;

		private static KeyCode hotkey = KeyCode.None;

		private static bool ctrl = false;
		private static bool alt = false;
		private static bool shift = false;
		private static bool command = false;

		public static bool IsHotkeyDown(){
			if(hotkey == KeyCode.None)
				return false;
			if(ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
				return false;
			if(alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
				return false;
			if(shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
				return false;
			if(Application.platform == RuntimePlatform.OSXPlayer && command && !(Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) //mac only button
				return false;
			if(!Input.GetKeyDown(hotkey))
				return false;
			return true;			
		}

		private static bool m_autoCentre = false;
		private static bool m_verticalScroll = false;

		static UITextField m_HotkeyField;
		static bool changed = false;

		public static bool autoCentre{
			get{return m_autoCentre;}
			set{
				m_autoCentre = value;
				if(eventAutoCentreChanged != null)
					eventAutoCentreChanged.Invoke(autoCentre);
			}
		}

		public static bool verticalScroll{
			get{ return m_verticalScroll;}
			set{ 
				m_verticalScroll = value;
				if(eventScrollDirectionChanged != null)
					eventScrollDirectionChanged.Invoke(m_verticalScroll);
			}
		}

		private static bool Changed {
			get{ return changed; }
			set{
				changed = value;

				if(changed){
					SaveOptions();
				}
			}
		}

		public static void CreateSettingsUI(UIHelperBase helper){
			
			LoadOptions();

			UIHelperBase group1 = helper.AddGroup("Toolbar Centre");
			UIHelperBase group2 = helper.AddGroup("Expandable Toolbar");
			
			group1.AddCheckbox("Automatically Centre", autoCentre, (v) => {autoCentre = v;Changed = true;});
			group2.AddCheckbox("Scroll Vertically", verticalScroll, (v) => {verticalScroll = v;Changed = true;});

			m_HotkeyField = (UITextField)group2.AddTextfield("Hotkey", hotkey == KeyCode.None ? "-" : hotkey.ToString(), (val) => {}, null);
			m_HotkeyField.eventClicked += HotkeyClicked;
			m_HotkeyField.eventKeyPress += KeyChange;
			m_HotkeyField.cursorWidth = 0;
			m_HotkeyField.canFocus = true;
			m_HotkeyField.isInteractive = true;
			m_HotkeyField.horizontalAlignment = UIHorizontalAlignment.Center;

			m_HotkeyField.normalBgSprite = "TextFieldPanel";
			m_HotkeyField.disabledBgSprite = m_HotkeyField.normalBgSprite + "Disabled";
			m_HotkeyField.focusedBgSprite = m_HotkeyField.normalBgSprite + "Focused";
			m_HotkeyField.hoveredBgSprite = m_HotkeyField.normalBgSprite + "Hovered";

			group2.AddCheckbox("Control", ctrl, (value) => {ctrl = value; UpdateText(); Changed = true;});
			group2.AddCheckbox("Alt", alt, (value) => {alt = value; UpdateText(); Changed = true;});
			group2.AddCheckbox("Shift", shift, (value) => {shift = value; UpdateText(); Changed = true;});
			if(Application.platform == RuntimePlatform.OSXPlayer){
				group2.AddCheckbox("Command", command, (value) =>{
					command = value;
					UpdateText();
					Changed = true;
				});
			}

			//m_saveButton = (UIButton)group.AddButton("Save", SaveOptions);
			//m_saveButton.color = Color.green;
			//m_saveButton.canFocus = false;
			UpdateText();
		}

		private static void KeyChange(UIComponent comp, UIKeyEventParameter eventParam){
			if(eventParam.keycode != KeyCode.None){
				hotkey = eventParam.keycode;
				eventParam.Use();
				SaveOptions();
			}
			UpdateText();
		}

		private static void HotkeyClicked(UIComponent comp, UIMouseEventParameter eventParam){
			m_HotkeyField.text = "Press a key";
		}

		private static void UpdateText(){
			if(hotkey == KeyCode.None){
				m_HotkeyField.text = "-";
			}
			string text = "";
			bool addPlus = false;
			if(ctrl){
				text += "Ctrl";
				addPlus = true;
			}
			if(alt){
				if(addPlus)
					text+="+";
				text += "Alt";
				addPlus = true;
			}
			if(shift){
				if(addPlus)
					text+="+";
				text += "Shift";
				addPlus = true;
			}
			if(command){
				if(addPlus)
					text+="+";
				text += "Cmd";
			}
			if(addPlus)
				text+="+";
			text += hotkey.ToString();

			m_HotkeyField.text = text;
		}

		private static int GetKeyCodeInt(KeyCode k){
			return (int)k;
		}

		private static readonly string m_FilePath = "ToolbarCentreOptions.xml";
		private static void LoadOptions(){ 
			try{
				XmlDocument xmldoc = new XmlDocument ();
				System.IO.FileStream fs = new System.IO.FileStream (m_FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				xmldoc.Load (fs);
				XmlNode optionNode = xmldoc.SelectSingleNode("Options");

				XmlNode toolbarCentreNode = optionNode.SelectSingleNode("AutoCentreOptions");
				if(toolbarCentreNode != null){
					autoCentre = XmlUtil.GetElementNodeBoolean(toolbarCentreNode,"AutoCentre",autoCentre);
				}

				XmlNode expandableToolbarNode = optionNode.SelectSingleNode("ExpandableToolbarOptions");
				if(expandableToolbarNode != null){
					KeyCode keycode = (KeyCode)XmlUtil.GetElementNodeInt(expandableToolbarNode,"Hotkey",(int)hotkey);
					if(keycode != KeyCode.Escape){ //ensures the hotkey is not the unsafe escape key
						hotkey = keycode;
					}

					verticalScroll = XmlUtil.GetElementNodeBoolean(expandableToolbarNode,"VerticalScroll",verticalScroll);
					alt = XmlUtil.GetElementNodeBoolean(expandableToolbarNode,"Alt",alt);
					shift = XmlUtil.GetElementNodeBoolean(expandableToolbarNode,"Shift",shift);
					ctrl = XmlUtil.GetElementNodeBoolean(expandableToolbarNode,"Control",ctrl);
					command = XmlUtil.GetElementNodeBoolean(expandableToolbarNode,"Command",command);
				}

				fs.Close();
				Changed = false;
			}catch(System.IO.FileNotFoundException e){
				Debug.ForcePrint("Unable to find options file");
			}catch(Exception e){
				Debug.ForcePrint("Unable to load options file");
				Debug.Print(e);
			}
		}

		private static void SaveOptions(){
			Debug.Print("sAving");
			Changed = false;
			XmlDocument doc = new XmlDocument();
			XmlNode optionsNode = doc.CreateElement("Options");
			doc.AppendChild(optionsNode);

			XmlNode toolbarCentreNode = doc.CreateElement("AutoCentreOptions");
			optionsNode.AppendChild(toolbarCentreNode);
			toolbarCentreNode.AppendChild(XmlUtil.CreateElementNode(doc,"AutoCentre",autoCentre));

			XmlNode expandableToolbarNode = doc.CreateElement("ExpandableToolbarOptions");
			optionsNode.AppendChild(expandableToolbarNode);
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"VerticalScroll",verticalScroll));
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"Hotkey",(int)hotkey));
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"Control",ctrl));
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"Alt",alt));
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"Shift",shift));
			expandableToolbarNode.AppendChild(XmlUtil.CreateElementNode(doc,"Command",command));

			XmlUtil.SaveFile(doc, m_FilePath);
		}
	}
}

