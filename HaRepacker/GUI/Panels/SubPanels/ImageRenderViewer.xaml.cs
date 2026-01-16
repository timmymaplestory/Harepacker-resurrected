using HaRepacker.GUI.Controls;
using HaRepacker.GUI.Input;
using HaSharedLibrary.Util;
using MapleLib.Converters;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static MapleLib.Configuration.UserSettings;

namespace HaRepacker.GUI.Panels.SubPanels
{
    public partial class ImageRenderViewer : UserControl
    {
        private bool isLoading = false;
        private MainPanel mainPanel;
        private ImageRenderViewerItem _bindingPropertyItem = new ImageRenderViewerItem();

        public ImageRenderViewerItem BindingPropertyItem { get { return _bindingPropertyItem; } }

        public ImageRenderViewer()
        {
            isLoading = true;
            InitializeComponent();
            if (Program.ConfigurationManager.UserSettings.ThemeColor == (int)UserSettingsThemeColor.Dark)
                VisualStateManager.GoToState(this, "BlackTheme", false);

            this.DataContext = _bindingPropertyItem;
            _bindingPropertyItem.PropertyChanged += (s, e) => {
                if (isLoading) return;
                if (e.PropertyName == "CanvasVectorLt") 
                    UpdateVectorProperty(WzCanvasProperty.LtPropertyName, _bindingPropertyItem.CanvasVectorLt);
            };
            Loaded += (s, e) => {
                _bindingPropertyItem.ShowCrosshair = Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation;
                _bindingPropertyItem.ShowImageBorder = Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation;
                ZoomSlider.Value = Program.ConfigurationManager.UserSettings.ImageZoomLevel;
                isLoading = false;
            };
        }

        public void SetIsLoading(bool b) { this.isLoading = b; }
        public void SetParentMainPanel(MainPanel p) { this.mainPanel = p; }

        private void UpdateVectorProperty(string name, NotifyPointF p) {
            var prop = _bindingPropertyItem.ParentWzCanvasProperty[name] as WzVectorProperty;
            if (prop == null) {
                prop = new WzVectorProperty(name, 0, 0);
                _bindingPropertyItem.ParentWzCanvasProperty.AddProperty(prop);
            }
            prop.X.Value = (int)p.X;
            prop.Y.Value = (int)p.Y;
            _bindingPropertyItem.ParentWzCanvasProperty.ParentImage.Changed = true;
        }

        private void ZoomSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e) {
            if (!isLoading) Program.ConfigurationManager.UserSettings.ImageZoomLevel = ((Slider)s).Value;
        }
    }
}
