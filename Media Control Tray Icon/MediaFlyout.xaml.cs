using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Media_Control_Tray_Icon
{
    /// <summary>
    /// Interaction logic for MediaFlyout.xaml
    /// </summary>
    public partial class MediaFlyout : FluentWindow
    {
        public MediaFlyout()
        {
            ApplicationThemeManager.ApplySystemTheme();
            ApplicationAccentColorManager.ApplySystemAccent();
            InitializeComponent();

        }
    }
}
