using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuCoin.NET.Rest;
using System.Runtime.CompilerServices;

namespace CredentialTool
{
    /// <summary>
    /// Secure storage for cryptographic credentials.
    /// </summary>
    public class CryptoCredentials : MemoryEncryptedCredentialsProvider, ICloneable
    {

        /// <summary>
        /// Gets or sets the name of the program for the storage of credentials.
        /// </summary>
        /// <remarks>
        /// Changing this changes the local application data path.
        /// </remarks>
        public static string ProgramName { get; set; } = "KuCoin.NET";

        /// <summary>
        /// Gets the global credential name-encoding pin.
        /// </summary>
        /// <remarks>
        /// Changing this will change how the names of encrypted files are generated.
        /// </remarks>
        public static string GlobalPin { get; set; } = "999999";


        public const int DefaultVersion = 2;

        protected static string pin;

        protected string name;

        protected MemoryEncryptedCredentialsProvider cred;

        protected bool isClear = true;

        protected bool isPassphraseRequired = true;

        protected int version = DefaultVersion;

        protected CryptoCredentials parent;

        public static Guid AppSeed { get; set; }

        internal CryptoCredentials() : base(null, null, null)
        {
        }

        internal CryptoCredentials(CryptoCredentials parent) : base(null, null, null)
        {
            this.parent = parent;
        }

        [JsonIgnore]
        public CryptoCredentials Parent
        {
            get => parent;
            internal set
            {
                SetProperty(ref parent, value);
            }
        }


        /// <summary>
        /// Gets or sets the working Pin.
        /// </summary>
        /// <remarks>
        /// This property is static to enable the user not having to enter their pin unnecessarily.
        /// </remarks>
        public static string Pin
        {
            get => string.IsNullOrEmpty(pin) ? pin : AesOperation.DecryptString(AppSeed, pin);
            set
            {
                if (ValidatePin(value))
                {
                    pin = AesOperation.EncryptString(AppSeed, value, out _);
                }
            }
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
        public virtual bool IsFilled
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
        public virtual Guid Seed
        {
            get => MakeSeed(seed);
            protected set
            {
                var newSeed = MakeSeed(value);

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
        public virtual bool IsClear
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
        public virtual bool IsPassphraseRequired
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
        public virtual string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value)) CheckIsClear();
            }
        }

        [JsonProperty("version")]
        public virtual int Version
        {
            get => version;
            internal set
            {
                SetProperty(ref version, value);
            }
        }

        [JsonProperty("sandBox")]
        public virtual bool Sandbox
        {
            get => sandbox;
            set
            {
                SetProperty(ref sandbox, value);
            }
        }

        [JsonProperty("futures")]
        public virtual bool Futures
        {
            get => futures;
            set
            {
                SetProperty(ref futures, value);
            }
        }



