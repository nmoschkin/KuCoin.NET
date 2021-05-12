using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Text;


namespace KuCoinApp
{
    public class CryptoCredentials : ObservableBase, ICredentialsProvider
    {
        protected static string pin;

        protected string name;

        protected string key;

        protected string secret;

        protected string passphrase;

        protected MemoryEncryptedCredentialsProvider cred;

        protected bool isClear = true;

        protected bool isPassphraseRequired = true;

        protected Guid seed;

        protected CryptoCredentials()
        {

        }

        /// <summary>
        /// Gets or sets the working Pin.
        /// </summary>
        /// <remarks>
        /// This property is static to enable the user not having to enter their pin unnecessarily.
        /// </remarks>
        public static string Pin
        {
            get => pin;
            set
            {
                if (ValidatePin(value))
                {
                    pin = value;
                }
            }
        }

        /// <summary>
        /// Validate that the pin is exactly 6 digits.
        /// </summary>
        /// <param name="pin">The pin number.</param>
        /// <returns></returns>
        public static bool ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return false;

            if (pin.Length == 6)
            {
                foreach (char ch in pin)
                {
                    if (!char.IsNumber(ch)) return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DeleteCredentials(string pin = null)
        {
            if (pin == null) pin = CryptoCredentials.pin;
            if (!ValidatePin(pin)) return;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Join(path, "KuCoin.NET");
            if (!Directory.Exists(path)) return;

            var file = GetCryptName(pin);

            path = Path.Join(path, file);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        /// <summary>
        /// Loads API Key information from encrypted storage.
        /// </summary>
        /// <param name="seed">The seed to use for encrypting.</param>
        /// <param name="pin">The pin to use to decrypt the storage.</param>
        /// <param name="create">True to create a new credentials file that does not exist.</param>
        /// <returns></returns>
        public static CryptoCredentials LoadFromStorage(Guid seed, string pin = null, bool create = true)
        {
            if (pin == null) pin = CryptoCredentials.pin;

            if (!ValidatePin(pin)) return null;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cred = new CryptoCredentials();

            path = Path.Join(path, "KuCoin.NET");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var file = GetCryptName(pin);

            path = Path.Join(path, file);

            if (!File.Exists(path))
            {
                if (create)
                {
                    cred.Seed = seed;
                    return cred;
                }
                else
                {
                    return null;
                }
            }

            var crypted = File.ReadAllText(path);

            try
            {
                cred.Seed = seed;

                var json = AesOperation.DecryptString(GetCipherSeed(seed, pin), crypted);
                JsonConvert.PopulateObject(json, cred);

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                json = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            }
            catch
            {
                File.Delete(path);
            }

            cred.CheckIsClear();
            Pin = pin;

            GC.Collect();

            return cred;
        }

        /// <summary>
        /// Generate a filename for storing the encrypted data using the provided pin.
        /// </summary>
        /// <param name="pin">The pin to use.</param>
        /// <returns></returns>
        protected static string GetCryptName(string pin)
        {
            var seedPath = new Guid(Encoding.UTF8.GetBytes(pin + "0e!._tt_t0"));
            var results = AesOperation.EncryptString(seedPath, pin, out _);

            StringBuilder sb = new StringBuilder();

            foreach (char ch in results)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a new seed from a seed and a pin.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="pin">The 6-digit pin.</param>
        /// <returns></returns>
        protected static Guid GetCipherSeed(Guid seed, string pin)
        {
            byte[] raw;
            if (pin.Length != 6) throw new ArgumentException("Pin must be exactly 6 characters.");

            AesOperation.EncryptString(seed, pin, out raw);
            return new Guid(raw);
        }


        /// <summary>
        /// True if the contents are not clean
        /// </summary>
        [JsonIgnore]
        public bool IsDirty => !isClear;

        /// <summary>
        /// True if all the required fields have been populated.
        /// </summary>
        [JsonIgnore]
        public bool IsFilled
        {
            get
            {
                return (!string.IsNullOrEmpty(secret) && (!isPassphraseRequired || !string.IsNullOrEmpty(passphrase)) && !string.IsNullOrEmpty(key));
            }
        }

        /// <summary>
        /// The seed that is used to encrypt this set of credentials.
        /// </summary>
        /// <remarks>
        /// The value can only be assigned via the <see cref="LoadFromStorage(Guid, string, bool)"/> method.
        /// </remarks>
        [JsonIgnore]
        public Guid Seed
        {
            get => Makeseed(seed);
            protected set
            {
                var newSeed = Makeseed(value);

                if (newSeed != seed)
                {
                    seed = newSeed;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True if all properties are empty.
        /// </summary>
        [JsonIgnore]
        public bool IsClear
        {
            get => isClear;
            protected set
            {
                if (SetProperty(ref isClear, value))
                {
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the passphrase component of the API credentials is a required field.
        /// </summary>
        [JsonProperty("passphraseRequired")]
        public bool IsPassphraseRequired
        {
            get => isPassphraseRequired;
            set
            {
                if (SetProperty(ref isPassphraseRequired, value)) CheckIsClear();
            }
        }

        /// <summary>
        /// Gets or sets the name of this credential set.
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value)) CheckIsClear();
            }
        }

        /// <summary>
        /// Gets or sets the API Key.
        /// </summary>
        [JsonProperty("key")]
        public string Key
        {
            get => GetKey();
            set
            {
                var str = AesOperation.EncryptString(Makeseed(seed), value, out _);

                if (key != str)
                {
                    key = str;

                    OnPropertyChanged();
                    CheckIsClear();
                }
            }
        }

        /// <summary>
        /// Gets or sets the API Secret.
        /// </summary>
        [JsonProperty("secret")]
        public string Secret
        {
            get => GetSecret();
            set
            {
                var str = AesOperation.EncryptString(Makeseed(seed), value, out _);

                if (secret != str)
                {
                    secret = str;

                    OnPropertyChanged();
                    CheckIsClear();
                }
            }
        }

        /// <summary>
        /// Gets or sets the API Passphrase.
        /// </summary>
        [JsonProperty("passphrase")]
        public string Passphrase
        {
            get => GetPassphrase();
            set
            {
                var str = AesOperation.EncryptString(Makeseed(seed), value, out _);

                if (passphrase != str)
                {
                    passphrase = str;

                    OnPropertyChanged();
                    CheckIsClear();
                }

            }
        }



        #region ICredentialsProvider Implementation

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

        #endregion

        /// <summary>
        /// Save the credentials to encrypted storage.
        /// </summary>
        /// <param name="pin">The pin with which to save the credentials.</param>
        public void SaveToStorage(string pin = null)
        {
            if (pin == null) pin = CryptoCredentials.pin;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Join(path, "KuCoin.NET");

            Directory.CreateDirectory(path);
            var file = GetCryptName(pin);

            path = Path.Join(path, file);

            var json = JsonConvert.SerializeObject(this);
            var crypted = AesOperation.EncryptString(GetCipherSeed(Seed, pin), json, out _);

            File.WriteAllText(path, crypted);
        }

        /// <summary>
        /// Clear the credentials.
        /// </summary>
        public void Clear()
        {
            SetProperty(ref secret, "", nameof(Secret));
            SetProperty(ref key, "", nameof(Key));
            SetProperty(ref passphrase, "", nameof(Passphrase));
            SetProperty(ref name, "", nameof(Passphrase));

            CheckIsClear();
        }

        /// <summary>
        /// Check if the credentials properties are clear.
        /// </summary>
        protected void CheckIsClear()
        {
            IsClear = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(key) && string.IsNullOrEmpty(passphrase) && string.IsNullOrEmpty(secret);
            OnPropertyChanged(nameof(IsFilled));
        }

        protected static Guid Makeseed(Guid? seed = null)
        {
            var g = seed ?? Guid.NewGuid();

            var b = g.ToByteArray();
            Array.Reverse(b);

            for (int i = 0; i < 16; i++)
            {
                b[i] = (byte)(~b[i] & 0xff);
            }

            return new Guid(b);
        }

      
    }
}
