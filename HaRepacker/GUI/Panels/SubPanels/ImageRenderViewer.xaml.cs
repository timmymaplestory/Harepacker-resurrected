using HaRepacker.GUI.Controls;
using HaRepacker.GUI.Input;
using HaSharedLibrary.Util;
using MapleLib.Converters;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MapleLib.Configuration.UserSettings;

namespace HaRepacker.GUI.Panels.SubPanels
{
    /// <summary>
    /// Interaction logic for ImageRenderViewer.xaml
    /// 這是負責顯示圖片、處理判定框邏輯的核心檔案
    /// </summary>
    public partial class ImageRenderViewer : UserControl
    {
        private bool isLoading = false;

        private MainPanel mainPanel;

        private ImageRenderViewerItem _bindingPropertyItem = new ImageRenderViewerItem();
        public ImageRenderViewerItem BindingPropertyItem
        {
            get { return _bindingPropertyItem; }
            private set { }
        }

        public ImageRenderViewer()
        {
            isLoading = true; // set isloading 

            InitializeComponent();

            // Set theme color (設定主題顏色)
            if (Program.ConfigurationManager.UserSettings.ThemeColor == (int)UserSettingsThemeColor.Dark)
            {
                VisualStateManager.GoToState(this, "BlackTheme", false);
            }

            this.DataContext = _bindingPropertyItem; // set data binding
            _bindingPropertyItem.PropertyChanged += ImgPropertyItem_PropertyChanged;

            Loaded += ImageRenderViewer_Loaded;
        }

        public void SetIsLoading(bool bIsLoading)
        {
            this.isLoading = bIsLoading;
        }
        public void SetParentMainPanel(MainPanel panel)
        {
            this.mainPanel = panel;
        }


        /// <summary>
        /// When the page loads
        /// </summary>
        private void ImageRenderViewer_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set via app settings
                _bindingPropertyItem.ShowCrosshair = Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation;
                _bindingPropertyItem.ShowImageBorder = Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation;

                ZoomSlider.Value = Program.ConfigurationManager.UserSettings.ImageZoomLevel;
            }
            finally
            {
                isLoading = false;
            }
        }

        #region UI Events

        /// <summary>
        /// Image zoom level on value changed
        /// </summary>
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isLoading)
                return;

            Slider zoomSlider = (Slider)sender;
            Program.ConfigurationManager.UserSettings.ImageZoomLevel = zoomSlider.Value;
        }

        private bool bBorderDragging = false;

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bBorderDragging = true;
            Rectangle_MouseMove(sender, e);
            // System.Diagnostics.Debug.WriteLine("Mouse left button down");
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (bBorderDragging)
            {
                // dragMove logic if needed
                // System.Diagnostics.Debug.WriteLine("Mouse drag move");
            }
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bBorderDragging = false;
            // System.Diagnostics.Debug.WriteLine("Mouse left button up");
        }

        /// <summary>
        /// On propertygrid property changed
        /// 這裡是核心：當介面上的數值改變時，更新 WZ 結構
        /// </summary>
        private void ImgPropertyItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case "ShowCrosshair":
                    {
                        if (_bindingPropertyItem.ShowCrosshair == true)
                            Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation = true;
                        else
                            Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation = false;
                        break;
                    }
                case "ShowImageBorder":
                    {
                        if (_bindingPropertyItem.ShowImageBorder == true)
                            Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation = true;
                        else
                            Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation = false;
                        break;
                    }
                case "Delay":
                    {
                        int newdelay = _bindingPropertyItem.Delay;
                        WzIntProperty intProperty = this._bindingPropertyItem.ParentWzCanvasProperty[WzCanvasProperty.AnimationDelayPropertyName] as WzIntProperty;
                        if (intProperty != null)
                        {
                            intProperty.Value = newdelay;
                        }
                        break;
                    }
                case "CanvasVectorOrigin":
                    {
                        NotifyPointF CanvasVectorOrigin = this._bindingPropertyItem.CanvasVectorOrigin;
                        UpdateVectorProperty(WzCanvasProperty.OriginPropertyName, CanvasVectorOrigin);
                        break;
                    }
                case "CanvasVectorHead":
                    {
                        NotifyPointF vectorHead = this._bindingPropertyItem.CanvasVectorHead;
                        UpdateVectorProperty(WzCanvasProperty.HeadPropertyName, vectorHead);
                        break;
                    }
                // --- 這裡就是 Timmy 視覺化修改的重點 ---
                case "CanvasVectorLt":
                case "CanvasVectorRb": // 同時監聽 Lt 和 Rb
                    {
                        NotifyPointF vectorLt = this._bindingPropertyItem.CanvasVectorLt;
                        NotifyPointF vectorRb = this._bindingPropertyItem.CanvasVectorRb;

                        // 更新 LT 到 WZ 檔案
                        UpdateVectorProperty(WzCanvasProperty.LtPropertyName, vectorLt);
                        // 更新 RB 到 WZ 檔案
                        UpdateVectorProperty(WzCanvasProperty.RbPropertyName, vectorRb);
                        break;
                    }
                    // -------------------------------------
            }
        }

        /// <summary>
        /// 專用的工具函式：更新 WZ 內的向量屬性 (lt, rb, head, origin)
        /// 如果屬性不存在，會自動建立；如果存在，則更新數值。
        /// </summary>
        private void UpdateVectorProperty(string propertyName, NotifyPointF point)
        {
            if (this._bindingPropertyItem.ParentWzCanvasProperty == null) return;

            WzVectorProperty vectorProp = this._bindingPropertyItem.ParentWzCanvasProperty[propertyName] as WzVectorProperty;
            
            if (vectorProp == null)
            {
                // 如果 WZ 裡面原本沒有這個屬性 (例如沒有 lt)，則建立新的
                vectorProp = new WzVectorProperty(propertyName, new WzIntProperty("X", (int)point.X), new WzIntProperty("Y", (int)point.Y));
                
                this._bindingPropertyItem.ParentWzCanvasProperty.AddProperty(vectorProp);
                this._bindingPropertyItem.ParentWzCanvasProperty.ParentImage.Changed = true;
            }
            else
            {
                // 如果已經存在，直接更新數值
                vectorProp.X.Value = (int)point.X;
                vectorProp.Y.Value = (int)point.Y;
            }
        }

        /// <summary>
        /// Color picker -- image ARGB editor
        /// </summary>
        private void MyColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (isLoading)
                return;

            if (e.NewValue.HasValue)
            {
                System.Windows.Media.Color selectedColor = e.NewValue.Value;

                if (selectedColor != null)
                {
                    _bindingPropertyItem.Bitmap = BitmapHelper.ApplyColorFilter(_bindingPropertyItem.BitmapBackup, selectedColor);
                }
            }
        }

        private void button_filter_apply_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading)
                return;

            if (_bindingPropertyItem.Image != null)
            {
                System.Windows.Media.Color? selectedColor = MyColorCanvas.SelectedColor;
                if (selectedColor != null)
                {
                    _bindingPropertyItem.Bitmap = BitmapHelper.ApplyColorFilter(_bindingPropertyItem.BitmapBackup, selectedColor.Value);
                    mainPanel.ChangeCanvasPropBoxImage(_bindingPropertyItem.Bitmap);
                }
            }
        }

        private void button_filter_reset_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading)
                return;

            if (_bindingPropertyItem.Bitmap != null)
            {
                _bindingPropertyItem.Bitmap = _bindingPropertyItem.BitmapBackup;
            }
        }
        #endregion
    }
}
