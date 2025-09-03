using System;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace ScanDetectionAPP.ViewModels
{
    /// <summary>
    /// 基础命令类（非泛型），实现 ICommand 接口
    /// 支持两种场景：1. 无参数命令 2. 以 object? 为参数的命令（兼容空值）
    /// </summary>
    public class RelayCommand : ICommand
    {
        // 无参数执行逻辑
        private readonly Action? _executeNoParam;
        // 带 object? 参数的执行逻辑（关键修改：object→object?，支持空值）
        private readonly Action<object?>? _executeWithParam;
        // 无参数可用性判断逻辑
        private readonly Func<bool>? _canExecuteNoParam;
        // 带 object? 参数的可用性判断逻辑（关键修改：object→object?，支持空值）
        private readonly Func<object? , bool>? _canExecuteWithParam;

        /// <summary>
        /// 命令可用性变更事件（自动关联 CommandManager，实现 UI 状态自动更新）
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #region 构造函数（覆盖无参数/带参数场景，同步修改委托参数类型）
        /// <summary>
        /// 无参数命令构造函数（无可用性判断）
        /// </summary>
        /// <param name="execute">无参数的执行逻辑（不可为 null）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action execute )
        {
            _executeNoParam = execute ?? throw new ArgumentNullException(
                nameof( execute ) , "命令执行逻辑（execute）不能为 null" );
        }

        /// <summary>
        /// 无参数命令构造函数（带可用性判断）
        /// </summary>
        /// <param name="execute">无参数的执行逻辑（不可为 null）</param>
        /// <param name="canExecute">无参数的可用性判断逻辑（可为 null，默认返回 true）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action execute , Func<bool> canExecute )
            : this( execute )
        {
            _canExecuteNoParam = canExecute;
        }

        /// <summary>
        /// 带 object? 参数命令构造函数（无可用性判断）
        /// </summary>
        /// <param name="execute">带 object? 参数的执行逻辑（不可为 null）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action<object?> execute )
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(
                nameof( execute ) , "命令执行逻辑（execute）不能为 null" );
        }

        /// <summary>
        /// 带 object? 参数命令构造函数（带可用性判断）
        /// </summary>
        /// <param name="execute">带 object? 参数的执行逻辑（不可为 null）</param>
        /// <param name="canExecute">带 object? 参数的可用性判断逻辑（可为 null，默认返回 true）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action<object?> execute , Func<object? , bool> canExecute )
            : this( execute )
        {
            _canExecuteWithParam = canExecute;
        }
        #endregion

        /// <summary>
        /// 判断命令是否可执行
        /// </summary>
        /// <param name="parameter">命令参数（若为无参数命令，此参数会被忽略）</param>
        /// <returns>true：命令可执行；false：命令不可执行</returns>
        public bool CanExecute( object? parameter )
        {
            // 优先处理无参数场景
            if ( _executeNoParam != null )
            {
                return _canExecuteNoParam?.Invoke() ?? true;
            }

            // 处理带参数场景（参数为 null 时，传递默认值，无空值警告）
            if ( _executeWithParam != null )
            {
                var safeParam = parameter ?? default;
                return _canExecuteWithParam?.Invoke( safeParam ) ?? true; // 已无 CS8604 警告
            }

            // 理论上不会触发（构造函数已保证 execute 非 null）
            return false;
        }

        /// <summary>
        /// 执行命令逻辑
        /// </summary>
        /// <param name="parameter">命令参数（若为无参数命令，此参数会被忽略）</param>
        /// <exception cref="InvalidOperationException">当命令未绑定执行逻辑时抛出</exception>
        public void Execute( object? parameter )
        {
            // 执行无参数逻辑
            if ( _executeNoParam != null )
            {
                _executeNoParam.Invoke();
                return;
            }

            // 执行带参数逻辑（参数为 null 时，传递默认值，无空值警告）
            if ( _executeWithParam != null )
            {
                var safeParam = parameter ?? default;
                _executeWithParam.Invoke( safeParam ); // 已无 CS8604 警告
                return;
            }

            // 修正笔误：删除重复的"绑定"
            throw new InvalidOperationException( "命令未绑定有效的执行逻辑" );
        }

        /// <summary>
        /// 手动触发 CanExecuteChanged 事件（强制 UI 更新命令状态）
        /// 场景：当可用性判断依赖的属性变更时（如 ViewModel 中的字段更新），需手动调用
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 泛型命令类（支持特定类型参数）
    /// 解决非泛型版本中参数需强制转换的问题，提升类型安全性
    /// </summary>
    /// <typeparam name="T">命令参数的具体类型（支持值类型/引用类型）</typeparam>
    public class RelayCommand<T> : ICommand
    {
        // 带泛型参数的执行逻辑
        private readonly Action<T> _execute;
        // 带泛型参数的可用性判断逻辑
        private readonly Func<T , bool>? _canExecute;

        /// <summary>
        /// 命令可用性变更事件（自动关联 CommandManager）
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #region 构造函数
        /// <summary>
        /// 泛型命令构造函数（无可用性判断）
        /// </summary>
        /// <param name="execute">带 T 类型参数的执行逻辑（不可为 null）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action<T> execute )
        {
            _execute = execute ?? throw new ArgumentNullException(
                nameof( execute ) , "命令执行逻辑（execute）不能为 null" );
        }

        /// <summary>
        /// 泛型命令构造函数（带可用性判断）
        /// </summary>
        /// <param name="execute">带 T 类型参数的执行逻辑（不可为 null）</param>
        /// <param name="canExecute">带 T 类型参数的可用性判断逻辑（可为 null，默认返回 true）</param>
        /// <exception cref="ArgumentNullException">当 execute 为 null 时抛出</exception>
        public RelayCommand( Action<T> execute , Func<T , bool> canExecute )
            : this( execute )
        {
            _canExecute = canExecute;
        }
        #endregion

        /// <summary>
        /// 判断命令是否可执行（泛型参数安全处理）
        /// </summary>
        /// <param name="parameter">命令参数（会自动转换为 T 类型，转换失败时返回 false）</param>
        /// <returns>true：命令可执行；false：命令不可执行或参数类型不匹配</returns>
        public bool CanExecute( object? parameter )
        {
            // 处理参数类型不匹配的情况（如 UI 传递的参数无法转换为 T）
            if ( !TryConvertParameter( parameter , out var typedParam ) )
            {
                return false;
            }

            // 调用可用性判断逻辑（无逻辑时默认可执行）
            return _canExecute?.Invoke( typedParam! ) ?? true;
        }

        /// <summary>
        /// 执行命令逻辑（泛型参数安全处理）
        /// </summary>
        /// <param name="parameter">命令参数（会自动转换为 T 类型，转换失败时抛出异常）</param>
        /// <exception cref="ArgumentException">当参数无法转换为 T 类型时抛出</exception>
        public void Execute( object? parameter )
        {
            // 转换参数并处理转换失败的情况
            if ( !TryConvertParameter( parameter , out var typedParam ) )
            {
                throw new ArgumentException(
                    $"命令参数类型不匹配，需传递 {typeof( T ).Name} 类型的参数" ,
                    nameof( parameter ) );
            }

            // 执行命令逻辑
            _execute.Invoke( typedParam! );
        }

        /// <summary>
        /// 手动触发 CanExecuteChanged 事件（强制 UI 更新命令状态）
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #region 私有辅助方法（参数转换，已解决 CS1061 错误）
        /// <summary>
        /// 安全转换参数为 T 类型
        /// </summary>
        /// <param name="parameter">原始参数</param>
        /// <param name="typedParam">转换后的 T 类型参数</param>
        /// <returns>true：转换成功；false：转换失败</returns>
        private bool TryConvertParameter( object? parameter , out T? typedParam )
        {
            // 情况 1：参数为 null（处理 T 为引用类型或可空值类型的场景）
            if ( parameter == null )
            {
                // 判断是否为可空值类型
                bool isNullableValueType = typeof( T ).IsValueType
                    && typeof( T ).IsGenericType
                    && typeof( T ).GetGenericTypeDefinition() == typeof( Nullable<> );

                // 若 T 是"非可空值类型"（如 int），null 无法转换，返回 false
                if ( typeof( T ).IsValueType && !isNullableValueType )
                {
                    typedParam = default;
                    return false;
                }

                typedParam = default;
                return true;
            }

            // 情况 2：参数类型已匹配 T（直接强转）
            if ( parameter is T )
            {
                typedParam = ( T )parameter;
                return true;
            }

            // 情况 3：参数类型不匹配，尝试类型转换（如 string → int）
            try
            {
                typedParam = ( T )Convert.ChangeType( parameter , typeof( T ) );
                return true;
            }
            catch ( Exception )
            {
                typedParam = default;
                return false;
            }
        }
        #endregion
    }
}