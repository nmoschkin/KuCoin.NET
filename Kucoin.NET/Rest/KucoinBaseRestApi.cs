using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Json;
using Kucoin.NET.Data;

namespace Kucoin.NET.Rest
{
    public class KucoinBaseRestApi : ObservableBase
    {
        static readonly JsonSerializerSettings defaultSettings;
        
        protected string url;
        protected MemoryEncryptedCredentialsProvider cred;

        protected bool isSandbox;
        protected bool isv1api;

        /// <summary>
        /// Gets credentials for access.
        /// </summary>
        internal ICredentialsProvider Credentials => cred;

        /// <summary>
        /// The base Url for all calls.
        /// </summary>
        public virtual string Url => url;

        /// <summary>
        /// True if running in sandbox mode.
        /// </summary>
        public virtual bool IsSandbox => cred?.GetSandbox() ?? isSandbox;

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
        /// <param name="url">REST API url.</param>
        /// <param name="isv1api">Is version 1 api.</param>
        public KucoinBaseRestApi(
            ICredentialsProvider credProvider,
            string url = null, 
            bool isv1api = false,
            bool futures = false) 
            : this(
                  null, 
                  null, 
                  null, 
                  false, 
                  url, 
                  isv1api,
                  futures)
        {
            if (credProvider != null)
            {
                cred = new MemoryEncryptedCredentialsProvider(credProvider);
                isSandbox = cred.GetSandbox();
            }
        }

        /// <summary>
        /// https://docs.kucoin.com
        /// </summary>
        /// <param name="key">Api Token Id(Mandatory)</param>
        /// <param name="secret">Api Secret(Mandatory)</param>
        /// <param name="passphrase">Api Passphrase used to create API(Mandatory)</param>
        /// <param name="isSandbox">Is sandbox / not real-time.</param>
        /// <param name="url">REST API url.</param>
        /// <param name="isv1api">Is version 1 api.</param>
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

            if (!string.IsNullOrEmpty(url))
            {
                this.url = url;
            }
            else
            {
                if (futures)
                {
                    if (isSandbox)
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
                    if (isSandbox)
                    {
                        this.url = "https://openapi-sandbox.kucoin.com";
                    }
                    else
                    {
                        this.url = "https://api.kucoin.com";
                    }
                }
            }
            
            if (key != null && secret != null)
            {
                cred = new MemoryEncryptedCredentialsProvider(key, secret, passphrase, null, isSandbox);

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                key = secret = passphrase = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                GC.Collect();
            }

            this.isSandbox = isSandbox;
            this.isv1api = isv1api;
        }

        
        static KucoinBaseRestApi()
        {
            defaultSettings = new JsonSerializerSettings()
            {
                ContractResolver = DataContractResolver.Instance,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore
            };

            JsonConvert.DefaultSettings = () => 
            {
                return defaultSettings;
            };

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
        /// Gets the results of a paginated list of <see cref="R"/> by page.
        /// </summary>
        /// <typeparam name="TItem">A type of result to return.</typeparam>
        /// <typeparam name="TPage">A page of the type of result to return (must implement <see cref="IPaginated{T}"/> of <see cref="R"/>.</typeparam>
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
            int timeout = 5, 
            bool auth = true, 
            IDictionary<string, object> reqParams = null, 
            bool wholeResponseJson = false) 
            where TItem : class, new() 
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
        /// Gets all results of a paginated list of <see cref="R"/>.
        /// </summary>
        /// <typeparam name="TItem">A type of result to return.</typeparam>
        /// <typeparam name="TPage">A page of the type of result to return (must implement <see cref="IPaginated{T}"/> of <see cref="R"/>.</typeparam>
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
            int timeout = 5,
            bool auth = true, 
            IDictionary<string, object> reqParams = null, 
            bool wholeResponseJson = false) 
            where TItem : class, new() 
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
            int timeout = 5, 
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

                var resp = await httpClient.SendAsync(req);

                return await CheckResponseData(resp, wholeResponseJson);
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