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
    /// <summary>
    /// 登录视图模型类，用于处理用户登录逻辑。
    /// 继承自 ViewModelBase，提供数据绑定支持。
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private string _userName = string.Empty;
        /// 获取或设置用户名。
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value); // 复用基类的SetProperty
        }

        private string _userPassword = string.Empty;
        /// 获取或设置用户密码。
        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty(ref _userPassword, value); // 自动触发通知
        }

        /// 登录命令，绑定到登录按钮的点击事件。
        public ICommand LoginCommand { get; }
        /// 初始化 LoginViewModel 实例，并创建登录命令。
        public LoginViewModel()
        {
            LoginCommand = CreateCommand(ExecuteLogin);
            LogDebug("LoginViewModel 初始化完成");//记录调试信息,发布后不记录
        }

        /// 执行登录操作的方法。  
        /// 验证用户名和密码是否为空，读取用户数据文件进行匹配验证，
        /// 若验证成功则打开主窗口并关闭登录窗口，否则提示错误信息。
        private void ExecuteLogin()
        {
            LogInfo($"用户 {UserName} 正在尝试登录"); // 普通信息，写入日志文件

            // 检查用户名和密码是否为空
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(UserPassword))
            {
                MessageBox.Show("请输入用户名和密码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // 构造用户数据文件路径
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");

                // 检查用户数据文件是否存在
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("无用户数据，请联系管理员！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 读取并解析用户数据文件
                string jsonContent = File.ReadAllText(filePath);
                //反序列化用户列表
                List<User>? users = string.IsNullOrEmpty(jsonContent) ? null : JsonSerializer.Deserialize<List<User>>(jsonContent);

                // 验证用户名和密码是否匹配
                bool isLoginSuccess = users?.Any(u =>
                    u.Username.Equals(UserName, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == UserPassword) ?? false;

                if (isLoginSuccess)
                {
                    // 登录成功，打开主窗口并关闭登录窗口
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Application.Current.Windows.OfType<LoginView>().FirstOrDefault()?.Close();
                    LogInfo($"用户 {UserName} 登录成功"); // 记录登录成功信息
                }
                else
                {
                    // 登录失败，显示错误提示
                    MessageBox.Show("用户名或密码错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogError($"用户 {UserName} 登录失败，用户名或密码错误"); // 记录登录失败信息
                }
            }
            catch (Exception ex)
            {
                // 异常处理，显示错误信息
                MessageBox.Show($"登录失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError($"用户{UserName}登录失败",ex);
            }
        }
    }
}