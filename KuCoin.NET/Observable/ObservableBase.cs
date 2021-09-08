
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Observable
{

    /// <summary>
    /// Provide data for the <see cref="ObservableBase.PropertyChanging"/> event.
    /// </summary>
    public class ObservablePropertyChangingEventArgs : PropertyChangingEventArgs
    {
        /// <summary>
        /// Gets or sets a value that indicates that the property change should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the event has been handled, and the remaining handlers should not be invoked.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservablePropertyChangingEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public ObservablePropertyChangingEventArgs(string propertyName) : base(propertyName)
        {
        }
    }

    /// <summary>
    /// Base class for observable classes.
    /// </summary>
    public abstract class ObservableBase : INotifyPropertyChanged, INotifyPropertyChanging
    { 
        public event PropertyChangedEventHandler PropertyChanged;

        private List<EventHandler<ObservablePropertyChangingEventArgs>> invokerList1 = new List<EventHandler<ObservablePropertyChangingEventArgs>>();
        private List<PropertyChangingEventHandler> invokerList2 = new List<PropertyChangingEventHandler>();

        /// <summary>
        /// Occurs when a property value is changing.
        /// </summary>
        public event EventHandler<ObservablePropertyChangingEventArgs> PropertyChanging
        {
            add
            {
                invokerList1.Add(value);
            }
            remove
            {
                invokerList1.Remove(value);
            }
        }

        event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging
        {
            add
            {
                invokerList2.Add(value);
            }
            remove
            {
                invokerList2.Remove(value);
            }
        }

        /// <summary>
        /// Set a property value if the current value is not equal to the new value and raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="backingStore">The value to compare and set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            bool pass;
            if (typeof(T).IsValueType)
            {
                pass = !backingStore.Equals(value);
            }
            else
            {
                if (!(backingStore is object) && !(value is object))
                {
                    pass = false;
                }
                else if (backingStore is object && !(value is object))
                {
                    pass = true;
                }
                else if (!(backingStore is object) && value is object)
                {
                    pass = true;
                }
                else
                {
                    pass = !backingStore.Equals(value);
                }
            }

            if (pass)
            {
                if (OnPropertyChanging(propertyName)) return false;

                backingStore = value;

                OnPropertyChanged(propertyName);
            }

            return pass;
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>True if the event was cancelled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            if (invokerList1.Count == 0 && invokerList2.Count == 0) return false;

            var e = new ObservablePropertyChangingEventArgs(propertyName);

            foreach (var invoker1 in invokerList1)
            {
                invoker1(this, e);
                if (e.Handled) return e.Cancel;
            }

            foreach (var invoker2 in invokerList2)
            {
                invoker2(this, e);
                if (e.Handled) break;
            }

            return e.Cancel;
        }

    }

}
