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
using System.Windows.Input; // 補上這個引用
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

        // ==========================================
        // 補上這三個滑鼠事件，騙過編譯器 (Start)
        // ==========================================
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            // 這裡留空，暫時不需要拖曳邊框功能，只要能編譯就好
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e) {
            // 這裡留空
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            // 這裡留空
        }
        // ==========================================
        // 補上這三個滑鼠事件，騙過編譯器 (End)
        // ==========================================

        // 這些是介面需要的過濾器按鈕功能，補上以防萬一
        private void MyColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e) { }
        private void button_filter_apply_Click(object sender, RoutedEventArgs e) { }
        private void button_filter_reset_Click(object sender, RoutedEventArgs e) { }
    }
}
