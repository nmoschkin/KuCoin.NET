using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public delegate void PinStatChangeEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Interaction logic for PinControl.xaml
    /// </summary>
    public partial class PinControl : UserControl
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for CharacterMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CharacterMarginProperty =
            DependencyProperty.Register("CharacterMargin", typeof(Thickness), typeof(PinControl), new PropertyMetadata(new Thickness(4), FormatPropertyChanged));

        // Using a DependencyProperty as the backing store for PasswordChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register("PasswordChar", typeof(string), typeof(PinControl), new PropertyMetadata("•", OnPasswordCharChanged));

        // Using a DependencyProperty as the backing store for PinSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PinSizeProperty =
            DependencyProperty.Register("PinSize", typeof(int), typeof(PinControl), new PropertyMetadata(6, FormatPropertyChanged));

        // Using a DependencyProperty as the backing store for TextSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSourceProperty =
            DependencyProperty.Register("TextSource", typeof(TextSource), typeof(PinControl), new PropertyMetadata(new TextSource(), OnTextSourceChange));

        // Using a DependencyProperty as the backing store for UsePasswordChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsePasswordCharProperty =
            DependencyProperty.Register("UsePasswordChar", typeof(bool), typeof(PinControl), new PropertyMetadata(true, OnUsePasswordCharChanged));

        #endregion Public Fields

        #region Private Fields

        private readonly List<BigCharacter> bigChars = new List<BigCharacter>();

        private bool isValid = false;

        #endregion Private Fields

        #region Public Constructors

        public PinControl()
        {
            TextSource = new TextSource();
            InitializeComponent();

            SetupPins();
            SetupPasswordIcon();

            TextSource.CharacterChanged += CharacterChanged;
            ShowHidePwdIcon.Margin = CharacterMargin;
        }

        #endregion Public Constructors

        #region Public Events

        public event PinStatChangeEventHandler PinComplete;
        public event PinStatChangeEventHandler PinInvalidated;
        public event EventHandler ShowHideClick;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the spacing between character cells.
        /// </summary>
        public Thickness CharacterMargin
        {
            get { return (Thickness)GetValue(CharacterMarginProperty); }
            set { SetValue(CharacterMarginProperty, value); }
        }

        /// <summary>
        /// Gets or sets the password character to use in place of visible text.
        /// </summary>
        public string PasswordChar
        {
            get { return (string)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }

        /// <summary>
        /// Gets the pin that has been entered.
        /// </summary>
        public string Pin
        {
            get => TextSource;
        }

        /// <summary>
        /// Gets or sets the number of characters in the pin.
        /// </summary>
        public int PinSize
        {
            get { return (int)GetValue(PinSizeProperty); }
            set { SetValue(PinSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="PinEntry.TextSource"/> object that is used to manage the text.
        /// </summary>
        public TextSource TextSource
        {
            get { return (TextSource)GetValue(TextSourceProperty); }
            set { SetValue(TextSourceProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value indicating that the password character will be used in place of visible text.
        /// </summary>
        public bool UsePasswordChar
        {
            get { return (bool)GetValue(UsePasswordCharProperty); }
            set { SetValue(UsePasswordCharProperty, value); }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            FocusLast();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                Focus();
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private static void FormatPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PinControl ctl)
            {
                ctl.SetupPins();
            }
        }

        private static void OnPasswordCharChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PinControl ctl && e.NewValue != e.OldValue)
            {
                foreach (var bc in ctl.bigChars)
                {
                    bc.PasswordChar = (string)e.NewValue;
                }
            }
        }

        private static void OnTextSourceChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PinControl ctl && e.NewValue != e.OldValue)
            {
                ctl.SetupChangeEvents(e.OldValue, e.NewValue);
            }
        }

        private static void OnUsePasswordCharChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PinControl ctl && e.NewValue != e.OldValue)
            {
                var show = (bool)e.NewValue;

                foreach (var bc in ctl.bigChars)
                {
                    bc.UsePasswordChar = show;
                }

                ctl.SetupPasswordIcon();
            }
        }

        private void Bc_Click(object sender, EventArgs e)
        {
            //if (sender is BigCharacter bc)
            //{
            //    bc.Focus();
            //}
        }

        private void CharacterChanged(object sender, CharacterChangedEventArgs e)
        {
            bool newValid = TextSource.ValidateText();

            if (newValid)
            {
                PinComplete?.Invoke(this, new EventArgs());
            }
            else if (!newValid)
            {
                PinInvalidated?.Invoke(this, new EventArgs());
            }

            isValid = newValid;
        }

        private void FocusLast()
        {

            for (int i = 0; i < PinSize; i++)
            {
                if (bigChars[i].Character == " ")
                {
                    bigChars[i].Focus();
                    return;
                }

            }

            bigChars.LastOrDefault().Focus();
        }

        private void GoBackward(object sender, GoEventArgs e)
        {
            if (sender is BigCharacter bc)
            {
                var idx = bigChars.IndexOf(bc);
                if (idx >= 0)
                {
                    if (idx == 0)
                    {
                        if (e.GoAround) bigChars[PinSize - 1].Focus();
                    }
                    else
                    {
                        bigChars[idx - 1].Focus();
                    }
                }
            }
        }

        private void GoForward(object sender, GoEventArgs e)
        {
            if (sender is BigCharacter bc)
            {
                var idx = bigChars.IndexOf(bc);
                if (idx >= 0)
                {
                    if (idx >= PinSize - 1)
                    {
                        if (e.GoAround) bigChars[0].Focus();
                    }
                    else
                    {
                        bigChars[idx + 1].Focus();
                    }
                }
            }

        }

        private void SetupChangeEvents(object oldValue, object newValue)
        {
            if (oldValue is TextSource tsOld)
            {
                tsOld.CharacterChanged -= CharacterChanged;
            }
            if (newValue is TextSource tsNew)
            {
                tsNew.CharacterChanged += CharacterChanged;
            }

        }
        private void SetupPasswordIcon()
        {

            BitmapSource img;

            byte[] b = UsePasswordChar ? AppResources.Hide_Password : AppResources.Show_Password;
            var stream = new MemoryStream(b);

            if (stream != null)
            {
                img = BitmapFrame.Create(stream,
                                    BitmapCreateOptions.None,
                                    BitmapCacheOption.OnLoad);

                ShowHidePwdIcon.Source = img;
            }
        }
        
        private void SetupPins()
        {
            if (bigChars.Count != 0)
            {
                foreach (var oldBc in bigChars)
                {
                    oldBc.GoForward -= GoForward;
                    oldBc.GoBackward -= GoBackward;
                }
            }

            PinGrid.Children.Clear();

            PinGrid.ColumnDefinitions.Clear();
            bigChars.Clear();

            int c = PinSize;

            ShowHidePwdIcon.Margin = CharacterMargin;

            for (int i = 0; i < c; i++)
            {
                var cd = new ColumnDefinition();
                PinGrid.ColumnDefinitions.Add(cd);

                var bc = new BigCharacter();
                bigChars.Add(bc);
                PinGrid.Children.Add(bc);

                bc.TextSource = TextSource;
                bc.TextSourceIndex = i;

                bc.Margin = CharacterMargin;

                bc.GoForward += GoForward;
                bc.GoBackward += GoBackward;
                bc.Click += Bc_Click;
                bc.UsePasswordChar = UsePasswordChar;
                bc.PasswordChar = PasswordChar;

                bc.SetBinding(BigCharacter.FontSizeProperty, new Binding("FontSize"));
                bc.SetBinding(BigCharacter.FontWeightProperty, new Binding("FontWeight"));
                bc.SetBinding(BigCharacter.FontStretchProperty, new Binding("FontStretch"));
                bc.SetBinding(BigCharacter.FontStyleProperty, new Binding("FontStyle"));
                bc.SetBinding(BigCharacter.FontFamilyProperty, new Binding("FontFamily"));

                bc.DataContext = this;

                bc.SetValue(Grid.ColumnProperty, i);
            }

            FocusLast();
        }
        private void ShowHidePwdIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                UsePasswordChar = !UsePasswordChar;
                ShowHideClick?.Invoke(this, new EventArgs());
            }
        }

        #endregion Private Methods
    }
}
