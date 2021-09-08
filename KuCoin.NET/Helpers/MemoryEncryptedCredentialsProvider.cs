using KuCoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace KuCoin.NET.Helpers
{
    /// <summary>
    /// Stores credentials encrypted in memory until needed.
    /// </summary>
    public class MemoryEncryptedCredentialsProvider : ObservableBase, ICredentialsProvider
    {
        protected string key;
        protected string secret;
        protected string passphrase;
        protected bool sandbox;
        protected bool futures;

        protected Guid seed;

        protected ICredentialsProvider attachedAccount;

        /// <summary>
        /// Initialize object from another <see cref="ICredentialsProvider"/> implementation.
        /// </summary>
        /// <param name="credProvider">The source provider.</param>
        /// <param name="seed">UUID encryption seed (optional)</param>
        public MemoryEncryptedCredentialsProvider(ICredentialsProvider credProvider, Guid? seed = null)
        {
            if (seed == null)
            {
                this.seed = MakeZipper();
            }
            else
            {
                this.seed = MakeZipper((Guid)seed);
            }

            if (credProvider == null) return;

            this.key = EncryptIt(credProvider.GetKey());
            this.secret = EncryptIt(credProvider.GetSecret());
            this.passphrase = EncryptIt(credProvider.GetPassphrase());
            this.sandbox = credProvider.GetSandbox();
            this.futures = credProvider.GetFutures();
        }

        /// <summary>
        /// Initialize object from raw credentials.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="secret">Secret </param>
        /// <param name="passphrase">Passphrase</param>
        /// <param name="seed">UUID encryption seed (optional)</param>
        /// <param name="sandbox">Is a sandbox account</param>
        /// <param name="futures">Is a futures account</param>
        public MemoryEncryptedCredentialsProvider(string key, string secret, string passphrase, Guid? seed = null, bool sandbox = false, bool futures = false)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value

            if (seed == null)
            {
                this.seed = MakeZipper();
            }
            else
            {
                this.seed = MakeZipper((Guid)seed);
                seed = null;
            }


            if (!string.IsNullOrEmpty(key)) this.key = EncryptIt(key);
            key = null;

            if (!string.IsNullOrEmpty(secret)) this.secret = EncryptIt(secret);
            secret = null;

            if (!string.IsNullOrEmpty(passphrase)) this.passphrase = EncryptIt(passphrase);
            passphrase = null;

            this.sandbox = sandbox;
            this.futures = futures;

#pragma warning restore IDE0059 // Unnecessary assignment of a value

            GC.Collect();

        }


        /// <summary>
        /// Gets or sets the value of another account attached to this instance (e.g. a KuCoin Futures account).
        /// </summary>
        public virtual ICredentialsProvider AttachedAccount
        {
            get => attachedAccount;
            set => attachedAccount = value;
        }

        // AES does the real encryption, but these functions make the data a little trickier to decipher.
        #region Mangling

        int keystone = -1;

        protected virtual Guid MakeZipper(Guid? seed = null)
        {
            var g = seed ?? Guid.NewGuid();
                        
            var b = g.ToByteArray();
            var bout = new byte[16];
            if (keystone == -1)
            {
                foreach (var btest in b)
                {
                    if ((~btest & 0xff) <= 15)
                    {
                        keystone = ~btest & 0xff;
                        break;
                    }
                }

                if (keystone == -1)
                {
                    var bt = (byte)(DateTime.Now.Ticks & 0xffL);
                    keystone = (~bt) & 0xf;
                }
            }

            Array.Reverse(b);

            int j = 0;

            for(int i = keystone; i < 16; i++)
            {
                bout[j++] = (byte)(~b[i] & 0xff);
            }

            for (int i = 0; i < keystone; i++)
            {
                bout[j++] = (byte)(~b[i] & 0xff);
            }

            var result = new Guid(bout);
            return result;
        }

        protected virtual int bulkadd(IEnumerable<int> values)
        {
            int i = 0;
            foreach (int j in values)
            {
                i += j;
            }

            return i;
        }

        protected virtual List<int> get_lengths(string value)
        {
            int thax;
            List<int> lengths = new List<int>();

            while (int.TryParse(value.Substring(value.Length - 4), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out thax))
            {
                lengths.Add((short)~thax);
                value = value.Substring(0, value.Length - 4);

                if (bulkadd(lengths) == value.Length) break;
            }

            lengths.Reverse();
            return lengths;
        }

        protected virtual char zip_right(char ch, byte[] zipper = null, char lastChar = '0')
        {
            var s = (int)ch;

            s = (~lastChar) ^ (char)s;

            if (zipper == null) zipper = MakeZipper(this.seed).ToByteArray();

            foreach (var b in zipper)
            {
                s = s ^ (char)b;
            }

            return (char)~(s & 0xffff);
        }


        protected virtual char zip_left(char ch, byte[] zipper = null, char lastChar = '0')
        {
            var s = (int)ch;

            if (zipper == null) zipper = MakeZipper(this.seed).ToByteArray();
            Array.Reverse(zipper);

            foreach (var b in zipper)
            {
                s = s ^ b;
            }

            s = (~lastChar) ^ (char)s;

            return (char)~(s & 0xffff);
        }

        protected virtual string zip_right(string s)
        {
            var sb = new StringBuilder();
            var zipper = MakeZipper(this.seed).ToByteArray();
            char lch = '0';
            char cha;

            foreach (char ch in s)
            {
                cha = zip_right(ch, zipper, lch);
                sb.Append(cha);
                lch = ch;
            }

            return sb.ToString();
        }

        protected virtual string zip_left(string s)
        {
            var sb = new StringBuilder();
            var zipper = MakeZipper(this.seed).ToByteArray();
            char lch = '0';
            char cha;

            foreach (char ch in s)
            {
                cha = zip_left(ch, zipper, lch);
                sb.Append(cha);
                lch = cha;
            }

            return sb.ToString();
        }

        protected virtual string multi_zip_right(params string[] values)
        {
            var sb = new StringBuilder();
            var zipper = MakeZipper(this.seed).ToByteArray();
            char lch = '0';
            char cha;


            foreach (var s in values)
            {
                foreach (char ch in s)
                {
                    cha = zip_right(ch, zipper, lch);
                    sb.Append(cha);
                    lch = ch;
                }
            }

            foreach (var s in values)
            {
                short lb = (short)~s.Length;
                sb.Append(lb.ToString("x04"));
            }

            return sb.ToString();
        }

        protected virtual string[] multi_zip_left(string value)
        {
            var sb = new StringBuilder();
            var zipper = MakeZipper(this.seed).ToByteArray();
            char lch = '0';
            char cha;

            List<int> lengths = get_lengths(value);
            List<string> values = new List<string>();
            List<string> results = new List<string>();

            foreach (var l in lengths)
            {
                values.Add(value.Substring(0, l));
                value = value.Substring(l);
            }

            foreach (var s in values)
            {
                foreach (char ch in s)
                {
                    cha = zip_left(ch, zipper, lch);
                    sb.Append(cha);
                    lch = cha;
                }

                results.Add(sb.ToString());
                sb.Clear();
            }

            return results.ToArray();

        }

        #endregion

        protected virtual string EncryptIt(string source)
        {
            if (source == null) return null;
            var work = zip_right(source);
            return AesOperation.EncryptString(MakeZipper(this.seed), work, out _);
        }

        protected virtual string DecryptIt(string source)
        {
            if (source == null) return null;
            var work = AesOperation.DecryptString(MakeZipper(this.seed), source);
            return zip_left(work);
        }

        public virtual string GetKey()
        {
            return !string.IsNullOrEmpty(key) ? DecryptIt(key) : null;
        }

        public virtual string GetPassphrase()
        {
            return !string.IsNullOrEmpty(passphrase) ? DecryptIt(passphrase) : null;
        }

        public virtual string GetSecret()
        {
            return !string.IsNullOrEmpty(secret) ? DecryptIt(secret) : null;
        }

        public virtual bool GetSandbox() => sandbox;


        public virtual bool GetFutures() => futures;

    }
}
