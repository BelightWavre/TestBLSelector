using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Component
{

    public partial class BLSelector : StackLayout
    {
        #region CornerRadius Property
        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(BLSelector), -1, BindingMode.Default);
        #endregion

        #region Border Size Property
        public int Border
        {
            get => (int)GetValue(BorderProperty);
            set => SetValue(BorderProperty, value);
        }
        public static BindableProperty BorderProperty = BindableProperty.Create(nameof(Border), typeof(int), typeof(BLSelector), 2, BindingMode.Default);
        #endregion

        #region FontSize Property
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(BLSelector), default(double), BindingMode.Default, propertyChanged: OnFontSizePropertyChanged);
        private static void OnFontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
            if (current.Content?.Children == null) return;

            foreach (var lbl in current.Content.Children.OfType<Label>()) lbl.FontSize = (double)newValue;
        }
        #endregion

        #region ItemBackGroundColor Property
        public Color ItemBackgroundColor
        {
            get => (Color)GetValue(ItemBackgroundColorProperty);
            set => SetValue(ItemBackgroundColorProperty, value);
        }
        public static BindableProperty ItemBackgroundColorProperty = BindableProperty.Create(nameof(ItemBackgroundColor), typeof(Color), typeof(BLSelector), Color.Black, BindingMode.Default, propertyChanged: OnItemBackgroundColorPropertyChanged);
        private static void OnItemBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
            if (current.Content?.Children == null) return;

            current.frameBackGround2.BackgroundColor = current.ItemBackgroundColor;
            current.frameBackGround2.BorderColor = current.ItemBackgroundColor;
        }
        #endregion

        #region SelectedItemBackgroundColor Property
        public Color SelectedItemBackgroundColor
        {
            get => (Color)GetValue(SelectedItemBackgroundColorProperty);
            set => SetValue(SelectedItemBackgroundColorProperty, value);
        }
        public static BindableProperty SelectedItemBackgroundColorProperty = BindableProperty.Create(nameof(SelectedItemBackgroundColor), typeof(Color), typeof(BLSelector), Color.Gray, BindingMode.Default, propertyChanged: OnSelectedItemBackgroundColorPropertyChanged);
        private static void OnSelectedItemBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
            if (current.Content?.Children == null) return;
    
            // Set background of the selected boxview to SelectedItemBackgroundColor
            ((BoxView)current.Content.Children[1 + current.Selected]).BackgroundColor = current.SelectedItemBackgroundColor;

        }
        #endregion

        #region TextColor Property
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(BLSelector), Color.White, BindingMode.Default, propertyChanged: OnTextColorPropertyChanged);
        private static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;

            // Check if text color is different from ItemBackgroundcolor
            // To be sure that text is visible
            if (current.TextColor == current.ItemBackgroundColor)
                current.TextColor = (0.2126 * current.ItemBackgroundColor.R + 0.7152 * current.ItemBackgroundColor.G + 0.0722 * current.ItemBackgroundColor.B) > 0.5F ? Color.Black : Color.White;

            if (current.Content?.Children == null) return;
            current.Content.Children.OfType<Label>().ForEach(c => c.TextColor = current.TextColor);
        }
        #endregion

        #region Labels Property
        public static BindableProperty LabelsProperty = BindableProperty.Create(nameof(Labels), typeof(string[]), typeof(BLSelector), null, BindingMode.Default, propertyChanged: OnLabelsPropertyChanged);
        public string[] Labels
        {
            get => (string[])GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }
        private static void OnLabelsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;

            // Check the number of strings
            if (current.Commands != null && current.Labels.Count() != current.Commands.Count())
            {
                Debug.WriteLine("Component::BLSelector - Bad number of Labels");
                throw new TargetParameterCountException();
            }

            // Updates the number of Label Controls
            current.BoxViewControls = new BoxView[current.Labels.Count()];

            // Set up the layout
            current.GridContendUpdate();
        }
        #endregion

        #region Selected Property
        public int Selected
        {
            get => (int)GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }
        public static BindableProperty SelectedProperty = BindableProperty.Create(nameof(Selected), typeof(int), typeof(BLSelector), default(int), BindingMode.Default, propertyChanged: OnSelectedPropertyChanged);
        private static void OnSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
            if (current.Content?.Children == null) return;

            // Set background of all boxview to ItemBackgroundColor
            for (int i = 1; i < 1 + current.BoxViewControls.Count(); i++) ((BoxView)current.Content.Children[i]).BackgroundColor = Color.Transparent;

            // Set background of the selected boxview to SelectedItemBackgroundColor
            ((BoxView)current.Content.Children[1 + current.Selected]).BackgroundColor = current.SelectedItemBackgroundColor;

            // Execute corresponding Action
            if (current.Commands != null) current.Commands[current.Selected]?.Execute(null);
        }
        #endregion

        #region Commmands Property
        public static BindableProperty CommandsProperty = BindableProperty.Create(nameof(Commands), typeof(ICommand[]), typeof(BLSelector), null, BindingMode.Default, propertyChanged: OnCommandsPropertyChanged);
            public ICommand[] Commands
            {
                get => (ICommand[])GetValue(CommandsProperty);
                set => SetValue(CommandsProperty, value);
            }
            private static void OnCommandsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            {
                var current = bindable as BLSelector;

                // Check the number of Commands
                if (current.Labels != null && current.Labels.Count() != current.Commands.Count())
                {
                    Debug.WriteLine("Component::BLSelector - Bad number of commands");
                    throw new TargetParameterCountException();
                }
            }
        #endregion

        private BoxView[] BoxViewControls;
        private Grid      Content;
        private Frame     frameBackGround1, frameBackGround2;


        public  void    OnItemTapped(object obj)
        {
            Selected = BoxViewControls.IndexOf(obj);
        }

        public  void    GridContendUpdate()
        {
            Content = new Grid { Padding = 0, Margin = 0, ColumnSpacing = 0, RowSpacing = 0 };

            // Set up rows and columns of the grid
            Content.RowDefinitions.Add(new RowDefinition { Height = HeightRequest == -1 ? new GridLength(1, GridUnitType.Auto)
                                                                                        : new GridLength(HeightRequest, GridUnitType.Absolute)
            });
            Labels.ForEach(s => {
                // Add a column for each Label
                Content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            });

            // Add Background - ChildIndex = 0 & 1
            frameBackGround1 = new Frame {
                BackgroundColor = SelectedItemBackgroundColor,
                BorderColor = SelectedItemBackgroundColor,
                Padding = Border,
                CornerRadius = this.CornerRadius != -1 ? this.CornerRadius : 0,
            };
            frameBackGround1.PropertyChanged += OnFramePropertyChanged;
            Content.Children.Add(frameBackGround1, 0, Labels.Count(), 0, 1);

            frameBackGround2 = new Frame {
                BackgroundColor = ItemBackgroundColor,
                BorderColor = ItemBackgroundColor,
                CornerRadius = this.CornerRadius != -1 ? (this.CornerRadius > Border ? this.CornerRadius - Border : this.CornerRadius/2)  :  0
            };
            frameBackGround2.PropertyChanged += OnFramePropertyChanged;
            frameBackGround1.Content = frameBackGround2;


            // Add Background for selected item  - ChildIndew = 1 ... 1+N
            for (int i = 0; i < BoxViewControls.Count(); i++)
            {
                BoxViewControls[i] = new BoxView
                {
                    BackgroundColor = Color.Transparent,
                    CornerRadius = this.CornerRadius != -1 ? this.CornerRadius : 0
                };
                if (WidthRequest != -1) BoxViewControls[i].WidthRequest = WidthRequest;
                BoxViewControls[i].PropertyChanged += OnBoxPropertyChanged;

                // Create the Tapped Command for this boxview
                var tgr = new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 1,
                    Command = new Command(OnItemTapped),
                    CommandParameter = BoxViewControls[i]
                };
                BoxViewControls[i].GestureRecognizers.Add(tgr);
                Content.Children.Add(BoxViewControls[i], i, 0);
            }


            // Add Labels
            for (int i = 0; i < BoxViewControls.Count(); i++)
            {
                // Creates the Label
                Label label = new Label {
                    Text = Labels[i],
                    Padding = 0,
                    Margin = this.Margin,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = TextColor,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = (double)GetValue(FontSizeProperty),
                    InputTransparent = true
                };

                Content.Children.Add(label, i, 0);
            }

            // Apply the SelectedItemBackgroundColor to the initially selected boxview
            ((BoxView)Content.Children[1 + Selected]).BackgroundColor = SelectedItemBackgroundColor;

            // If the control was already set-up (we are on an update) then remove the actual content to replace/update it
            if (Children.Count() > 0) Children.RemoveAt(0);
            
            // Add the (new/updated) content
            Children.Add(Content);
        }

        private void OnBoxPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            BoxView box = sender as BoxView;

            if (e.PropertyName == nameof(Height))
            {
                if (this.CornerRadius == -1)
                    box.CornerRadius = box.Height / 2;
            }
        }

        private void OnFramePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Frame frame = sender as Frame;

            if (e.PropertyName == nameof(Height))
            {
                if (this.CornerRadius == -1)
                    frame.CornerRadius = (float)frame.Height / 2;
            }
        }

    }

    public class HalvedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2.0F;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Trace.WriteLine("HalvedConverter - Only one way bindings are supported with this converter");
            throw new NotSupportedException("HalvedConverter - Only one way bindings are supported with this converter");
        }
    }

}
