using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScanDetectionAPP.ViewModels
{
    /// <summary>
    /// 实现INotifyPropertyChanged的基类，供所有绑定模型继承
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名（不填则自动取调用者名称）</param>
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// 安全更新属性值并触发通知（避免不必要的通知）
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="storage">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否更新成功</returns>
        protected bool SetProperty<T>( ref T storage , T value , [CallerMemberName] string propertyName = "" )
        {
            if ( EqualityComparer<T>.Default.Equals( storage , value ) )
                return false;

            storage = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}