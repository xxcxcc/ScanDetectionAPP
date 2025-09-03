
using ScanDetectionAPP.ViewModels;

namespace ScanDetectionAPP.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
            var loginViewModel = new LoginViewModel();
            this.DataContext = loginViewModel;
        }
    }
}
