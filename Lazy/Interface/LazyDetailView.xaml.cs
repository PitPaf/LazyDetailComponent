using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace pza.Interface
{
    public partial class LazyDetailView : Window
    {
        //private List<string> ExDetailNames;
        private static LazyDetailViewModel viewModel;

        public LazyDetailView(ExternalCommandData commandData)
        {
            viewModel = new LazyDetailViewModel(commandData);
            viewModel.LazyDetailModel.PropertyChanged += InterfaceVisibilityChange;
            InitializeComponent();
            this.DataContext = viewModel;
            //ExDetailNames = viewModel.LazyDetailModel.ExistingDetailNames;
        }

        private void InterfaceVisibilityChange(object sender, PropertyChangedEventArgs e)
        {
            var model = (LazyDetailModel)sender;
            if (e.PropertyName == nameof(model.InterfaceVisible))
            {
                if (model.InterfaceVisible) this.ShowDialog();
                else this.Hide();
            }
            if (e.PropertyName == nameof(model.InterfaceExists))
            {
                if (!model.InterfaceVisible) this.Close();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            viewModel.LazyDetailModel.CancelDetail();
        }
    }
}
