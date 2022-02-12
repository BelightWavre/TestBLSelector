﻿using System;
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
        #region Text Margin Property
        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }
        public static BindableProperty ItemMarginProperty = BindableProperty.Create(nameof(ItemMargin), typeof(Thickness), typeof(BLSelector), default(Thickness), BindingMode.Default);
        #endregion

        #region CornerRadius Property
        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, (double)value);
        }
        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(BLSelector), -1.0, BindingMode.Default);
        #endregion

        #region Border Size Property
        public int Border
        {
            get => (int)GetValue(BorderProperty);
            set => SetValue(BorderProperty, value);
        }
        public static BindableProperty BorderProperty = BindableProperty.Create(nameof(Border), typeof(int), typeof(BLSelector), 3, BindingMode.Default);
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
            get {
                // Check if text color is different from ItemBackgroundcolor
                // To be sure that text is visible
                var color = (Color)GetValue(TextColorProperty);
                var ItemBackgroundColor = (Color)GetValue(ItemBackgroundColorProperty);
                if (color == ItemBackgroundColor)
                    color = (0.2126 * ItemBackgroundColor.R + 0.7152 * ItemBackgroundColor.G + 0.0722 * ItemBackgroundColor.B) > 0.5F ? Color.Black : Color.White;
                return color;
            }
            set => SetValue(TextColorProperty, value);
        }
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(BLSelector), Color.White, BindingMode.Default, propertyChanged: OnTextColorPropertyChanged);
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
            current.FrameBkgndControls = new Frame[current.Labels.Count()];

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
            for (int i = 0; i < current.FrameBkgndControls.Length; i++) current.FrameBkgndControls[i].Opacity = 0;

            // Set background of the selected boxview to SelectedItemBackgroundColor
            current.FrameBkgndControls[current.Selected].Opacity = 1;

            // Execute corresponding Action
            if (current.Commands != null) current.Commands[current.Selected]?.Execute(null);

            Debug.WriteLine($"Details for label's Background (BoxView):");
            Debug.WriteLine($"    BoxViewControls[0].Opacity = {current.FrameBkgndControls[0].Opacity}");
            Debug.WriteLine($"    BoxViewControls[0].Height = {current.FrameBkgndControls[0].Height}");
            Debug.WriteLine($"    BoxViewControls[0].Width = {current.FrameBkgndControls[0].Width}");
            Debug.WriteLine($"    BoxViewControls[0].CornerRadius = {current.FrameBkgndControls[0].CornerRadius}");
            Debug.WriteLine($"Details for label's Background (BoxView):");
            Debug.WriteLine($"    BoxViewControls[1].Opacity = {current.FrameBkgndControls[1].Opacity}");
            Debug.WriteLine($"    BoxViewControls[1].Height = {current.FrameBkgndControls[1].Height}");
            Debug.WriteLine($"    BoxViewControls[1].Width = {current.FrameBkgndControls[1].Width}");
            Debug.WriteLine($"    BoxViewControls[1].CornerRadius = ({current.FrameBkgndControls[1].CornerRadius}");
            Debug.WriteLine($"");
            Debug.WriteLine($"");

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

        private Frame[] FrameBkgndControls;
        private Grid Content;
        private Frame frameBackGround1, frameBackGround2;


        public void OnItemTapped(object obj)
        {
            Debug.WriteLine($"Selected item = {FrameBkgndControls.IndexOf(obj)} of [0..{FrameBkgndControls.Length - 1}]");
            Debug.WriteLine($"");
            
            Selected = FrameBkgndControls.IndexOf(obj);
        }

        // Set up the content of the control / grid :
        // Content[0] = Background - Of  SelectedItemBackgroundColor  to draw the border of the control
        // Content[1] = Background (a litle bit smaller) - Of  ItemBackgroundColor  to draw the foreground of the labels when not selected
        // Content[2...2+N] = BoxViewControls[0...N-1] = Background of the items when selected (of color SelectedItemBackgroundColor)
        //        => It's Opacity is changed from 1 when this item is selected to 0 when the item isn't selected
        // Content[2+N +1 ... 2+N +N] = Labels for the text
        public void GridContendUpdate()
        {
            Content = new Grid { Padding = 0, Margin = 0, ColumnSpacing = 0, RowSpacing = 0 };

            // Set up rows and columns of the grid
            Content.RowDefinitions.Add(new RowDefinition
            {
                Height = HeightRequest == -1 ? new GridLength(1, GridUnitType.Auto)
                                             : new GridLength(HeightRequest, GridUnitType.Absolute)
            });
            Labels.ForEach(s => {
                // Add a column for each Label
                Content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            });

            // Add Background - ChildIndex = 0 & 1
            frameBackGround1 = new Frame   // Background - Of  SelectedItemBackgroundColor  to draw the border of the control
            {
                BackgroundColor = SelectedItemBackgroundColor,
                BorderColor = SelectedItemBackgroundColor,
                Padding = Border,
                CornerRadius = (float)(this.CornerRadius != -1 ? this.CornerRadius : 0),
            };
            frameBackGround1.PropertyChanged += OnFramePropertyChanged;
            Content.Children.Add(frameBackGround1, 0, Labels.Count(), 0, 1);

            frameBackGround2 = new Frame   
            {
                BackgroundColor = ItemBackgroundColor,
                BorderColor = ItemBackgroundColor,
                CornerRadius = (float)(this.CornerRadius != -1 ? (this.CornerRadius > Border ? this.CornerRadius - Border : this.CornerRadius / 2) : 0)
            };
            frameBackGround2.PropertyChanged += OnFramePropertyChanged;
            frameBackGround1.Content = frameBackGround2;


            // Add Background for selected item  - ChildIndew = 2 ... 2+N
            for (int i = 0; i < FrameBkgndControls.Count(); i++)
            {
                FrameBkgndControls[i] = new Frame
                {
                    BackgroundColor = SelectedItemBackgroundColor,
                    Opacity = 0,
                    CornerRadius = this.CornerRadius != -1 ? (float)this.CornerRadius : 0F
                };
                if (WidthRequest != -1) FrameBkgndControls[i].WidthRequest = WidthRequest;
                FrameBkgndControls[i].PropertyChanged += OnFramePropertyChanged;

                // Create the Tapped Command for this boxview
                var tgr = new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 1,
                    Command = new Command(OnItemTapped),
                    CommandParameter = FrameBkgndControls[i]
                };
                FrameBkgndControls[i].GestureRecognizers.Add(tgr);
                Content.Children.Add(FrameBkgndControls[i], i, 0);
            }


            // Add Labels
            for (int i = 0; i < FrameBkgndControls.Count(); i++)
            {
                // Creates the Label
                Label label = new Label
                {
                    Text = Labels[i],
                    Padding = 0,
                    Margin = this.ItemMargin,
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
            FrameBkgndControls[Selected].Opacity = 1;

            // If the control was already set-up (we are on an update) then remove the actual content to replace/update it
            if (Children.Count() > 0) Children.RemoveAt(0);

            // Add the (new/updated) content
            Children.Add(Content);
        }


        private void OnFramePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Problem if comment removed :  Corner are correct but color of selected item disappears !!!
            // if (Device.RuntimePlatform == Device.Android) return;

            Frame frame = sender as Frame;

            if (e.PropertyName == nameof(Height))
            {
                if (this.CornerRadius == -1) {
                    frame.CornerRadius = (float)frame.Height / 2;
                    Debug.WriteLine($"---> frame.CornerRadius = ({frame.CornerRadius})");
                }
            }
        }
    }

}