        /// <summary>
        /// Gets or sets the API Key.
        /// </summary>
        [JsonProperty("key")]
        public virtual string Key
        {
            get => GetKey();
            set
            {
                string str;

                if (version == 1)
                {
                    str = AesOperation.EncryptString(MakeSeed(seed), value, out _);
                }
                else
                {
                    str = EncryptIt(value);
                }

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
        public virtual string Secret
        {
            get => GetSecret();
            set
            {
                string str;

                if (version == 1)
                {
                    str = AesOperation.EncryptString(MakeSeed(seed), value, out _);
                }
                else
                {
                    str = EncryptIt(value);
                }

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
        public virtual string Passphrase
        {
            get => GetPassphrase();
            set
            {
                string str;

                if (version == 1)
                {
                    str = AesOperation.EncryptString(MakeSeed(seed), value, out _);
                }
                else
                {
                    str = EncryptIt(value);
                }

                if (passphrase != str)
                {
                    passphrase = str;

                    OnPropertyChanged();
                    CheckIsClear();
                }

            }
        }

        [JsonProperty("attachedAccount")]
        [JsonConverter(typeof(AttachedAccountConverter))]
        public override ICredentialsProvider AttachedAccount
        {
            get
            {
                if (attachedAccount == null && !futures)
                {
                    var c = new CryptoCredentials(this);

                    c.Sandbox = sandbox;
                    c.Futures = true;

                    attachedAccount = c;
                }

                return attachedAccount;
            }
            set
            {
                if (futures && value != null) throw new InvalidOperationException("Futures account cannot have attached account.");
                SetProperty(ref attachedAccount, value);
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
            if (pin == null) pin = CryptoCredentials.Pin;

            if (!ValidatePin(pin)) return;
            if (Pin == null) Pin = pin;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, ProgramName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var file = GetCryptName(pin);

            path = Path.Combine(path, file);

            if (!CheckExists(path, out int vers))
            {
                return;
            }
            else if (vers > 1)
            {
                path += $".{vers}";
            }

            File.Decrypt(path);
            File.Delete(path);

        }

        /// <summary>
        /// Loads API Key information from encrypted storage.
        /// </summary>
        /// <param name="seed">The seed to use for encrypting.</param>
        /// <param name="pin">The pin to use to decrypt the storage.</param>
        /// <param name="create">True to create a new credentials file that does not exist.</param>
        /// <returns></returns>
        public static CryptoCredentials LoadFromStorage(Guid seed, string pin = null, bool create = true, bool autoUpgrade = true)
        {
            if (pin == null) pin = CryptoCredentials.Pin;

            if (!ValidatePin(pin)) return null;
            if (Pin == null) Pin = pin;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cred = new CryptoCredentials();
            int vers;

            path = Path.Combine(path, ProgramName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var file = GetCryptName(pin);

            path = Path.Combine(path, file);

            if (!CheckExists(path, out vers))
            {
                if (create)
                {
                    cred.version = DefaultVersion;
                    cred.Seed = GetCipherSeed(seed, pin);
                    return cred;
                }
                else
                {
                    return null;
                }
            }
            else if (vers > 1)
            {
                path += $".{vers}";
            }
            var crypted = File.ReadAllText(path);

            try
            {
                cred.version = vers;

                string json = null;

                if (vers == 1)
                {
                    cred.Seed = seed;
                    json = AesOperation.DecryptString(GetCipherSeed(seed, pin), crypted);

                }
                else if (vers >= 2)
                {
                    cred.Seed = GetCipherSeed(seed, pin);
                    json = cred.DecryptIt(crypted);
                }

                var jcfg = new JsonSerializerSettings()
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                };

                JsonConvert.PopulateObject(json, cred, jcfg);

                if (vers == 1 && autoUpgrade)
                {
                    vers = DefaultVersion;
                    var oldpath = path;
                    path += $".{vers}";

                    cred.Seed = GetCipherSeed(seed, pin);
                    cred.Version = DefaultVersion;

                    JsonConvert.PopulateObject(json, cred);
                    json = JsonConvert.SerializeObject(cred);

                    crypted = cred.EncryptIt(json);

                    if (File.Exists(path)) File.Delete(path);

                    File.WriteAllText(path, crypted);

                    try
                    {
                        File.Encrypt(path);

                    }
                    catch
                    {

                    }

                    try
                    {
                        File.Delete(oldpath);
                    }
                    catch
                    {

                    }
                }



#pragma warning disable IDE0059 // Unnecessary assignment of a value
                json = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            cred.CheckIsClear();
            Pin = pin;

            GC.Collect();
            cred.futures = false;

            if (cred.attachedAccount != null && cred.attachedAccount is CryptoCredentials cr)
            {
                cr.Parent = cred;
            }

            return cred;
        }

        public static Guid GetHvcyp(Guid? newval = null)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, ProgramName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var file = GetCryptName(GlobalPin, true);

            path = Path.Combine(path, file);
            Guid g;

            if (newval != null && File.Exists(path)) File.Delete(path);

            if (File.Exists(path))
            {
                g = Guid.Parse(File.ReadAllText(path));

                return g;
            }
            else
            {
                g = newval ?? Guid.NewGuid();

                File.WriteAllText(path, g.ToString("d"));
                File.Encrypt(path);

                return g;
            }
        }

        /// <summary>
        /// Generate a filename for storing the encrypted data using the provided pin.
        /// </summary>
        /// <param name="pin">The pin to use.</param>
        /// <returns></returns>
        protected static string GetCryptName(string pin, bool userKey = false)
        {
            Guid seedPath;

            if (userKey)
            {
                seedPath = new Guid(Encoding.UTF8.GetBytes(pin + "0e!._aa_t0"));
            }
            else
            {
                seedPath = new Guid(Encoding.UTF8.GetBytes(pin + "0e!._tt_t0"));
            }

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

        protected static bool CheckExists(string filename, out int version)
        {
            for (int i = DefaultVersion; i >= 2; i--)
            {
                if (File.Exists(filename + $".{i}"))
                {
                    version = i;
                    return true;
                }
            }

            if (File.Exists(filename))
            {
                version = 1;
                return true;
            }

            version = DefaultVersion;
            return false;
        }

        /// <summary>
        /// Save the credentials to encrypted storage.
        /// </summary>
        /// <param name="pin">The pin with which to save the credentials.</param>
        public virtual void SaveToStorage(string pin = null)
        {
            if (pin == null) pin = CryptoCredentials.Pin;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, ProgramName);

            Directory.CreateDirectory(path);
            var file = GetCryptName(pin);

            path = Path.Combine(path, file);

            var json = JsonConvert.SerializeObject(this);
            string crypted = null;

            if (version == 1)
            {
                crypted = AesOperation.EncryptString(GetCipherSeed(Seed, pin), json, out _);
            }
            else if (version >= 2)
            {
                path += $".{version}";
                crypted = EncryptIt(json);
            }

            if (File.Exists(path)) File.Delete(path);

            File.WriteAllText(path, crypted);

            if (version >= 2)
                File.Encrypt(path);
        }

        /// <summary>
        /// Clear the credentials.
        /// </summary>
        public virtual void Clear()
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
        protected virtual void CheckIsClear()
        {
            IsClear = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(key) && string.IsNullOrEmpty(passphrase) && string.IsNullOrEmpty(secret);
            OnPropertyChanged(nameof(IsFilled));
        }

        protected virtual Guid MakeSeed(Guid? seed = null)
        {
            if (version == 1)
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
            else
            {
                return MakeZipper(seed);
            }

        }


        #region ICredentialsProvider Implementation

        public override string GetKey()
        {
            if (version == 1)
            {
                return !string.IsNullOrEmpty(key) ? AesOperation.DecryptString(MakeSeed(seed), key) : null;
            }
            else
            {
                return base.GetKey();
            }
        }

        public override string GetPassphrase()
        {
            if (version == 1)
            {
                return !string.IsNullOrEmpty(passphrase) ? AesOperation.DecryptString(MakeSeed(seed), passphrase) : null;
            }
            else
            {
                return base.GetPassphrase();
            }
        }

        public override string GetSecret()
        {
            if (version == 1)
            {
                return !string.IsNullOrEmpty(secret) ? AesOperation.DecryptString(MakeSeed(seed), secret) : null;
            }
            else
            {
                return base.GetSecret();
            }
        }

        #endregion

        #region ICloneable

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public virtual CryptoCredentials Clone()
        {
            return (CryptoCredentials)MemberwiseClone();
        }

        #endregion

    }

}
