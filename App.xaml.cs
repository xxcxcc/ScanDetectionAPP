using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy; 
using System.Windows;

namespace ScanDetectionAPP
{
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
            // 1. 初始化log4net（加载配置文件，确保log4net.config已设为“始终复制”）
            var logConfigFile = new System.IO.FileInfo( "log4net.config" );
            if ( logConfigFile.Exists )
            {
                XmlConfigurator.Configure( logConfigFile );
            }
            else
            {
                // 可选：配置文件不存在时，弹窗提示（避免静默失败）
                MessageBox.Show( "log4net配置文件不存在，日志功能无法使用！" , "配置错误" );
                return;
            }

            // 2. 动态修改日志级别：调试模式用DEBUG，发布模式用INFO
            try
            {
                // 1：获取log4net的日志仓库，并转换为Hierarchy（实际实现类）
                var repository = LogManager.GetRepository() as Hierarchy;
                if ( repository == null )
                {
                    MessageBox.Show( "无法获取log4net日志仓库，日志级别设置失败！" , "日志错误" );
                    return;
                }

                // 2：获取“根日志器”（所有日志器的父级，设置它的级别即可全局生效）
                var rootLogger = repository.Root; // 正确：Hierarchy的Root属性是根日志器（Logger类型）

                // 3：根据模式设置根日志器的级别
#if DEBUG
                // 调试模式：记录DEBUG及以上级别（DEBUG/INFO/WARN/ERROR/FATAL）
                rootLogger.Level = Level.Debug;
#else
                // 发布模式：记录INFO及以上级别（INFO/WARN/ERROR/FATAL）
                rootLogger.Level = Level.Info;
#endif

                // 步骤4：强制刷新日志器配置（确保级别立即生效）
                repository.Configured = true;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( $"设置日志级别失败：{ex.Message}" , "日志错误" );
            }

            // 原有启动代码
            base.OnStartup( e );
        }
    }
}