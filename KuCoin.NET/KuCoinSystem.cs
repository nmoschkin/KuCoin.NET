using KuCoin.NET.Helpers;
using KuCoin.NET.Rest;
using KuCoin.NET.Services;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET
{

    /// <summary>
    /// Global application-domain credentials utilization modes.
    /// </summary>
    public enum UseCredentialsMode
    {
        /// <summary>
        /// Default behavior. 
        /// </summary>
        /// <remarks>
        /// In the absence of an explicitly-passed <see cref="ICredentialsProvider"/>, the caller will
        /// use the first instance of <see cref="ICredentialsProvider"/> in the global cache that satisfies 
        /// the sandbox and futures configuration requirements of the caller.
        /// </remarks>
        Default,

        /// <summary>
        /// Always use credentials from the global cache, even if they do not match the caller's configuration.
        /// </summary>
        /// <remarks>
        /// In the absence of an explicitly-passed <see cref="ICredentialsProvider"/>, the caller will
        /// use the first instance of <see cref="ICredentialsProvider"/> in the global cache that satisfies 
        /// the sandbox and futures requirements of the caller, or else the first available.
        /// </remarks>
        Always,

        /// <summary>
        /// Always use credentials from the global cache, even if they do not match the caller's configuration.
        /// </summary>
        /// <remarks>
        /// The caller will use the first instance of <see cref="ICredentialsProvider"/> in the global cache that satisfies 
        /// the sandbox and futures requirements of the caller, then the explicitly-passed <see cref="ICredentialsProvider"/>,
        /// and then the first available credentials in the global cache.
        /// </remarks>
        AlwaysGlobalFirst,

        /// <summary>
        /// Never use credentials in the global cache, even if they are the only credentials available.
        /// </summary>
        /// <remarks>
        /// Callers will always need their own local <see cref="ICredentialsProvider"/>.
        /// </remarks>
        Never
    }


    /// <summary>
    /// Global KuCoin.NET library methods.
    /// </summary>
    /// <remarks>
    /// This is a starting point for accessing the rest of this library's features.
    /// </remarks>
    public static class KuCoinSystem
    {
        /// <summary>
        /// Gets the system logger.
        /// </summary>
        /// <remarks>
        /// In addition to logging, <see cref="SimpleLog"/> can also be used to open or close the log, or to set a different filename.
        /// </remarks>
        public static SimpleLog Logger { get; } = new SimpleLog(".\\KuCoinNET.log", false);

        /// <summary>
        /// Global application-domain credentials utilization mode.
        /// </summary>
        /// <remarks>
        /// Changes to this property take effect immediately, across the entire application domain.
        /// </remarks>
        public static UseCredentialsMode UseCredentialsMode { get; set; }
        
        /// <summary>
        /// Create a credentials provider to use with the <see cref="IServiceFactory"/> instance acquired from <see cref="GetServiceFactory"/> or <see cref="Initialize"/>.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="sandbox">The API key is a sandbox key.</param>
        /// <param name="futures">The API key is a KuCoin Futures key.</param>
        /// <param name="addToCache">Add these credentials to the global <see cref="KuCoinSystem.Credentials"/> list.</param>
        /// <returns>
        /// A new <see cref="ICredentialsProvider"/> that can be used to connect to any service that requires an API key.
        /// </returns>
        /// <remarks>
        /// This method returns a <see cref="MemoryEncryptedCredentialsProvider"/> instance.  Once your credentials are passed in,
        /// they are encrypted to be stored in memory and the original values are set to null.  The credentials remain encrypted in memory until they are needed by calling the 'Get' methods.
        /// At that point, the credentials are decrypted and returned to the caller.  The caller should dispose of the unencrypted versions as soon as they are no longer needed.
        /// Classes in this assembly only access the unecrypted versions of these values at the exact moment when a request must be sent.
        /// </remarks>
        public static ICredentialsProvider CreateCredentialsProvider(string key, string secret, string passphrase, bool sandbox = false, bool futures = false, bool addToCache = true)
        {
            var result = new MemoryEncryptedCredentialsProvider(key, secret, passphrase, sandbox: sandbox, futures: futures);

            if (addToCache)
            {
                if (Credentials == null)
                {
                    Credentials = new List<ICredentialsProvider>();
                }
                Credentials.Add(result);
            }

            return result;
        }


        /// <summary>
        /// Gets or sets a list that can be used to store <see cref="ICredentialsProvider"/> instances.
        /// </summary>
        public static IList<ICredentialsProvider> Credentials { get; set; } = new List<ICredentialsProvider>();

        /// <summary>
        /// Get the <see cref="ISymbolDataService"/> factory <see cref="IServiceFactory"/> instance.
        /// </summary>
        /// <returns>A service factory that can be used to connect to the various data services available for a ticker symbol.</returns>
        public static IServiceFactory GetServiceFactory()
        {
            return ServiceFactory.Instance;
        }

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <returns>The <see cref="KuCoinSystem.NET.Rest.Market"/> instance.</returns>
        public static Market Market => Market.Instance;

        /// <summary>
        /// Refresh the market data.
        /// </summary>
        /// <returns>The <see cref="KuCoinSystem.NET.Rest.Market"/> instance.</returns>
        public static Market RefreshMarket()
        {
            Market.Instance.RefreshSymbolsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Market.Instance.RefreshCurrenciesAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            return Market.Instance;
        }

        public static async Task<Market> RefreshMarketAsync()
        {
            await Market.Instance.RefreshSymbolsAsync();
            await Market.Instance.RefreshCurrenciesAsync();

            return Market.Instance;
        }

        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        /// <param name="context">The synchronization context.</param>
        /// <returns></returns>
        public static bool InitializeDispatcher(SynchronizationContext context)
        {
            return Dispatcher.Initialize(context);
        }

        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        /// <returns></returns>
        public static bool InitializeDispatcher()
        {
            return Dispatcher.Initialize();
        }

        /// <summary>
        /// Capture the dispatcher, and create and refresh market data.
        /// </summary>
        /// <param name="openLog">True to open the KuCoin.NET system log file.</param>
        /// <param name="logFile">Optional log file name (default is '.\KuCoinNET.log')</param>
        /// <returns>A new see <see cref="IServiceFactory"/> that can be used to retrieve symbol data and subscribe to tickers.</returns>
        /// <remarks>
        /// This method should be called from the primary (dispatcher or UI) thread of an application, most ideally on application startup.
        /// </remarks>
        public static async Task<IServiceFactory> InitializeAsync(bool openLog = false, string logFile = null) 
        {
            if (openLog)
            {
                if (logFile != null)
                {
                    Logger.OpenLog(logFile);
                }
                else
                {
                    Logger.OpenLog();
                }
            }

            InitializeDispatcher();
            await CreateMarket();

            return ServiceFactory.Instance;
        }

        /// <summary>
        /// Initialize the dispatcher with a <see cref="SynchronizationContext"/>, and create and refresh market data.
        /// </summary>
        /// <param name="context">The <see cref="SynchronizationContext"/> object to initialize from.</param>
        /// <param name="openLog">True to open the KuCoin.NET system log file.</param>
        /// <param name="logFile">Optional log file name (default is '.\KuCoinNET.log')</param>
        /// <returns>A new see <see cref="IServiceFactory"/> that can be used to retrieve symbol data and subscribe to tickers.</returns>
        /// <remarks>
        /// This method must be called with a valid <see cref="SynchronizationContext"/> (usually acquired from the dispatch/UI thread).
        /// </remarks>
        public static async Task<IServiceFactory> InitializeAsync(SynchronizationContext context, bool openLog = false, string logFile = null)
        {
            if (openLog)
            {
                if (logFile != null)
                {
                    Logger.OpenLog(logFile);
                }
                else
                {
                    Logger.OpenLog();
                }
            }

            InitializeDispatcher(context);
            await CreateMarket();

            return ServiceFactory.Instance;
        }


        /// <summary>
        /// Capture the dispatcher, and create and refresh market data.
        /// </summary>
        /// <param name="openLog">True to open the KuCoin.NET system log file.</param>
        /// <param name="logFile">Optional log file name (default is '.\KuCoinNET.log')</param>
        /// <returns>A new see <see cref="IServiceFactory"/> that can be used to retrieve symbol data and subscribe to tickers.</returns>
        /// <remarks>
        /// This method should be called from the primary (dispatcher or UI) thread of an application, most ideally on application startup.
        /// </remarks>
        public static IServiceFactory Initialize(bool openLog = false, string logFile = null)
        {
            if (openLog)
            {
                if (logFile != null)
                {
                    Logger.OpenLog(logFile);
                }
                else
                {
                    Logger.OpenLog();
                }
            }

            InitializeDispatcher();
            _ = CreateMarket();

            return ServiceFactory.Instance;
        }

        /// <summary>
        /// Initialize the dispatcher with a <see cref="SynchronizationContext"/>, and create and refresh market data.
        /// </summary>
        /// <param name="context">The <see cref="SynchronizationContext"/> object to initialize from.</param>
        /// <param name="openLog">True to open the KuCoin.NET system log file.</param>
        /// <param name="logFile">Optional log file name (default is '.\KuCoinNET.log')</param>
        /// <returns>A new see <see cref="IServiceFactory"/> that can be used to retrieve symbol data and subscribe to tickers.</returns>
        /// <remarks>
        /// This method must be called with a valid <see cref="SynchronizationContext"/> (usually acquired from the dispatch/UI thread).
        /// </remarks>
        public static IServiceFactory Initialize(SynchronizationContext context, bool openLog = false, string logFile = null)
        {
            if (openLog)
            {
                if (logFile != null)
                {
                    Logger.OpenLog(logFile);
                }
                else
                {
                    Logger.OpenLog();
                }
            }

            InitializeDispatcher(context);
            _ = CreateMarket();

            return ServiceFactory.Instance;
        }

        /// <summary>
        /// Create a trading class with the specified credentials.
        /// </summary>
        /// <param name="credentialsProvider">The <see cref="ICredentialsProvider"/> to use.</param>
        /// <returns></returns>
        public static Trade CreateTradingClass(ICredentialsProvider credentialsProvider)
        {
            return new Trade(credentialsProvider);
        }

        /// <summary>
        /// Create a user class with the specified credentials.
        /// </summary>
        /// <param name="credentialsProvider">The <see cref="ICredentialsProvider"/> to use.</param>
        /// <returns></returns>
        public static User CreateUserClass(ICredentialsProvider credentialsProvider)
        {
            return new User(credentialsProvider);
        }

        private static async Task CreateMarket()
        {
            await Market.CreateMarket(true);
        }

    }
}
