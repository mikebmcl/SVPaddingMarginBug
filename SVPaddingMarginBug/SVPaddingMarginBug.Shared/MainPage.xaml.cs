using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SVPaddingMarginBug
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();

            NeitherStackPanel.SizeChanged += NeitherStackPanel_SizeChanged;
            NeitherScrollViewer.SizeChanged += NeitherScrollViewer_SizeChanged;
            ContentMarginStackPanel.SizeChanged += ContentMarginStackPanel_SizeChanged;
            ContentMarginScrollViewer.SizeChanged += ContentMarginScrollViewer_SizeChanged;
            SVPaddingStackPanel.SizeChanged += SVPaddingStackPanel_SizeChanged;
            SVPaddingScrollViewer.SizeChanged += SVPaddingScrollViewer_SizeChanged;
            BothStackPanel.SizeChanged += BothStackPanel_SizeChanged;
            BothScrollViewer.SizeChanged += BothScrollViewer_SizeChanged;
            // Note: We need to use LayoutUpdated events because the ViewPort* and Extent* data isn't ready (except in UWP) when SizeChanged fires for the first time and we don't want to have to resize the window just to get the data.
            NeitherScrollViewer.LayoutUpdated += NeitherScrollViewer_LayoutUpdated;
            ContentMarginScrollViewer.LayoutUpdated += ContentMarginScrollViewer_LayoutUpdated;
            SVPaddingScrollViewer.LayoutUpdated += SVPaddingScrollViewer_LayoutUpdated;
            BothScrollViewer.LayoutUpdated += BothScrollViewer_LayoutUpdated;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Thickness m_scrollViewerBorderThickness = new Thickness(1);
        public Thickness ScrollViewerBorderThickness
        {
            get => m_scrollViewerBorderThickness;
            set
            {
                if (m_scrollViewerBorderThickness != value)
                {
                    m_scrollViewerBorderThickness = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Thickness m_contentBorderThickness = new Thickness(2);
        public Thickness ContentBorderThickness
        {
            get => m_contentBorderThickness;
            set
            {
                if (m_contentBorderThickness != value)
                {
                    m_contentBorderThickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ScrollViewerPaddingName => nameof(ScrollViewerPadding);
        private Thickness m_scrollViewerPadding = new Thickness(16);
        public Thickness ScrollViewerPadding
        {
            get => m_scrollViewerPadding;
            set
            {
                if (m_scrollViewerPadding != value)
                {
                    m_scrollViewerPadding = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ContentMarginName => nameof(ContentMargin);
        private Thickness m_contentMargin = new Thickness(10);
        public Thickness ContentMargin
        {
            get => m_contentMargin;
            set
            {
                if (m_contentMargin != value)
                {
                    m_contentMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScrollBarVisibility m_verticalScrollBarVisibility = ScrollBarVisibility.Visible;
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => m_verticalScrollBarVisibility;
            set
            {
                if (m_verticalScrollBarVisibility != value)
                {
                    m_verticalScrollBarVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScrollBarVisibility m_horizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => m_horizontalScrollBarVisibility;
            set
            {
                if (m_horizontalScrollBarVisibility != value)
                {
                    m_horizontalScrollBarVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        string _dblFormat = "F";
        System.Globalization.CultureInfo _cultureInfo = System.Globalization.CultureInfo.CurrentUICulture;
        private ScrollViewerContentExtentData GetContentExtentDataForScrollViewer(ScrollViewer sv) => new ScrollViewerContentExtentData() { ScrollViewerName = sv.Name, ContentActualHeight = $"Content ActualHeight: {((sv.Content as FrameworkElement)?.ActualHeight ?? double.NaN).ToString(_dblFormat, _cultureInfo)}", ContentActualHeightValue = (sv.Content as FrameworkElement)?.ActualHeight ?? double.NaN, ContentActualWidth = $"Content ActualWidth: {((sv.Content as FrameworkElement)?.ActualWidth ?? double.NaN).ToString(_dblFormat, _cultureInfo)}", ContentActualWidthValue = (sv.Content as FrameworkElement)?.ActualWidth ?? double.NaN, SVActualHeight = $"ScrollViewer ActualHeight: {sv.ActualHeight.ToString(_dblFormat, _cultureInfo)}", SVActualHeightValue = sv.ActualHeight, SVActualWidth = $"ScrollViewer ActualWidth: {sv.ActualWidth.ToString(_dblFormat, _cultureInfo)}", SVActualWidthValue = sv.ActualWidth, ExtentHeight = $"ExtentHeight: {sv.ExtentHeight.ToString(_dblFormat, _cultureInfo)}", ExtentHeightValue = sv.ExtentHeight, ExtentWidth = $"ExtentWidth: {sv.ExtentWidth.ToString(_dblFormat, _cultureInfo)}", ExtentWidthValue = sv.ExtentWidth, ViewPortHeight = $"ViewportHeight: {sv.ViewportHeight.ToString(_dblFormat, _cultureInfo)}", ViewPortHeightValue = sv.ViewportHeight, ViewPortWidth = $"ViewportWidth: {sv.ViewportWidth.ToString(_dblFormat, _cultureInfo)}", ViewPortWidthValue = sv.ViewportWidth, ScrollableHeight = $"ScrollableHeight: {sv.ScrollableHeight.ToString(_dblFormat, _cultureInfo)}", ScrollableHeightValue = sv.ScrollableHeight, ScrollableWidth = $"ScrollableWidth: {sv.ScrollableWidth.ToString(_dblFormat, _cultureInfo)}", ScrollableWidthValue = sv.ScrollableWidth };

        private Dictionary<string, (string dataString, ScrollViewerContentExtentData data, Thickness scrollViewerPadding, Thickness contentMargin)> _dataForLogger = new Dictionary<string, (string dataString, ScrollViewerContentExtentData data, Thickness scrollViewerPadding, Thickness contentMargin)>();
        private void AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(string scrollViewerName, (string dataString, ScrollViewerContentExtentData data, Thickness scrollViewerPadding, Thickness contentMargin) dataInfo, bool fromSizeChanged, [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null, [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (string.IsNullOrEmpty(scrollViewerName))
            {
                if (callerMemberName == null)
                {
                    callerMemberName = string.Empty;
                }
                if (callerFilePath == null)
                {
                    callerMemberName = string.Empty;
                }
                //_log.Error($"In call from '{callerMemberName}' in file '' at line number , expected argument {nameof(scrollViewerName)} to have a value but it is {(scrollViewerName == null ? "null" : "empty")}");
                return;
            }
            if (dataInfo.data == null || string.IsNullOrEmpty(dataInfo.dataString))
            {
                if (callerMemberName == null)
                {
                    callerMemberName = string.Empty;
                }
                if (callerFilePath == null)
                {
                    callerMemberName = string.Empty;
                }
                var errorString = $"In call from '{callerMemberName}' in file '' at line number , argument {nameof(dataInfo)} has invalid data.";
                if (dataInfo.data == null)
                {
                    errorString += $" {nameof(dataInfo)}.{nameof(dataInfo.data)} is null.";
                }
                if (string.IsNullOrEmpty(dataInfo.dataString))
                {
                    errorString += $" Expected {nameof(dataInfo)}.{nameof(dataInfo.dataString)} to have a value but it is {(dataInfo.dataString == null ? "null" : "empty")}.";
                }
                //_log.Error(errorString);
                return;
            }
            if (!_dataForLogger.ContainsKey(scrollViewerName) || fromSizeChanged)
            {
                if (_dataForLogger.ContainsKey(scrollViewerName))
                {
                    _dataForLogger[scrollViewerName] = dataInfo;
                }
                else
                {
                    _dataForLogger.Add(scrollViewerName, dataInfo);
                }

                if (scrollViewerName == nameof(NeitherScrollViewer))
                {
                    foreach (var item in _dataForLogger)
                    {
                        var neitherData = _dataForLogger[nameof(NeitherScrollViewer)].data;
                        if (item.Key == nameof(NeitherScrollViewer))
                        {
                            continue;
                        }
                        PopulateExpectedAndActualValuesForScrollViewerContentExtentData(neitherData, item.Value.data, item.Value.scrollViewerPadding, item.Value.contentMargin);
                    }
                }
                else
                {
                    if (_dataForLogger.ContainsKey(nameof(NeitherScrollViewer)))
                    {
                        PopulateExpectedAndActualValuesForScrollViewerContentExtentData(_dataForLogger[nameof(NeitherScrollViewer)].data, dataInfo.data, dataInfo.scrollViewerPadding, dataInfo.contentMargin);
                    }
                }
                if (_dataForLogger.Count == 4)
                {
                    foreach (var item in _dataForLogger)
                    {
                        //_log.Debug(item.Value.dataString);
                    }
                }
            }
        }

        private void PopulateExpectedAndActualValuesForScrollViewerContentExtentData(ScrollViewerContentExtentData neither,
            ScrollViewerContentExtentData dataToPopulate, Thickness scrollViewerPadding, Thickness contentMargin)
        {
            dataToPopulate.SVActualWidthExpectedDifference = 0;
            dataToPopulate.SVActualWidthActualDifference = neither.SVActualWidthValue - dataToPopulate.SVActualWidthValue;

            dataToPopulate.SVActualHeightExpectedDifference = 0;
            dataToPopulate.SVActualHeightActualDifference = neither.SVActualHeightValue - dataToPopulate.SVActualHeightValue;

            dataToPopulate.ViewPortWidthExpectedDifference = scrollViewerPadding.Left + scrollViewerPadding.Right;
            dataToPopulate.ViewPortWidthActualDifference = neither.ViewPortWidthValue - dataToPopulate.ViewPortWidthValue;

            dataToPopulate.ViewPortHeightExpectedDifference = scrollViewerPadding.Top + scrollViewerPadding.Bottom;
            dataToPopulate.ViewPortHeightActualDifference = neither.ViewPortHeightValue - dataToPopulate.ViewPortHeightValue;

            dataToPopulate.ExtentWidthExpectedDifference = -(contentMargin.Left + contentMargin.Right);
            dataToPopulate.ExtentWidthActualDifference = neither.ExtentWidthValue - dataToPopulate.ExtentWidthValue;

            dataToPopulate.ExtentHeightExpectedDifference = -(contentMargin.Top + contentMargin.Bottom);
            dataToPopulate.ExtentHeightActualDifference = neither.ExtentHeightValue - dataToPopulate.ExtentHeightValue;

            dataToPopulate.ContentActualWidthExpectedDifference = 0;
            dataToPopulate.ContentActualWidthActualDifference = neither.ContentActualWidthValue - dataToPopulate.ContentActualWidthValue;

            dataToPopulate.ContentActualHeightExpectedDifference = 0;
            dataToPopulate.ContentActualHeightActualDifference = neither.ContentActualHeightValue - dataToPopulate.ContentActualHeightValue;

            dataToPopulate.ScrollableWidthExpectedDifference = -(contentMargin.Left + contentMargin.Right + scrollViewerPadding.Left + scrollViewerPadding.Right);
            dataToPopulate.ScrollableWidthActualDifference = neither.ScrollableWidthValue - dataToPopulate.ScrollableWidthValue;

            dataToPopulate.ScrollableHeightExpectedDifference = -(contentMargin.Top + contentMargin.Bottom + scrollViewerPadding.Top + scrollViewerPadding.Bottom);
            dataToPopulate.ScrollableHeightActualDifference = neither.ScrollableHeightValue - dataToPopulate.ScrollableHeightValue;
        }

        private void NeitherStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = NeitherScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            NeitherScrollViewerContentExtentData = data;
        }

        private void NeitherScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = NeitherScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            NeitherScrollViewerContentExtentData = data;
        }

        private void ContentMarginStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = ContentMarginScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }

            ContentMarginScrollViewerContentExtentData = data;
        }

        private void ContentMarginScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = ContentMarginScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            ContentMarginScrollViewerContentExtentData = data;
        }

        private void SVPaddingStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = SVPaddingScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            SVPaddingScrollViewerContentExtentData = data;
        }

        private void SVPaddingScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = SVPaddingScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            SVPaddingScrollViewerContentExtentData = data;
        }

        private void BothStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = BothScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            BothScrollViewerContentExtentData = data;
        }

        private void BothScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sv = BothScrollViewer;
            if (sv == null)
            {
                return;
            }
            var data = GetContentExtentDataForScrollViewer(sv);
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), true);
            }
            BothScrollViewerContentExtentData = data;
        }

        private void NeitherScrollViewer_LayoutUpdated(object sender, object e)
        {
            var sv = NeitherScrollViewer;
            if (sv == null)
            {
                return;
            }
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                // Avoid triggering a Windows.UI.Xaml.LayoutCycleException
                sv.LayoutUpdated -= NeitherScrollViewer_LayoutUpdated;
                var data = GetContentExtentDataForScrollViewer(sv);
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), false);
                NeitherScrollViewerContentExtentData = data;
            }
        }

        private void ContentMarginScrollViewer_LayoutUpdated(object sender, object e)
        {
            var sv = ContentMarginScrollViewer;
            if (sv == null)
            {
                return;
            }
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                // Avoid triggering a Windows.UI.Xaml.LayoutCycleException
                sv.LayoutUpdated -= ContentMarginScrollViewer_LayoutUpdated;
                var data = GetContentExtentDataForScrollViewer(sv);
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), false);
                ContentMarginScrollViewerContentExtentData = data;
            }
        }

        private void SVPaddingScrollViewer_LayoutUpdated(object sender, object e)
        {
            var sv = SVPaddingScrollViewer;
            if (sv == null)
            {
                return;
            }
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                // Avoid triggering a Windows.UI.Xaml.LayoutCycleException
                sv.LayoutUpdated -= SVPaddingScrollViewer_LayoutUpdated;
                var data = GetContentExtentDataForScrollViewer(sv);
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), false);
                SVPaddingScrollViewerContentExtentData = data;
            }
        }

        private void BothScrollViewer_LayoutUpdated(object sender, object e)
        {
            var sv = BothScrollViewer;
            if (sv == null)
            {
                return;
            }
            if (sv.ExtentWidth != 0 && sv.ExtentHeight != 0 && sv.ViewportWidth != 0 && sv.ViewportHeight != 0)
            {
                // Avoid triggering a Windows.UI.Xaml.LayoutCycleException
                sv.LayoutUpdated -= BothScrollViewer_LayoutUpdated;
                var data = GetContentExtentDataForScrollViewer(sv);
                var debugData = $"\n{sv.Name}\n{nameof(ScrollViewerPadding)}: {ScrollViewerPadding}\n{nameof(ContentMargin)}: {ContentMargin}\n{nameof(ScrollViewerBorderThickness)}: {ScrollViewerBorderThickness}\n{nameof(ContentBorderThickness)}: {ContentBorderThickness}\n{data.SVActualWidth}\n{data.ViewPortWidth}\n{data.ExtentWidth}\n{data.ContentActualWidth}\n{data.ScrollableWidth}\n{data.SVActualHeight}\n{data.ViewPortHeight}\n{data.ExtentHeight}\n{data.ContentActualHeight}\n{data.ScrollableHeight}\n";
                AddDataForLoggerAndPopulateExpectedAndActualValuesForScrollViewerContentExtentDatas(sv.Name, (debugData, data, sv.Padding, ((sv.Content as StackPanel)?.Margin ?? default)), false);
                BothScrollViewerContentExtentData = data;
            }
        }

        private ScrollViewerContentExtentData _neitherScrollViewerContentExtentData;
        public ScrollViewerContentExtentData NeitherScrollViewerContentExtentData
        {
            get => _neitherScrollViewerContentExtentData;
            set
            {
                if (_neitherScrollViewerContentExtentData != value)
                {
                    _neitherScrollViewerContentExtentData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScrollViewerContentExtentData _contentMarginScrollViewerContentExtentData;
        public ScrollViewerContentExtentData ContentMarginScrollViewerContentExtentData
        {
            get => _contentMarginScrollViewerContentExtentData;
            set
            {
                if (_contentMarginScrollViewerContentExtentData != value)
                {
                    _contentMarginScrollViewerContentExtentData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScrollViewerContentExtentData _svPaddingScrollViewerContentExtentData;
        public ScrollViewerContentExtentData SVPaddingScrollViewerContentExtentData
        {
            get => _svPaddingScrollViewerContentExtentData;
            set
            {
                if (_svPaddingScrollViewerContentExtentData != value)
                {
                    _svPaddingScrollViewerContentExtentData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScrollViewerContentExtentData _bothScrollViewerContentExtentData;
        public ScrollViewerContentExtentData BothScrollViewerContentExtentData
        {
            get => _bothScrollViewerContentExtentData;
            set
            {
                if (_bothScrollViewerContentExtentData != value)
                {
                    _bothScrollViewerContentExtentData = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
