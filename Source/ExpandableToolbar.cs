using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedToolbar{
	public class ExpandableToolbar : MonoBehaviour{

		private static readonly Vector2 defaultSize = new Vector2(859,109);
		private static readonly Vector3 defaultPosition = new Vector3(596,-110);

		private static readonly Vector2 defaultPositionDecrementButton = new Vector2(16, 33);
		private static readonly Vector2 defaultPositionIncrementButton = new Vector2(812, 33);

		private int rows = 3;

		private UIButton m_expandButton;
		private UITabContainer m_TBTabContainer;

		private UIPanel currentGroupPanel;
		private UIScrollablePanel currentScrollablePanel;
		private UIScrollbar currentScrollbar;

		private bool m_expanded = false;

		private Dictionary<UIPanel,UIScrollbar> m_verticalScrollbars = new Dictionary<UIPanel, UIScrollbar>();

		public UITabContainer toolbarTabContainer{
			get{ return m_TBTabContainer; }
			set{ m_TBTabContainer = value; }
		}

		public bool expanded{
			get{ return m_expanded;}
			set{
				m_expanded = value;
			}
		}

		public void Awake(){
			InitUI();
		}

		private void InitUI(){
			if(toolbarTabContainer == null){
				MainToolbar toolbar = ToolsModifierControl.mainToolbar;
				UITabstrip tabstrip = (UITabstrip)toolbar.component;
				toolbarTabContainer = tabstrip.tabContainer;
			}

			Options.eventScrollDirectionChanged += (bool value) =>{
				if(expanded)
					Expand(value);				
			};

			if(m_expandButton == null){
				UIView view = UIView.GetAView();
				UIMultiStateButton advisorButton = view.FindUIComponent<UIMultiStateButton>("AdvisorButton");
				m_expandButton = (UIButton)view.AddUIComponent(typeof(UIButton));
				m_expandButton.absolutePosition = advisorButton.absolutePosition + new Vector3(advisorButton.width,0);
				var atlas = TextureLoader.CreateTextureAtlas("ExapandCollapseIcons","ExpandCollapseIcons",TextureLoader.FileDirectory.Images, new string[]{
					"Expand","Collapse","OptionBase","OptionBaseDisabled","OptionBaseHovered","OptionBaseFocused"
				},2,3);
				//m_expandButton.text = "+";
				m_expandButton.atlas = atlas;
				m_expandButton.name = "ExpandToolbarButton";
				m_expandButton.isInteractive = true;
				m_expandButton.scaleFactor = 0.76f;
				m_expandButton.size = new Vector2(36,36);
				m_expandButton.normalBgSprite = "OptionBase";
				m_expandButton.normalFgSprite = "Expand";
				m_expandButton.disabledBgSprite = "OptionBaseDisabled";
				m_expandButton.hoveredBgSprite = "OptionBaseHovered";
				m_expandButton.eventClicked += (component, eventParam) => {
					ToggleExpand();
				};
			}
		}

		public void Start(){
			
		}

		public void Update(){
			try{
				if(Options.IsHotkeyDown()){
					ToggleExpand();
				}
			}catch(Exception e){
				Debug.Print(e);
			}
		}

		private void ToggleExpand(){
			expanded = !expanded;
			if(expanded){
				Expand(Options.verticalScroll);
			} else{
				Contract();
			}
			if(m_expandButton != null){
				//m_expandButton.normalBgSprite = expanded ? "OptionBaseFocused": "OptionBase";
				m_expandButton.normalFgSprite = expanded ? "Collapse": "Expand";
			}
		}

		private void Expand(bool verticalScroll){
			Debug.Print("Expanded");
			var container = toolbarTabContainer;
			container.height = defaultSize.y * rows;
			container.relativePosition = defaultPosition + new Vector3(0,-109) * (rows-1);

			foreach(UIComponent comp in container.components){
				//Debug.Print(comp);
				if(comp is UIPanel){
					ApplyToIconPanel((UIPanel)comp, ExpandIconPanel, verticalScroll);
				}
			}
			container.Invalidate();

		}

		private void Contract(){
			Debug.Print("UnExpanded");
			var container = toolbarTabContainer;
			container.size = defaultSize;
			container.relativePosition = defaultPosition;

			foreach(UIComponent comp in container.components){
				//Debug.Print(comp);
				if(comp is UIPanel){
					ApplyToIconPanel((UIPanel)comp, CollapseIconPanel, false);
				}
			}
			container.Invalidate();
		}

		private delegate void IconPanelModifier(UIPanel panel, UIScrollablePanel scrollablePanel, UIScrollbar scrollbar, bool verticalScroll);

		private void ApplyToIconPanel(UIPanel groupPanel, IconPanelModifier modifier, bool verticalScroll){
			if(groupPanel == null)
				return;
			if(modifier == null)
				return;
			try{ //seperator panels do not have UIScrollablePanels nor UIScrollbars
				UITabContainer tabContainer = groupPanel.GetComponentInChildren<UITabContainer>();
				foreach(UIComponent comp in tabContainer.components){ //for each displayable panel of icons
					if(comp is UIPanel){
						UIPanel panel = comp as UIPanel;
						UIScrollablePanel sp = panel.GetComponentInChildren<UIScrollablePanel>();
						UIScrollbar sb = panel.GetComponentInChildren<UIScrollbar>();
						modifier.Invoke(panel,sp,sb,verticalScroll);
						panel.Invalidate();
						sp.Invalidate();
						sb.Invalidate();
					}
				}
				tabContainer.Invalidate();
			}catch(Exception e){
				//Debug.Print(groupPanel,e);
			}
		}

		private void ExpandIconPanel(UIPanel panel, UIScrollablePanel scrollablePanel, UIScrollbar horizontalScrollbar, bool verticalScroll){
			panel.height *= defaultSize.y * rows;
			horizontalScrollbar.value = 0;


			//adjust layout of UIScollablePanel
			scrollablePanel.autoLayout = true;
			scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
			scrollablePanel.wrapLayout = true;
			scrollablePanel.width++;

			UIScrollbar verticalScrollbar;
			try{
				verticalScrollbar = m_verticalScrollbars[panel];
			}catch{				
				verticalScrollbar = CreateVerticalScrollbar(panel,scrollablePanel);
				m_verticalScrollbars[panel] = verticalScrollbar;
			}

			if(verticalScroll){
				scrollablePanel.autoLayoutDirection = LayoutDirection.Horizontal;
				verticalScrollbar.Show();
			} else{
				verticalScrollbar.Hide();
				scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
				horizontalScrollbar.decrementButton.relativePosition = new Vector3(defaultPositionDecrementButton.x, panel.height / 2.0f - 21.5f);
				horizontalScrollbar.incrementButton.relativePosition = new Vector3(defaultPositionIncrementButton.x, panel.height / 2.0f - 21.5f);
			}
		}

		private void CollapseIconPanel(UIPanel panel, UIScrollablePanel scrollablePanel, UIScrollbar horizontalScrollbar, bool verticalScroll){
			panel.height = defaultSize.y;
			panel.width = defaultSize.x;

			try{
				m_verticalScrollbars[panel].Hide();
			}catch{}

			//adjust layout of UIScollablePanel
			scrollablePanel.autoLayout = true;
			scrollablePanel.autoLayoutStart = LayoutStart.BottomLeft;
			scrollablePanel.wrapLayout = false;
			scrollablePanel.autoLayoutDirection = LayoutDirection.Horizontal;
			scrollablePanel.width = 763.0f;
			//adjust position of Scollbar buttons
			horizontalScrollbar.decrementButton.relativePosition = defaultPositionDecrementButton;
			horizontalScrollbar.incrementButton.relativePosition = defaultPositionIncrementButton;
		}

		public void OnDestroy(){
			if(m_expandButton != null){
				GameObject.Destroy(m_expandButton.gameObject);
			}
			foreach(UIScrollbar sb in m_verticalScrollbars.Values){
				GameObject.Destroy(sb.gameObject);
			}
		}

		private static UIScrollbar CreateVerticalScrollbar(UIPanel panel, UIScrollablePanel scrollablePanel){
			UIScrollbar verticalScrollbar = panel.AddUIComponent<UIScrollbar>();
			verticalScrollbar.name = "VerticalScrollbar";
			verticalScrollbar.width = 20f;
			verticalScrollbar.height = panel.height;
			verticalScrollbar.orientation = UIOrientation.Vertical;
			verticalScrollbar.pivot = UIPivotPoint.BottomLeft;
			verticalScrollbar.AlignTo(panel, UIAlignAnchor.TopRight);
			verticalScrollbar.minValue = 0;
			verticalScrollbar.value = 0;
			verticalScrollbar.incrementAmount = 50;
			verticalScrollbar.autoHide = true;

			UISlicedSprite trackSprite = verticalScrollbar.AddUIComponent<UISlicedSprite>();
			trackSprite.relativePosition = Vector2.zero;
			trackSprite.autoSize = true;
			trackSprite.size = trackSprite.parent.size;
			trackSprite.fillDirection = UIFillDirection.Vertical;
			trackSprite.spriteName = "ScrollbarTrack";

			verticalScrollbar.trackObject = trackSprite;

			UISlicedSprite thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
			thumbSprite.relativePosition = Vector2.zero;
			thumbSprite.fillDirection = UIFillDirection.Vertical;
			thumbSprite.autoSize = true;
			thumbSprite.width = thumbSprite.parent.width - 8;
			thumbSprite.spriteName = "ScrollbarThumb";

			verticalScrollbar.thumbObject = thumbSprite;

			verticalScrollbar.eventValueChanged += (component, value) => {
				scrollablePanel.scrollPosition = new Vector2(0,value);
			};

			panel.eventMouseWheel += (component, eventParam) => {
				verticalScrollbar.value -= (int)eventParam.wheelDelta * verticalScrollbar.incrementAmount;
			};

			scrollablePanel.eventMouseWheel += (component, eventParam) =>{
				verticalScrollbar.value -= (int)eventParam.wheelDelta * verticalScrollbar.incrementAmount;
			};

			scrollablePanel.verticalScrollbar = verticalScrollbar;

			return verticalScrollbar;
		}
	}
}

