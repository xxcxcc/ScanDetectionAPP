using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ScanDetectionAPP.Models;
using ScanDetectionAPP.Views;

namespace ScanDetectionAPP.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set => SetProperty( ref _userName , value ); // 复用基类的SetProperty
        }

        private string _userPassword = string.Empty;
        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty( ref _userPassword , value ); // 自动触发通知
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(UserPassword))
            {
                MessageBox.Show("请输入用户名和密码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("无用户数据，请联系管理员！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string jsonContent = File.ReadAllText(filePath);
                List<User>? users = string.IsNullOrEmpty(jsonContent) ? null : JsonSerializer.Deserialize<List<User>>(jsonContent);

                bool isLoginSuccess = users?.Any(u => 
                    u.Username.Equals(UserName, StringComparison.OrdinalIgnoreCase) && 
                    u.Password == UserPassword) ?? false;

                if (isLoginSuccess)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Application.Current.Windows.OfType<LoginView>().FirstOrDefault()?.Close();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}