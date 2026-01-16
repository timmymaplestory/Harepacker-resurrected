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
        
		
		private bool _isDragging = false;
private Point _lastMousePoint;
		
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
// ==========================================
        // 這是修改後：有實際功能的滑鼠拖曳事件
        // ==========================================
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                _isDragging = true;
                _lastMousePoint = e.GetPosition(this); // 紀錄滑鼠按下的位置
                element.CaptureMouse(); // 鎖定滑鼠，確保拖曳時游標不會跑掉
                e.Handled = true;
            }
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // 1. 計算滑鼠移動的距離
                Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - _lastMousePoint.X;
                double deltaY = currentPoint.Y - _lastMousePoint.Y;

                // 2. 取得目前的縮放比例 (Zoom)，不然放大圖片時拖曳速度會變很奇怪
                double zoom = ZoomSlider.Value;
                if (zoom <= 0) zoom = 1;

                // 3. 更新數據 (這裡將移動量除以縮放倍率，加到 CanvasVectorLt 上)
                if (_bindingPropertyItem != null && _bindingPropertyItem.CanvasVectorLt != null)
                {
                    _bindingPropertyItem.CanvasVectorLt.X += (int)(deltaX / zoom);
                    _bindingPropertyItem.CanvasVectorLt.Y += (int)(deltaY / zoom);
                }

                // 4. 更新最後位置，為下一次計算做準備
                _lastMousePoint = currentPoint;
            }
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                var element = sender as FrameworkElement;
                if (element != null)
                {
                    element.ReleaseMouseCapture(); // 釋放滑鼠
                }
            }
        }
        private void MyColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e) { }
        private void button_filter_apply_Click(object sender, RoutedEventArgs e) { }
        private void button_filter_reset_Click(object sender, RoutedEventArgs e) { }
    }
}
