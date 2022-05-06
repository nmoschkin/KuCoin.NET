using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace KuCoin.NET.Rest
{
    /// <summary>
    /// The base class for all KuCoin communication, including KuCoin and KuCoin Futures, REST and websocket APIs.
    /// </summary>
    public abstract class KucoinBaseRestApi : ObservableBase
    {
        private static readonly JsonSerializerSettings defaultSettings;
        protected static object requestLockObject = new object();

        protected string url;
        protected MemoryEncryptedCredentialsProvider cred;

        protected bool isSandbox;
        protected bool isFutures;
        protected bool isv1api;

        protected static long lastTime = DateTime.UtcNow.Ticks;


        /// <summary>
        /// Gets credentials for access.  This may be null for classes implementing public APIs.
        /// </summary>
        internal ICredentialsProvider Credentials => cred;

        /// <summary>
        /// The base Url for all calls.
        /// </summary>
        public virtual string Url => url;

        /// <summary>
        /// True if object created in sandbox mode.
        /// </summary>
        public virtual bool IsSandbox => isSandbox;


        /// <summary>
        /// True if object created with KuCoin Futures API endpoints.
        /// </summary>
        public virtual bool IsFutures => isFutures;


        /// <summary>
        /// True if using the Version 1 API
        /// </summary>
        public virtual bool IsV1Api => isv1api;

        /// <summary>
        /// Create a new unique Id
        /// </summary>
        public virtual string GetUniqueId()
        {
            return Guid.NewGuid().ToString("d").Replace("-", "");
        }

        /// <summary>
        /// Initialize a new instance of a REST API client with a credentials provider.
        /// </summary>
        /// <param name="credProvider">An object that implements <see cref="ICredentialsProvider"/>.</param>
        /// <param name="url">(Optional) Alternate REST API endpoint.</param>
        /// <param name="isv1api">Is version 1 API.</param>
        /// <param name="futures">Use KuCoin Futures API endpoints.</param>
        /// <remarks>
        /// If <paramref name="credProvider"/> is null, then this class will be initialized for public API use.
        /// If <paramref name="credProvider"/> is not null, then this class will be initialized for private API use,
        /// the <paramref name="futures"/> parameter is ignored, and the value of <see cref="ICredentialsProvider.GetFutures"/> is used, instead.
        /// </remarks>
        public KucoinBaseRestApi(
            ICredentialsProvider credProvider,
            string url = null,
            bool isv1api = false,
            bool futures = false)
        {
            this.isv1api = isv1api;

            if (credProvider != null)
            {
                cred = new MemoryEncryptedCredentialsProvider(credProvider);
                futures = cred.GetFutures();
            }

            SetDefaultUrl(futures, cred?.GetSandbox() ?? false, url);
        }

        /// <summary>
        /// Initialize a new instance of a REST API client with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is sandbox / not real-time.</param>
        /// <param name="url">(Optional) Alternate REST API endpoint.</param>
        /// <param name="isv1api">Is version 1 API.</param>
        /// <param name="futures">Use KuCoin Futures API endpoints.</param>
        /// <remarks>
        /// If <paramref name="key"/>, <paramref name="secret"/>, or <paramref name="passphrase"/> are null, then this class will be initialized for public API use.
        /// </remarks>
        public KucoinBaseRestApi(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            bool futures = false
            )
        {
            this.isv1api = isv1api;

            if (key != null && secret != null && passphrase != null)
            {
                cred = new MemoryEncryptedCredentialsProvider(key, secret, passphrase, null, isSandbox);

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                key = secret = passphrase = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                GC.Collect();
            }

            SetDefaultUrl(futures, isSandbox, url);
        }

        /// <summary>
        /// Sets the default URL for this instance based on the criteria.
        /// </summary>
        /// <param name="futures">Futures endpoint.</param>
        /// <param name="sandbox">Sandbox endpoint.</param>
        /// <param name="url">Optional alternate URL (overrides <paramref name="futures"/> and <paramref name="sandbox"/>)</param>
        private void SetDefaultUrl(bool futures, bool sandbox, string url = null)
        {
            if (url != null)
            {
                this.url = url;
            }
            else if (futures)
            {
                if (sandbox)
                {
                    this.url = "https://api-sandbox-futures.kucoin.com";
                }
                else
                {
                    this.url = "https://api-futures.kucoin.com";
                }
            }
            else
            {
                if (sandbox)
                {
                    this.url = "https://openapi-sandbox.kucoin.com";
                }
                else
                {
                    this.url = "https://api.kucoin.com";
                }
            }

            this.isSandbox = sandbox;
            this.isFutures = futures;
        }

        /// <summary>
        /// Global initialization work.
        /// </summary>
        static KucoinBaseRestApi()
        {
            // Create the default JSON deserialization settings
            // and set the new default contract resolver.
            defaultSettings = new JsonSerializerSettings()
            {
                // We are changing the fundamental global behavior for deserialization 
                // with a new default data contract resolver.
                // This is set specifically for cases where we must deserialize decimal entities from strings.
                ContractResolver = DataContractResolver.Instance,

                // We want to overwrite observable entities with new data
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,

                // We don't care about nothing
                NullValueHandling = NullValueHandling.Ignore,

                Converters = new JsonConverter[]
                {
                    new StringToDecimalConverter()
                },

                FloatParseHandling = FloatParseHandling.Decimal

            };

            // Set the new global default behavior for the JSON library.
            JsonConvert.DefaultSettings = () =>
            {
                return defaultSettings;
            };

            try
            {
                // It's always good to attempt to initialize the dispatcher 
                // at our first opportunity.  
                Dispatcher.Initialize();
            }
            catch { }
        }

        /// <summary>
        /// Create an SHA256 HMAC Hash From a Message and Secret.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="secret">The secret to hash against.</param>
        /// <returns>A Base-64 Encoded Hash String.</returns>
        protected static string CreateToken(string message, string secret)
        {
            secret = secret ?? "";

            byte[] keyByte = Encoding.ASCII.GetBytes(secret);
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        /// <summary>
        /// Joins two Url pieces into a Uri.
        /// </summary>
        /// <param name="baseUrl">The base Url.</param>
        /// <param name="relUrl">The relative Url.</param>
        /// <returns>An absolute Uri.</returns>
        public static Uri UriJoin(string baseUrl, string relUrl)
        {
            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
            }

            if (relUrl[0] == '/')
            {
                return new Uri(baseUrl + relUrl);
            }
            else
            {
                return new Uri(baseUrl + "/" + relUrl);
            }
        }

        /// <summary>
        /// Resolve the correct credentials according to the global <see cref="KuCoinSystem.UseCredentialsMode"/> setting.
        /// </summary>
        /// <returns></returns>
        protected ICredentialsProvider ResolveCredentials()
        {
            ICredentialsProvider cred = this.cred;

            var ucm = KuCoinSystem.UseCredentialsMode;
            
            if (ucm == UseCredentialsMode.Never)
            {
                return cred;
            }

            if (ucm == UseCredentialsMode.Default || ucm == UseCredentialsMode.Always && cred != null)
            {
                return cred;
            }
            else
            {
                cred = null;
            }

            if (KuCoinSystem.Credentials != null && KuCoinSystem.Credentials.Count > 0)
            {
                foreach (var checkCred in KuCoinSystem.Credentials)
                {
                    if (checkCred.GetFutures() == IsFutures && checkCred.GetSandbox() == IsSandbox)
                    {
                        cred = checkCred;
                        break;
                    }
                }

                if (cred == null && KuCoinSystem.UseCredentialsMode == UseCredentialsMode.Always)
                {
                    cred = KuCoinSystem.Credentials[0];
                }

            }

            return cred ?? this.cred;
        }

        /// <summary>
        /// Gets the results of a paginated list of <typeparamref name="TItem"/> by page.
        /// </summary>
        /// <typeparam name="TItem">A type of result to return.</typeparam>
        /// <typeparam name="TPage">A page of the type of result to return (must implement <see cref="IPaginated{T}"/> of <typeparamref name="TItem"/>.)</typeparam>
        /// <param name="method">The <see cref="HttpMethod"/> of the new call.</param>
        /// <param name="uri">The relative path of the endpoint.</param>
        /// <param name="currentPage">The page to retrieve.</param>
        /// <param name="pageSize">The maximum size of each page.</param>
        /// <param name="timeout">Timeout value, in seconds.</param>
        /// <param name="auth">True if the call is authenticated.</param>
        /// <param name="reqParams">Optional parameters for the call.</param>
        /// <param name="wholeResponseJson">True to return the whole response, or false to just return the 'data' portion.</param>
        /// <returns></returns>
        protected async Task<IList<TItem>> GetResultsByPage<TItem, TPage>(
            HttpMethod method,
            string uri,
            int currentPage = 1,
            int pageSize = 50,
            int timeout = 10,
            bool auth = true,
            IDictionary<string, object> reqParams = null,
            bool wholeResponseJson = false)
            where TItem : class, IDataObject, new()
            where TPage : class, IPaginated<TItem>, new()
        {
            var param = new Dictionary<string, object>();
            var l = new List<TItem>();

            param.Add("pageSize", pageSize);
            param.Add("currentPage", currentPage);

            var jobj = await MakeRequest(method, uri, timeout, auth, reqParams, wholeResponseJson);

            TPage p = null;

            try
            {
                p = jobj.ToObject<TPage>();
                l.AddRange(p.Items);
            }
            catch (Exception jex)
            {
                Console.Write(jex.Message);
            }

            return l;
        }

        /// <summary>
        /// Gets all results of a paginated list of <typeparamref name="TItem"/>.
        /// </summary>
        /// <typeparam name="TItem">A type of result to return.</typeparam>
        /// <typeparam name="TPage">A page of the type of result to return (must implement <see cref="IPaginated{T}"/> of <typeparamref name="TItem"/>.)</typeparam>
        /// <param name="method">The <see cref="HttpMethod"/> of the new call.</param>
        /// <param name="uri">The relative path of the endpoint.</param>
        /// <param name="timeout">Timeout value, in seconds.</param>
        /// <param name="auth">True if the call is authenticated.</param>
        /// <param name="reqParams">Optional parameters for the call.</param>
        /// <param name="wholeResponseJson">True to return the whole response, or false to just return the 'data' portion.</param>
        /// <returns></returns>
        protected async Task<IList<TItem>> GetAllPaginatedResults<TItem, TPage>(
            HttpMethod method,
            string uri,
            int timeout = 10,
            bool auth = true,
            IDictionary<string, object> reqParams = null,
            bool wholeResponseJson = false)
            where TItem : class, IDataObject, new()
            where TPage : class, IPaginated<TItem>, new()
        {
            int pageSize = 500;
            int currPage = 1;

            var param = new Dictionary<string, object>();
            var l = new List<TItem>();

            param.Add("pageSize", pageSize);
            param.Add("currentPage", currPage);

            var jobj = await MakeRequest(method, uri, timeout, auth, reqParams, wholeResponseJson);

            TPage p = null;

            try
            {
                p = jobj.ToObject<TPage>();
                l.AddRange(p.Items);
            }
            catch (Exception jex)
            {
                Console.Write(jex.Message);
            }

            if (p.TotalNumber > pageSize)
            {
                int total = p.TotalNumber;

                for (int i = pageSize; i < total; i += pageSize)
                {
                    param["currentPage"] = ++currPage;
                    jobj = await MakeRequest(method, uri, timeout, auth, reqParams, wholeResponseJson);
                    p = jobj.ToObject<TPage>();

                    l.AddRange(p.Items);
                }
            }

            return l;
        }

        
        /// <summary>
        /// Gets the last error encountered by the most recent invocation of <see cref="BeginMakeRequest(Action{JToken}, HttpMethod, string, int, bool, IDictionary{string, object}, bool)"/>.
        /// </summary>
        public Exception LastCallBackError { get; protected set; }


        /// <summary>
        /// Begin making a new request to the API endpoint with a callback function.
        /// </summary>
        /// <param name="callback">The callback function to execute after the connection has returned or timed out.</param>
        /// <param name="method">The <see cref="HttpMethod"/> of the new call.</param>
        /// <param name="uri">The relative path of the endpoint.</param>
        /// <param name="timeout">Timeout value, in seconds.</param>
        /// <param name="auth">True if the call is authenticated.</param>
        /// <param name="reqParams">Optional parameters for the call.</param>
        /// <param name="wholeResponseJson">True to return the whole response, or false to just return the 'data' portion.</param>
        /// <remarks>
        /// A <see cref="JToken"/> object that can be deserialized to suit members in the derived class will be passed to the <paramref name="callback"/> function.
        /// <br /><br />This function returns immediately. Results should be handled by the <paramref name="callback"/> function.
        /// <br /><br />In case of error, a null value is returned. The exception can be retrieved from the <see cref="LastCallBackError"/> property.
        /// </remarks>
        /// 
        protected void BeginMakeRequest(
            Action<JToken> callback,
            HttpMethod method,
            string uri,
            int timeout = 10,
            bool auth = true,
            IDictionary<string, object> reqParams = null,
            bool wholeResponseJson = false)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await MakeRequest(method, uri, timeout, auth, reqParams, wholeResponseJson);
                    callback(result);
                }
                catch (Exception ex)
                {                   
                    LastCallBackError = ex;
                    callback(null);
                }
            });
        }

        /// <summary>
        /// Make a new request to the API endpoint.
        /// </summary>
        /// <param name="method">The <see cref="HttpMethod"/> of the new call.</param>
        /// <param name="uri">The relative path of the endpoint.</param>
        /// <param name="timeout">Timeout value, in seconds.</param>
        /// <param name="auth">True if the call is authenticated.</param>
        /// <param name="reqParams">Optional parameters for the call.</param>
        /// <param name="wholeResponseJson">True to return the whole response, or false to just return the 'data' portion.</param>
        /// <returns>A <see cref="JToken"/> object that can be deserialized to suit members in the derived class.</returns>
        protected async Task<JToken> MakeRequest(
            HttpMethod method,
            string uri,
            int timeout = 10,
            bool auth = true,
            IDictionary<string, object> reqParams = null,
            bool wholeResponseJson = false)
        {
            string token;
            string json_data = reqParams != null ? JsonConvert.SerializeObject(reqParams) : "";

            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                if (reqParams != null && reqParams.Count > 0)
                {
                    // sort the keys as per the Python API
                    var sdict = new SortedDictionary<string, object>(reqParams);
                    var sb = new StringBuilder();

                    foreach (var kv in sdict)
                    {
                        if (sb.Length != 0) sb.Append("&");

                        string sval;

                        if (kv.Value is Enum)
                        {
                            sval = EnumToStringConverter<Enum>.GetEnumName((Enum)kv.Value);
                        }
                        else
                        {
                            sval = kv.Value.ToString();
                        }

                        sb.Append(string.Format("{0}={1}", kv.Key, sval));
                    }

                    uri += "?" + sb.ToString();
                }

                token = uri;
            }
            else
            {
                token = uri + json_data;
            }

            Uri requri = UriJoin(this.url, uri);

            HttpRequestMessage req = new HttpRequestMessage(method, requri);

            req.Headers.Clear();

            // KuCoin API Authentication Magic

            ICredentialsProvider cred = ResolveCredentials();
           
            if (auth && cred != null)
            {
                var now_time = EpochTime.DateToSeconds(DateTime.Now) * 1000;
                var str_to_sign = now_time.ToString() + method.Method + token;

                var sign = CreateToken(str_to_sign, cred.GetSecret());

                if (isv1api)
                {
                    req.Headers.Add("KC-API-SIGN", sign);
                    req.Headers.Add("KC-API-TIMESTAMP", now_time.ToString());
                    req.Headers.Add("KC-API-KEY", cred.GetKey());
                    req.Headers.Add("KC-API-PASSPHRASE", cred.GetPassphrase());
                }
                else
                {
                    var passphrase = CreateToken(cred.GetPassphrase(), cred.GetSecret());

                    req.Headers.Add("KC-API-SIGN", sign);
                    req.Headers.Add("KC-API-TIMESTAMP", now_time.ToString());
                    req.Headers.Add("KC-API-KEY", cred.GetKey());
                    req.Headers.Add("KC-API-PASSPHRASE", passphrase);
                    req.Headers.Add("KC-API-KEY-VERSION", "2");
                }
            }

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, timeout);

                if (method == HttpMethod.Get)
                {
                    json_data = "";
                }

                req.Content = new StringContent(json_data, Encoding.UTF8, "application/json");

                long nt = DateTime.UtcNow.Ticks;

                lock (requestLockObject)
                {
                    if (nt - lastTime < 2_500_000)
                    {
                        Thread.Sleep(250);
                    }

                    lastTime = nt;
                }

                var resp = await httpClient.SendAsync(req);

                var result = await CheckResponseData(resp, wholeResponseJson);
                return result;
            }
        }

        /// <summary>
        /// Check if the response is good.
        /// </summary>
        /// <param name="response_data">The data to check.</param>
        /// <param name="wholeResponseJson">True to return the whole response, or false to just return the 'data' portion.</param>
        /// <returns>A <see cref="JToken"/> object that can be deserialized to suit members in the derived class.</returns>
        protected static async Task<JToken> CheckResponseData(HttpResponseMessage response_data, bool wholeResponseJson = false)
        {
            string content = await response_data.Content.ReadAsStringAsync();

            if (response_data.IsSuccessStatusCode)
            {
                JObject data;
                try
                {
                    data = JObject.Parse(content);
                }
                catch
                {
                    throw new Exception(string.Format("{0}-{1}", response_data.StatusCode, content));
                }

                if (wholeResponseJson && data != null)
                {
                    return data;
                }
                else if (data != null && data.ContainsKey("code"))
                {
                    if (data["code"].ToObject<string>() == "200000")
                    {
                        if (data.ContainsKey("data"))
                        {
                            return data["data"];
                        }
                        else
                        {
                            return data;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}-{1}", response_data.StatusCode, content));
                    }
                }
                else
                {
                    throw new Exception(string.Format("{0}-{1}", response_data.StatusCode, content));
                }
            }
            else
            {
                throw new Exception(string.Format("{0}-{1}", response_data.StatusCode, content));
            }
        }
    }
}