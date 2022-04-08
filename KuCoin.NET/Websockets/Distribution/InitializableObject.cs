using KuCoin.NET.Data;
using KuCoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    
    /// <summary>
    /// Base class for all <see cref="DistributableObject{TKey, TValue}"/> types that perform initialization work.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TInternal">The internal maintained state object.</typeparam>
    /// <remarks>
    /// <see cref="InitializableObject{TKey, TValue, TInternal}"/> is a type of <see cref="DistributableObject{TKey, TValue}"/>.
    /// </remarks>
    public abstract class InitializableObject<TKey, TValue, TInternal> : DistributableObject<TKey, TValue>, IInitializable<TKey, TInternal>
        where TValue: IStreamableObject
        
    {
        protected IInitialDataProvider<TKey, TInternal> initProvider;

        protected TInternal internalData;

        protected bool initialized;
        protected bool initializing;

        protected bool? isProviderAvailable;

        protected bool failure;

        protected int maxResets = 3;
        protected int resets = 0;
        protected int resetTimeout = 30000;
        protected DateTime? lastFailureTime;

        public event EventHandler Initialized;

        public InitializableObject(IDistributor parent, TKey key, bool direct) : base(parent, key, direct)
        {
            if (parent is IInitialDataProvider<TKey, TInternal> provParent)
            {
                SetInitialDataProvider(provParent);
            }
        }
        public virtual TInternal InternalData
        {
            get => internalData;
            protected set
            {
                SetProperty(ref internalData, value);
            }
        }

        public virtual IInitialDataProvider<TKey, TInternal> DataProvider
        {
            get => initProvider;
            protected set
            {
                SetProperty(ref initProvider, value);
            }
        }

        public virtual bool IsDataProviderAvailable
        {
            get => isProviderAvailable ?? (initProvider != null);
            protected set
            {
                SetProperty(ref isProviderAvailable, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public virtual bool IsInitialized
        {
            get => !disposedValue ? initialized : throw new ObjectDisposedException(GetType().FullName);
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                SetProperty(ref initialized, value);
            }
        }

        public virtual int ResetCount
        {
            get => resets;
            protected set
            {
                SetProperty(ref resets, value);
            }
        }

        public virtual int ResetTimeout
        {
            get => resetTimeout;
            set
            {
                SetProperty(ref resetTimeout, value);
            }
        }
        public virtual int MaxTimeoutRetries
        {
            get => maxResets;
            set
            {
                SetProperty(ref maxResets, value);
            }
        }

        public virtual bool Failure
        {
            get => failure;
            protected set
            {
                if (SetProperty(ref failure, value))
                {
                    if (value)
                    {
                        state = FeedState.Failed;
                        lastFailureTime = DateTime.UtcNow;
                    }

                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(TimeUntilNextRetry));
                    OnPropertyChanged(nameof(LastFailureTime));
                }
            }
        }

        /// <summary>
        /// The time of the last failure, or null if the feed is running normally.
        /// </summary>
        public virtual DateTime? LastFailureTime
        {
            get => lastFailureTime;
            protected set
            {
                if (SetProperty(ref lastFailureTime, value))
                {
                    OnPropertyChanged(nameof(TimeUntilNextRetry));
                }
            }
        }

        /// <summary>
        /// The number of milliseconds remaining until the next retry, or null if the feed is running normally.
        /// </summary>
        public virtual double? TimeUntilNextRetry
        {
            get
            {
                if (lastFailureTime is DateTime t)
                {
                    return (resetTimeout - (DateTime.UtcNow - t).TotalMilliseconds);
                }
                else
                {
                    return null;
                }
            }
        }


        public virtual void SetInitialDataProvider(IInitialDataProvider<TKey, TInternal> dataProvider)
        {
            DataProvider = dataProvider;
            if (dataProvider != null)
            {
                IsDataProviderAvailable = true;
            }
            else
            {
                IsDataProviderAvailable = false;
            }
        }

        /// <summary>
        /// Triggered when data has been initialized.
        /// </summary>
        /// <remarks>
        /// The default behavior of this method is to raise the <see cref="Initialized"/> event.<br />
        /// If you override the default method, be sure to call the base method, unless this behavior is not desired.<br /><br />
        /// This method is executed after <see cref="OnInitialDataProvided(TInternal)"/> on a new thread from <see cref="Initialize"/>, and only if initialization succeeds.
        /// </remarks>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, new EventArgs());
        }

        private void InitialDataCallback(TInternal initData)
        {
            lock (lockObj)
            {
                if (!(initData is object))
                {
                    State = FeedState.Failed;
                    LastFailureTime = DateTime.UtcNow;
                    Failure = true;
                    FailReason = FailReason.OrderBookTimeout;
                    OnPropertyChanged(nameof(TimeUntilNextRetry));

                    initializing = false;
                    IsInitialized = false;
                }
                else
                {
                    initializing = false;
                    IsInitialized = true;
                }

                OnInitialDataProvided(initData);
                _ = Task.Run(OnInitialized);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> Initialize()
        {
            lock (lockObj)
            {
                IsInitialized = false;
                initializing = true;

                State = FeedState.Initializing;
            }

            TInternal initData = default;

            try
            {
                await Task.Delay(100);

                if (DataProvider != null)
                {

                    if (DataProvider is IInitialDataProviderCallback<TKey, TInternal> adp)
                    {
                        adp.BeginProvideInitialData(key, InitialDataCallback);
                        return true;
                    }

                    initData = await DataProvider?.ProvideInitialData(key);
                }
            }
            catch (Exception ex)
            {
                KuCoinSystem.Logger.Log(ex.Message);
                initData = default;
            }

            InitialDataCallback(initData);
            return initData is object;
        }

        /// <summary>
        /// Occurs when initial data is provided by the provider.
        /// </summary>
        /// <param name="data">The data that was provided.</param>
        /// <remarks>
        /// The default behavior of this method is to set <see cref="InternalData"/> to the value of <paramref name="data"/>.<br />
        /// If you override the default method, be sure to call the base method, unless this behavior is not desired.<br /><br />
        /// This method is called before <see cref="OnInitialized"/> from <see cref="Initialize"/>, and only if initialization succeeds.
        /// </remarks>
        protected virtual void OnInitialDataProvided(TInternal data)
        {
            InternalData = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task Reset()
        {
            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    if (initializing) return;

                    initialized = failure = false;
                    LastFailureTime = null;
                    state = FeedState.Connected;

                    _ = Task.Run(() =>
                    {
                        OnPropertyChanged(nameof(IsInitialized));
                        OnPropertyChanged(nameof(Failure));
                        OnPropertyChanged(nameof(TimeUntilNextRetry));
                        OnPropertyChanged(nameof(LastFailureTime));
                    });
                    
                    buffer.Clear();
                    PerformResetTasks();
                }
            });
        }

        /// <summary>
        /// These are tasks that derived class will perform during a reset operation.
        /// </summary>
        /// <remarks>
        /// The default behavior of this method is to set <see cref="InternalData"/> to default/null.<br />
        /// If you override the default method, be sure to call the base method, unless this behavior is not desired.
        /// </remarks>
        protected virtual void PerformResetTasks()
        {
            lock (lockObj)
            {
                InternalData = default;
            }
        }

    }
}
