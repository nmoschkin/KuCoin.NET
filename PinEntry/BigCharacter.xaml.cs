using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataTools.PinEntry
{
    /// <summary>
    /// Delegate for the GoForward and GoBackward events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void KeyProcessEventHandler(object sender, GoEventArgs e);

    /// <summary>
    /// Represents a single large character in a cell.
    /// </summary>
    public partial class BigCharacter : UserControl
    {

        #region Public Fields

        // Using a DependencyProperty as the backing store for AllowAlphaChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowAlphaCharProperty =
            DependencyProperty.Register("AllowAlphaChar", typeof(bool), typeof(BigCharacter), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for AllowNumericChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowNumericCharProperty =
            DependencyProperty.Register("AllowNumericChar", typeof(bool), typeof(BigCharacter), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for AllowOtherChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowOtherCharProperty =
            DependencyProperty.Register("AllowOtherChar", typeof(bool), typeof(BigCharacter), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for UsePasswordChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsePasswordCharProperty =
            DependencyProperty.Register("UsePasswordChar", typeof(bool), typeof(BigCharacter), new PropertyMetadata(true, OnUsePasswordCharChange));

        #endregion Public Fields

        #region Public Constructors

        public BigCharacter()
        {
            InitializeComponent();
            TextArea.DataContext = this;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler Click;

        /// <summary>
        /// Raised when the parent control should navigate backward in the list of BigCharacters.
        /// </summary>
        public event KeyProcessEventHandler GoBackward;

        /// <summary>
        /// Raised when the parent control should navigate forward in the list of BigCharacters.
        /// </summary>
        public event KeyProcessEventHandler GoForward;

        #endregion Public Events

        #region TextSource

        // Using a DependencyProperty as the backing store for TextSourceIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSourceIndexProperty =
            DependencyProperty.Register("TextSourceIndex", typeof(int?), typeof(BigCharacter), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for TextSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSourceProperty =
            DependencyProperty.Register("TextSource", typeof(TextSource), typeof(BigCharacter), new PropertyMetadata(null, OnTextSourceChanged));

        /// <summary>
        /// Gets or sets the <see cref="PinEntry.TextSource"/>
        /// </summary>
        public TextSource TextSource
        {
            get { return (TextSource)GetValue(TextSourceProperty); }
            set { SetValue(TextSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the position this BigCharacter represents in a <see cref="PinEntry.TextSource"/>.
        /// </summary>
        public int? TextSourceIndex
        {
            get { return (int?)GetValue(TextSourceIndexProperty); }
            set { SetValue(TextSourceIndexProperty, value); }
        }
        private static void OnTextSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BigCharacter ctl)
            {
                if (e.NewValue is TextSource tsNew)
                {
                    if (tsNew.ValidationFlags != TextValidationFlags.AllowAll)
                    {
                        ctl.AllowAlphaChar = (tsNew.ValidationFlags & TextValidationFlags.AllowLetters) != 0;
                        ctl.AllowNumericChar = (tsNew.ValidationFlags & TextValidationFlags.AllowDigits) != 0;
                        ctl.AllowOtherChar = (tsNew.ValidationFlags & TextValidationFlags.AllowOtherChars) != 0;

                        if (ctl.TextSourceIndex is int idx)
                        {
                            if (idx >= tsNew.Length)
                            {
                                tsNew[idx] = ' ';
                            }
                            else
                            {
                                ctl.Character = "" + tsNew[idx];
                            }
                        }
                    }
                }

                ctl.ChangeTextSourceEvent(e.OldValue, e.NewValue);
            }
        }

        private void ChangeTextSourceEvent(object oldObj, object newObj)
        {
            if (oldObj is TextSource tsOld)
            {
                tsOld.CharacterChanged -= TextCharacterChanged;
            }
            if (newObj is TextSource tsNew)
            {
                tsNew.CharacterChanged += TextCharacterChanged;
            }

        }

        private void TextCharacterChanged(object sender, CharacterChangedEventArgs e)
        {
            if (TextSourceIndex is int idx && idx == e.Index)
            {
                if (Character[0] != e.NewChar)
                {
                    Character = new string(new char[] { e.NewChar });
                }
            }
        }

        #endregion

        #region Characters

        // Using a DependencyProperty as the backing store for Character.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(string), typeof(BigCharacter), new PropertyMetadata(" ", OnCharacterChanged));

        // Using a DependencyProperty as the backing store for PasswordChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register("PasswordChar", typeof(string), typeof(BigCharacter), new PropertyMetadata("•", OnPasswordCharChanged));

        /// <summary>
        /// Gets or sets the character that this control contains.
        /// </summary>
        public string Character
        {
            get { return (string)GetValue(CharacterProperty); }
            set { SetValue(CharacterProperty, value); }
        }
        /// <summary>
        /// Gets or sets a character that will be displayed as a password character instead of visible text.
        /// </summary>
        public string PasswordChar
        {
            get { return (string)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }

        private static void OnCharacterChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BigCharacter ctl)
            {
                if (e.NewValue is string s && s.Length > 1)
                {
                    ctl.Character = s.Substring(s.Length - 1, 1);
                }
                else if (e.NewValue != e.OldValue && e.NewValue is string s2 && ctl.TextSource is object && ctl.TextSourceIndex is int idx)
                {
                    if (ctl.TextSource[idx] != s2[0])
                    {
                        ctl.TextSource[idx] = s2[0];
                    }

                    if (ctl.UsePasswordChar && ctl.TextArea.Text != ctl.PasswordChar && s2 != " ")
                    {
                        ctl.TextArea.Text = ctl.PasswordChar;
                    }
                    else if (s2 == " " && ctl.TextArea.Text != " ")
                    {
                        ctl.TextArea.Text = " ";
                    }
                    else if (!ctl.UsePasswordChar && s2 != " " && ctl.TextArea.Text != s2)
                    {
                        ctl.TextArea.Text = s2;
                    }

                    if (s2 != " ")
                    {
                        ctl.OnGoForward();
                    }

                }
            }
        }
        private static void OnPasswordCharChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BigCharacter ctl)
            {
                if (e.NewValue is string s && e.NewValue != e.OldValue)
                {
                    if (!string.IsNullOrEmpty(s) && s.Length > 1)
                    {
                        throw new InvalidDataException("PasswordChar must be exactly one character");
                    }

                    if (ctl.UsePasswordChar && ctl.TextArea.Text != s && ctl.Character != " ")
                    {
                        ctl.TextArea.Text = s;
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating if letters are allowed.
        /// </summary>
        public bool AllowAlphaChar
        {
            get { return (bool)GetValue(AllowAlphaCharProperty); }
            set { SetValue(AllowAlphaCharProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating if numbers are allowed.
        /// </summary>
        public bool AllowNumericChar
        {
            get { return (bool)GetValue(AllowNumericCharProperty); }
            set { SetValue(AllowNumericCharProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether characters besides numbers or letters are allowed.
        /// </summary>
        public bool AllowOtherChar
        {
            get { return (bool)GetValue(AllowOtherCharProperty); }
            set { SetValue(AllowOtherCharProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating that a password character will be used in place of visible text.
        /// </summary>
        public bool UsePasswordChar
        {
            get { return (bool)GetValue(UsePasswordCharProperty); }
            set { SetValue(UsePasswordCharProperty, value); }
        }

        #endregion Public Properties

        #region Protected Methods

        protected void OnClick()
        {
            Click?.Invoke(this, new EventArgs());
        }

        protected void OnGoBackward(bool goAround = false)
        {
            GoBackward?.Invoke(this, new GoEventArgs(goAround));
        }

        protected void OnGoForward(bool goAround = false)
        {
            GoForward?.Invoke(this, new GoEventArgs(goAround));
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            BorderArea.BorderBrush = Brushes.Blue;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Space)
            {
                Character = " ";

                if (e.Key == Key.Back)
                {
                    OnGoBackward();
                }
            }
            else if (e.Key == Key.Up || e.Key == Key.Left)
            {
                OnGoBackward(true);
            }
            else if (e.Key == Key.Down || e.Key == Key.Right)
            {
                OnGoForward(true);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            BorderArea.BorderBrush = Brushes.Black;

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                OnClick();
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            if (TextSource is object)
            {
                if (!TextSource.ValidateChar(e.Text[0])) return;
            }
            else
            {
                if (!AllowAlphaChar && char.IsLetter(e.Text[0])) return;
                if (!AllowNumericChar && char.IsDigit(e.Text[0])) return;
                if (!AllowOtherChar && !char.IsWhiteSpace(e.Text[0]) && !char.IsLetterOrDigit(e.Text[0])) return;
            }

            Character = e.Text;
        }

        #endregion Protected Methods

        #region Private Methods

        private static void OnUsePasswordCharChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BigCharacter ctl && e.NewValue != e.OldValue && e.NewValue is bool b)
            {
                if (ctl.Character == " " || string.IsNullOrEmpty(ctl.Character)) return;

                if (b)
                {
                    ctl.TextArea.Text = ctl.PasswordChar;
                }
                else
                {
                    ctl.TextArea.Text = ctl.Character;
                }
            }
        }

        #endregion Private Methods
    }

    /// <summary>
    /// GoBackward and GoForward EventArgs
    /// </summary>
    public class GoEventArgs : EventArgs
    {
        #region Public Constructors

        public GoEventArgs(bool goAround = false)
        {
            GoAround = goAround;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// True if around-the-world navigation is recommended.
        /// </summary>
        public bool GoAround { get; private set; }

        #endregion Public Properties
    }
}
