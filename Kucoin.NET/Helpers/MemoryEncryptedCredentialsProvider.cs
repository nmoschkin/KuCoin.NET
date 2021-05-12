using Kucoin.NET.Data.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Helpers
{
    /// <summary>
    /// Stores credentials encrypted in memory until needed.
    /// </summary>
    public class MemoryEncryptedCredentialsProvider : ICredentialsProvider
    {
        protected string key;
        protected string secret;
        protected string passphrase;

        protected Guid seed;

        /// <summary>
        /// Initialize object from another <see cref="ICredentialsProvider"/> implementation.
        /// </summary>
        /// <param name="credProvider">The source provider.</param>
        /// <param name="seed">UUID encryption seed (optional)</param>
        public MemoryEncryptedCredentialsProvider(ICredentialsProvider credProvider, Guid? seed = null)
        {
            if (seed == null)
            {
                this.seed = Makeseed();
            }
            else
            {
                this.seed = Makeseed((Guid)seed);
            }

            this.key = AesOperation.EncryptString(Makeseed(this.seed), credProvider.GetKey(), out _);
            this.secret = AesOperation.EncryptString(Makeseed(this.seed), credProvider.GetSecret(), out _);
            this.passphrase = AesOperation.EncryptString(Makeseed(this.seed), credProvider.GetPassphrase(), out _);

        }

        /// <summary>
        /// Initialize object from raw credentials.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="secret">Secret </param>
        /// <param name="passphrase">Passphrase</param>
        /// <param name="seed">UUID encryption seed (optional)</param>
        public MemoryEncryptedCredentialsProvider(string key, string secret, string passphrase, Guid? seed = null)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value

            if (seed == null)
            {
                this.seed = Makeseed();
            }
            else
            {
                this.seed = Makeseed((Guid)seed);
                seed = null;
            }

            if (!string.IsNullOrEmpty(key)) this.key = AesOperation.EncryptString(Makeseed(this.seed), key, out _);
            key = null;

            if (!string.IsNullOrEmpty(secret)) this.secret = AesOperation.EncryptString(Makeseed(this.seed), secret, out _);
            secret = null;

            if (!string.IsNullOrEmpty(passphrase)) this.passphrase = AesOperation.EncryptString(Makeseed(this.seed), passphrase, out _);
            passphrase = null;

#pragma warning restore IDE0059 // Unnecessary assignment of a value

            GC.Collect();

        }

        protected static Guid Makeseed(Guid? seed = null)
        {
            var g = seed ?? Guid.NewGuid();

            var b = g.ToByteArray();
            Array.Reverse(b);

            for(int i = 0; i < 16; i++)
            {
                b[i] = (byte)(~b[i] & 0xff);
            }

            return new Guid(b);
        }

        public virtual string GetKey()
        {
            return !string.IsNullOrEmpty(key) ? AesOperation.DecryptString(Makeseed(seed), key) : null;
        }

        public virtual string GetPassphrase()
        {
            return !string.IsNullOrEmpty(passphrase) ? AesOperation.DecryptString(Makeseed(seed), passphrase) : null;
        }

        public virtual string GetSecret()
        {
            return !string.IsNullOrEmpty(secret) ? AesOperation.DecryptString(Makeseed(seed), secret) : null;
        }
    }
}
