using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using UI = Autodesk.Revit.UI;
using System.Windows.Controls.Primitives;
using Test1203.ViewModel;

namespace Test1203
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Document doc, UI.ExternalEvent exEvent, UpdateHandler handler)
        {
            InitializeComponent();
            Doc = doc;
            ExEvent = exEvent;
            Handler = handler;
        }

        Document Doc;
        internal UI.ExternalEvent ExEvent;
        internal UpdateHandler Handler;
        public ObservableCollection<ParameterData> TableData { get; set; }
        List<int> checkedIndices { get; set; } = new List<int>();

        private void El1_Click(object sender, RoutedEventArgs e)
        {
           ViewModelHandling.Element1 = ViewModelHandling.SelectElement();
            El1Category.Text = ViewModelHandling.Element1.Category.Name;
            El1Type.Text = ViewModelHandling.Element1.Name;
            El1Id.Text = ViewModelHandling.Element1.Id.IntegerValue.ToString();
            this.Activate();
            if (ViewModelHandling.Element2 != null)
            {
                ViewModelHandling.GetCommonParameters();
                UpdateGrid();
                CoverRectangle.Visibility = System.Windows.Visibility.Hidden;
            }

            TextImage.CreateRevitGraphics(0, ViewModelHandling.Element1, Doc.ActiveView.Id);
        }
        private void El2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelHandling.Element2 = ViewModelHandling.SelectElement();
            El2Category.Text = ViewModelHandling.Element2.Category.Name;
            El2Type.Text = ViewModelHandling.Element2.Name;
            El2Id.Text = ViewModelHandling.Element2.Id.IntegerValue.ToString();
            this.Activate();
            if (ViewModelHandling.Element1 != null)
            {
                ViewModelHandling.GetCommonParameters();
                UpdateGrid();
                CoverRectangle.Visibility = System.Windows.Visibility.Hidden;
            }

            TextImage.CreateRevitGraphics(1, ViewModelHandling.Element2, Doc.ActiveView.Id);
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            if (cell != null)
            {
                DataGridRow row = DataGridRow.GetRowContainingElement(cell);
                row.Background = new SolidColorBrush(Colors.Wheat);
                int index = row.GetIndex();
                checkedIndices.Add(index);
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            if (cell != null)
            {
                DataGridRow row = DataGridRow.GetRowContainingElement(cell);
                row.Background = new SolidColorBrush(Colors.White);
                int index = row.GetIndex();
                checkedIndices.Remove(index);
            }
        }
        private void Transfer_1to2_Click(object sender, RoutedEventArgs e)
        {
            if (checkedIndices.Count > 0)
            {
                Handler.ParameterIndices = checkedIndices;
                ExEvent.Raise();
                UpdateGrid();
            }
        }
        private void Transfer_2to1_Click(object sender, RoutedEventArgs e)
        {
            if (checkedIndices.Count > 0)
            {
                Handler.ReverseElements = true;
                Handler.ParameterIndices = checkedIndices;
                ExEvent.Raise();
                UpdateGrid();
            }
        }
        private void DataGridCell_Clicked(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null)
            {
                DataGridRow Row = DataGridRow.GetRowContainingElement(cell);
                object CellContent = CheckBoxColumn.GetCellContent(Row);


                if (CellContent is CheckBox checkBox)
                {
                    bool checkBoxStatus = (bool)checkBox.IsChecked;
                    checkBox.IsChecked = checkBoxStatus ? false : true;
                }
            }
        }
        private void DataGridCell_Selected(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null)
            {
                cell.IsSelected = false;
                cell.BorderThickness = new Thickness(0);
            }
        }
        private void Highlight1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelHandling.HighlightInModel(0);
        }
        private void Highlight2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelHandling.HighlightInModel(1);
        }
        private void ChangeTableFieldSize()
        {
            double third = this.OneThirdColumn.ActualWidth;

            NameColumn.Width = third - 30;
            El1Column.Width = third + 20;
            El2Column.Width = third;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeTableFieldSize();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeTableFieldSize();
            CoverRectangle.Visibility = System.Windows.Visibility.Visible;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                TextImage.RemoveAllGraphicElements();
            }
            catch { }
        }
        private void UpdateGrid()
        {
            //// Update grid values, optionally remove updated (i.e. equal) parameters
            if (checkedIndices.Count != 0)
            {
                checkedIndices.Sort((a, b) => b.CompareTo(a));
                foreach (var index in checkedIndices)
                {
                    ViewModelHandling.Parameter_Data.RemoveAt(index);
                }
                checkedIndices.Clear();
            }
            var TableData = new ObservableCollection<ParameterData>(ViewModelHandling.Parameter_Data);
            ParameterTable.ItemsSource = null;
            ParameterTable.ItemsSource = TableData;
        }
    }
}
