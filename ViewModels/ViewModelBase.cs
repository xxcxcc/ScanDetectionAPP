using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using log4net;

namespace ScanDetectionAPP.ViewModels
{
    /// <summary>
    /// MVVM基础类，所有ViewModel都应继承此类
    /// 自动实现属性通知、命令创建、线程安全等核心功能
    /// 只需关注业务逻辑，无需关心基础实现
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected readonly ILog _log;

        public ViewModelBase()
        {
            // 3. 初始化日志对象（参数是当前ViewModel的类型，日志中会显示“哪个ViewModel的日志”）
            _log = LogManager.GetLogger( GetType() );
        }

        #region 基础属性通知（核心功能）
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知（自动在UI线程执行，避免跨线程错误）
        /// </summary>
        /// <param name="propertyName">属性名（不用手动传，自动获取调用者名称）</param>
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            // 确保在UI线程更新UI
            Application.Current.Dispatcher.Invoke( () =>
            {
                PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
            } );
        }

        /// <summary>
        /// 安全更新属性值（值不变时不触发通知，提升性能）
        /// 用法：SetProperty(ref _name, value);
        /// </summary>
        /// <returns>是否真的更新了值（方便后续联动逻辑）</returns>
        protected bool SetProperty<T>( ref T storage , T value , [CallerMemberName] string propertyName = "" )
        {
            // 避免值未变化时触发通知（比如连续两次设置相同值）
            if ( EqualityComparer<T>.Default.Equals( storage , value ) )
                return false;

            storage = value;
            OnPropertyChanged( propertyName );
            return true;
        }
        #endregion

        #region 命令创建快捷方法
        /// <summary>
        /// 快速创建无参数命令（不用手动new RelayCommand）
        /// 用法：LoginCommand = CreateCommand(Login);
        /// </summary>
        protected ICommand CreateCommand( Action execute )
        {
            return new RelayCommand( execute );
        }

        /// <summary>
        /// 快速创建带可用性判断的无参数命令
        /// 用法：SaveCommand = CreateCommand(Save, CanSave);
        /// </summary>
        protected ICommand CreateCommand( Action execute , Func<bool> canExecute )
        {
            return new RelayCommand( execute , canExecute );
        }

        /// <summary>
        /// 快速创建带参数命令（自动处理参数类型）
        /// 用法：DeleteCommand = CreateCommand<int>(Delete);
        /// </summary>
        protected ICommand CreateCommand<T>( Action<T> execute )
        {
            return new RelayCommand<T>( execute );
        }

        /// <summary>
        /// 快速创建带参数且有可用性判断的命令
        /// 用法：EditCommand = CreateCommand<Item>(Edit, CanEdit);
        /// </summary>
        protected ICommand CreateCommand<T>( Action<T> execute , Func<T , bool> canExecute )
        {
            return new RelayCommand<T>( execute , canExecute );
        }
        #endregion

        #region 批量通知与辅助功能
        /// <summary>
        /// 批量触发多个属性的通知（比如刷新多个关联属性）
        /// 用法：NotifyProperties(nameof(Name), nameof(Age), nameof(Address));
        /// </summary>
        protected void NotifyProperties( params string[] propertyNames )
        {
            if ( propertyNames == null || propertyNames.Length == 0 )
                return;

            foreach ( var propName in propertyNames.Distinct() ) // 去重，避免重复通知
            {
                OnPropertyChanged( propName );
            }
        }

        /// <summary>
        /// 检查属性名是否有效（防止拼写错误导致通知失效）
        /// 调试时用，发布时自动失效（不影响性能）
        /// </summary>
        [Conditional( "DEBUG" )] // 只在DEBUG模式生效
        protected void ValidatePropertyName( string propertyName )
        {
            var propInfo = GetType().GetProperty( propertyName );
            if ( propInfo == null )
            {
                throw new ArgumentException( $"属性名错误：ViewModel中不存在名为“{propertyName}”的属性（可能是拼写错误）" );
            }
        }
        #endregion

        #region 日志与错误处理
        /// <summary>
        /// 记录信息日志
        /// </summary>
        protected void LogInfo( string message )
        {
            _log.Info(message);
        }

        /// <summary>
        /// 记录错误日志（自动包含当前ViewModel类型）
        /// </summary>
        protected void LogError( string message , Exception? ex = null )
        {
            if (ex == null)
            {
                _log.Error(message );// 无异常时记录错误
            }
            else
            {
                _log.Error(message ,ex);// 有异常时，自动记录异常堆栈信息
            }
        }

        /// <summary>
        /// 记录调试信息（开发时用，发布后不会记录，避免日志冗余）
        /// </summary>
        protected void LogDebug( string message )
        {
            _log.Debug( message ); // 对应log4net的DEBUG级别（配置中设为INFO则不记录）
        }
        #endregion
    }
}
