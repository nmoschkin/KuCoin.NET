using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using DataTools.PinEntry.Observable;

namespace DataTools.PinEntry
{
    /// <summary>
    /// CharacterChanged event arguments.
    /// </summary>
    public class CharacterChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new character.
        /// </summary>
        public char NewChar { get; private set; }

        /// <summary>
        /// The old character (may be null).
        /// </summary>
        public char? OldChar { get; private set; }

        /// <summary>
        /// The index of the character.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The containing string.
        /// </summary>
        public string Text { get; private set; }

        public CharacterChangedEventArgs(string s, int index, char? oldChar = null)
        {
            Index = index;
            Text = s;
            NewChar = s[index];
            OldChar = oldChar;
        }

    }

    /// <summary>
    /// CharacterChanged event handler delegate.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CharacterChangedEventHandler(object sender, CharacterChangedEventArgs e);
    
    /// <summary>
    /// Text validation flags
    /// </summary>
    [Flags]
    public enum TextValidationFlags
    {
        /// <summary>
        /// Allow any character
        /// </summary>
        AllowAll = 0,

        /// <summary>
        /// Allow whitespace characters
        /// </summary>
        AllowWhitespace = 1,

        /// <summary>
        /// Allow letters
        /// </summary>
        AllowLetters = 2,

        /// <summary>
        /// Allow digits
        /// </summary>
        AllowDigits = 4,

        /// <summary>
        /// Allow other characters
        /// </summary>
        AllowOtherChars = 8
    }

    /// <summary>
    /// Represents a source of text for individual controls that have access to one character, each.
    /// </summary>
    public class TextSource : ObservableBase
    {
        private Guid key = Guid.NewGuid();

        /// <summary>
        /// Event that is fired when a character is changed via the default indexer.
        /// </summary>
        public event CharacterChangedEventHandler CharacterChanged;

        protected string text;

        protected int length = 6;

        protected TextValidationFlags flags = TextValidationFlags.AllowAll;

        /// <summary>
        /// Instantiate a new empty text source with a default length of 6 characters.
        /// </summary>
        /// <param name="flags">The default validation criteria flags.</param>
        /// <remarks>
        /// The default value for <see cref="flags"/> is <see cref="TextValidationFlags.AllowDigits"/>.
        /// </remarks>
        public TextSource(TextValidationFlags flags = TextValidationFlags.AllowDigits)
        {
            if (flags is TextValidationFlags f)
            {
                this.flags = f;
            }

            text = "";
            CheckTextLength();
        }

        /// <summary>
        /// Instantiate a new TextSource with the specified length.
        /// </summary>
        /// <param name="length">The length of the default string.</param>
        /// <param name="flags">The default validation criteria flags.</param>
        /// <remarks>
        /// The default value for <see cref="flags"/> is <see cref="TextValidationFlags.AllowDigits"/>.
        /// </remarks>
        public TextSource(int length, TextValidationFlags flags = TextValidationFlags.AllowDigits)
        {
            if (flags is TextValidationFlags f)
            {
                this.flags = f;
            }

            this.length = length;
            CheckTextLength();
        }

        /// <summary>
        /// Instantiate a new TextSource from a string.
        /// </summary>
        /// <param name="text">The initial string contents.</param>
        /// <param name="flags">The default validation criteria flags.</param>
        /// <remarks>
        /// The default value for <see cref="flags"/> is <see cref="TextValidationFlags.AllowDigits"/>.
        /// </remarks>
        public TextSource(string text, TextValidationFlags flags = TextValidationFlags.AllowDigits)
        {
            if (flags is TextValidationFlags f)
            {
                this.flags = f;
            }

            length = text.Length;
            this.text = AesOperation.EncryptString(key, text, out _);
        
            CheckTextLength();
        }


        /// <summary>
        /// Gets or sets the default validation criteria flags.
        /// </summary>
        public TextValidationFlags ValidationFlags
        {
            get => flags;
            set
            {
                SetProperty(ref flags, value);
            }
        }

        /// <summary>
        /// Gets or sets the character at the specified index.
        /// </summary>
        /// <param name="index">The position of the character.</param>
        /// <returns>
        /// If the index is >= length of the string, then the string will be automatically extended to that point.
        /// </returns>
        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= length) throw new ArgumentOutOfRangeException();
                return Text[index];
            }
            set
            {
                if (index < 0) throw new ArgumentOutOfRangeException();

                if (index >= length)
                {
                    length = index + 1;
                
                    if (value == ' ')
                    {
                        CheckTextLength();
                        return;
                    }
                    else
                    {
                        CheckTextLength(true);
                    }
                }

                var chars = AesOperation.DecryptString(key, text).ToCharArray();

                if (chars[index] != value)
                {
                    var oldChar = chars[index];
                    chars[index] = value;
                    var output = new string(chars);
                    text = AesOperation.EncryptString(key, output, out _);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CharacterChanged?.Invoke(this, new CharacterChangedEventArgs(output, index, oldChar));
                        OnPropertyChanged(nameof(Text));
                    });
                }
            }

        }

        /// <summary>
        /// Returns true if the text is a decimal number.
        /// </summary>
        /// <returns></returns>
        public bool IsValidNumber() => ValidateText(TextValidationFlags.AllowDigits);

        /// <summary>
        /// Validate the text in the buffer.
        /// </summary>
        /// <param name="flags">The validation criteria flags.</param>
        /// <returns>True if the text in the buffer is valid.</returns>
        public bool ValidateText(TextValidationFlags? flags = null)
        {
            if (flags == null) flags = this.flags;

            if (flags == TextValidationFlags.AllowAll) return true;

            var chars = Text.ToCharArray();

            foreach (char ch in chars)
            {
                if (!ValidateChar(ch, flags)) return false;
            }

            return true;
        }

        /// <summary>
        /// Validate a single character based on the specified criteria.
        /// </summary>
        /// <param name="ch">The character to validate.</param>
        /// <param name="flags">The validation criteria flags.</param>
        /// <returns>True if the character is valid.</returns>
        public bool ValidateChar(char ch, TextValidationFlags? flags = null)
        {
            if (flags == null) flags = this.flags;

            if (flags == TextValidationFlags.AllowAll) return true;

            var f = (TextValidationFlags)flags;

            if ((f & TextValidationFlags.AllowWhitespace) == 0 && char.IsWhiteSpace(ch)) return false;
            else if ((f & TextValidationFlags.AllowLetters) == 0 && char.IsLetter(ch)) return false;
            else if ((f & TextValidationFlags.AllowDigits) == 0 && char.IsDigit(ch)) return false;
            else if ((f & TextValidationFlags.AllowOtherChars) == 0 && !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch)) return false;
            else return true;
        }

        /// <summary>
        /// Gets or sets the fixed length of the string.
        /// </summary>
        public int Length
        {
            get => length;
            set
            {
                if (SetProperty(ref length, value))
                {
                    CheckTextLength();
                }
            }
        }

        /// <summary>
        /// Check that the text length is exactly a long as the <see cref="Length"/> property specifies.
        /// </summary>
        /// <param name="suppressEvent">True to suppress raising any <see cref="INotifyPropertyChanged.PropertyChanged"/> events.</param>
        /// <returns>The length of the text.</returns>
        protected int CheckTextLength(bool suppressEvent = false)
        {
            string decrypt;

            if (string.IsNullOrEmpty(text))
            {
                text = AesOperation.EncryptString(key, new string(' ', length), out _);
                if (!suppressEvent) OnPropertyChanged(nameof(Text));
            }
            else if (Text.Length > length)
            {
                decrypt = AesOperation.DecryptString(key, text);               
                decrypt = decrypt.Substring(0, length);
                text = AesOperation.EncryptString(key, decrypt, out _);

                if (!suppressEvent) OnPropertyChanged(nameof(Text));
            }
            else if (Text.Length < length)
            {
                decrypt = AesOperation.DecryptString(key, text);
                decrypt = decrypt + new string(' ', length - decrypt.Length);
                text = AesOperation.EncryptString(key, decrypt, out _);

                if (!suppressEvent) OnPropertyChanged(nameof(Text));
            }

            return length;
        }

        /// <summary>
        /// Gets or sets the text value of this object.
        /// </summary>
        /// <remarks>
        /// Setting this property will adjust the fixed text length of the object, automatically.
        /// </remarks>
        public string Text
        {
            get => string.IsNullOrEmpty(text) ? text : AesOperation.DecryptString(key, text);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (SetProperty(ref text, value))
                    {
                        if (length != 0) Length = 0;
                    }                   
                }
                else
                {
                    var crypt = AesOperation.EncryptString(key, value, out _);

                    if (SetProperty(ref text, crypt))
                    {
                        if (length != value.Length)
                        {
                            Length = value.Length;
                        }
                    }
                }
            }
        }

        #region object overrides

        public override bool Equals(object obj)
        {
            if (obj is string s)
            {
                return s.Equals(Text);
            }
            else if (obj is TextSource t)
            {
                return Text == t.Text;
            }
            else
            {
                return false;
            }
        }

        public override string ToString() => Text;

        public override int GetHashCode()
        {
            return Text?.GetHashCode() ?? 0;
        }

        #endregion

        #region Implicit string cast operators

        public static implicit operator string(TextSource t)
        {
            return t.Text;
        }

        public static implicit operator TextSource(string s)
        {
            return new TextSource(s);
        }

        #endregion

        #region Equality comparison operators for TextSource and string

        public static bool operator ==(TextSource s1, TextSource s2)
        {
            if (s1 is object && s2 is object)
            {
                return s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool operator !=(TextSource s1, TextSource s2)
        {
            if (s1 is object && s2 is object)
            {
                return !s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static bool operator ==(string s1, TextSource s2)
        {
            if (s1 is object && s2 is object)
            {
                return s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool operator !=(string s1, TextSource s2)
        {
            if (s1 is object && s2 is object)
            {
                return !s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static bool operator ==(TextSource s1, string s2)
        {
            if (s1 is object && s2 is object)
            {
                return s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool operator !=(TextSource s1, string s2)
        {
            if (s1 is object && s2 is object)
            {
                return !s1.Equals(s2);
            }
            else if (s1 is object || s2 is object)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion



    }
}
