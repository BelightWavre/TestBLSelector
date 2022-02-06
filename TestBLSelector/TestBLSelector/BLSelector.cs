using System;
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
        public static BindableProperty ItemBackgroundColorProperty = BindableProperty.Create(nameof(ItemBackgroundColor), typeof(Color), typeof(BLSelector), default(Color), BindingMode.Default, propertyChanged: OnItemBackgroundColorPropertyChanged);
        private static void OnItemBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
            if (current.Content?.Children == null) return;

            // Set background of all boxview to ItemBackgroundColor
            foreach (var box in current.Content.Children.OfType<BoxView>()) box.BackgroundColor = current.ItemBackgroundColor;

            // Set background of the selected boxview to SelectedItemBackgroundColor
            if (current.SelectedItemBackgroundColor == null) return;
            ((BoxView)current.Content.Children[1 + current.Selected]).BackgroundColor = current.SelectedItemBackgroundColor;
        }
        #endregion

        #region SelectedItemBackgroundColor Property
        public Color SelectedItemBackgroundColor
        {
            get => (Color)GetValue(SelectedItemBackgroundColorProperty);
            set => SetValue(SelectedItemBackgroundColorProperty, value);
        }
        public static BindableProperty SelectedItemBackgroundColorProperty = BindableProperty.Create(nameof(SelectedItemBackgroundColor), typeof(Color), typeof(BLSelector), default(Color), BindingMode.Default, propertyChanged: OnSelectedItemBackgroundColorPropertyChanged);
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
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(BLSelector), default(Color), BindingMode.Default, propertyChanged: OnTextColorPropertyChanged);
        private static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var current = bindable as BLSelector;
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
            for (int i = 1; i < 1 + current.BoxViewControls.Count(); i++) ((BoxView)current.Content.Children[i]).BackgroundColor = current.ItemBackgroundColor;

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
        private BoxView   boxViewBackGround;
        const   int       Duration = 300;

        public  void    OnItemTapped(object obj)
        {
            Selected = BoxViewControls.IndexOf(obj);
        }

        public  void    GridContendUpdate()
        {
            Content = new Grid { Padding = 0, Margin = 0, ColumnSpacing = 0, RowSpacing = 0 };

            // Set up rows and columns of the grid
            Content.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(1, GridUnitType.Auto) });
            Labels.ForEach(s => {
                // Add a column for each Label
                Content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            });

            // Add Background - ChildIndex = 0
            boxViewBackGround = new BoxView
            {
                BackgroundColor = ItemBackgroundColor,
                CornerRadius = 10
            };
            boxViewBackGround.BindingContext = this;
            //DO NOT WORK - ATTEMPT 1
            //boxViewBackGround.SetBinding(BoxView.CornerRadiusProperty, new Binding("Height", BindingMode.Default, new HalvedConverter()));
            Content.Children.Add(boxViewBackGround, 0, Labels.Count(), 0, 1);

            // Add Background for selected item  - ChildIndew = 1 ... 1+N
            for (int i = 0; i < BoxViewControls.Count(); i++)
            {
                var boxView = new BoxView
                {
                    BackgroundColor = ItemBackgroundColor,
                    CornerRadius = 10
                };
                boxView.BindingContext = this;
                //DO NOT WORK - ATTEMPT 1
                //boxView.SetBinding(BoxView.CornerRadiusProperty, new Binding("Height", BindingMode.Default, new HalvedConverter()));

                BoxViewControls[i] = boxView;   // Memorises the control associated to the label

                // Create the Tapped Command for this boxview
                var tgr = new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 1,
                    Command = new Command(OnItemTapped),
                    CommandParameter = boxView
                };
                boxView.GestureRecognizers.Add(tgr);
                Content.Children.Add(boxView, i, 0);
            }

            // Apply the SelectedItemBackgroundColor to the initially selected boxview
            ((BoxView)Content.Children[1 + Selected]).BackgroundColor = SelectedItemBackgroundColor;

            Labels.ForEach(s => {

                // Creates the Label
                Label lbl = new Label { Text = s, Padding = 0, Margin = this.Margin, HorizontalTextAlignment = TextAlignment.Center, TextColor = TextColor,
                                        FontAttributes = FontAttributes.Bold, FontSize = (double)GetValue(FontSizeProperty),
                                        VerticalOptions = LayoutOptions.Center, InputTransparent = true };

                Content.Children.Add(lbl, Labels.IndexOf(s), 0);
            });

            // If the control was already set-up (we are on an update) then remove the actual content to replace/update it
            if (Children.Count() > 0) Children.RemoveAt(0);
            
            // Add the (new/updated) content
            Children.Add(Content);
        }

        //DO NOT WORK - ATTEMPT 2
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Height))
            {
                foreach (var box in Content.Children.OfType<BoxView>())
                {
                    box.CornerRadius = this.Height / 2;
                }
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
