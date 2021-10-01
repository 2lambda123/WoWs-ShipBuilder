﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WoWsShipBuilder.UI.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.UserControls
{
    /// <summary>
    /// A UserControl that allows to open a drop-down list of available upgrades. Clicking on one selects the upgrade.
    /// Similar functionality to a <see cref="ComboBox"/>. However, unlike the combobox, the images within the list do not get disposed unexpectedly.
    /// </summary>
    public class DropDownSelector : UserControl
    {
        #region Static Fields and Constants

        private static readonly ModernizationImageConverter Converter = new();

        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<DropDownSelector, int>(nameof(SelectedIndex), 0, notifying: SelectedIndexChanged);

        public static readonly StyledProperty<List<Modernization>> AvailableModernizationsProperty =
            AvaloniaProperty.Register<DropDownSelector, List<Modernization>>(nameof(AvailableModernizations), notifying: ModernizationListChanged);

        public static readonly StyledProperty<Action<Modernization>?> SelectedModernizationChangedProperty =
            AvaloniaProperty.Register<DropDownSelector, Action<Modernization>?>(nameof(SelectedModernizationChanged));

        private static readonly StyledProperty<IImage> SelectedImageProperty = AvaloniaProperty.Register<DropDownSelector, IImage>(
            nameof(SelectedImage),
            (IImage)Converter.Convert(DataHelper.PlaceholderModernization, typeof(IImage), null!, CultureInfo.InvariantCulture));

        private static readonly StyledProperty<IReadOnlyList<Modernization>> EffectiveModernizationsListProperty =
            AvaloniaProperty.Register<DropDownSelector, IReadOnlyList<Modernization>>(nameof(EffectiveModernizationsList));

        #endregion

        public DropDownSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the currently selected index. Index 0 will always be the placeholder modification, indicating that the user did not select an upgrade for this slot.
        /// </summary>
        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the list of available upgrades for this control. This list should not contain the placeholder upgrade object.
        /// </summary>
        public List<Modernization> AvailableModernizations
        {
            get => GetValue(AvailableModernizationsProperty);
            set => SetValue(AvailableModernizationsProperty, value);
        }

        /// <summary>
        /// Gets or sets a callback for a change of the currently selected upgrade.
        /// </summary>
        public Action<Modernization>? SelectedModernizationChanged
        {
            get => GetValue(SelectedModernizationChangedProperty);
            set => SetValue(SelectedModernizationChangedProperty, value);
        }

        /// <summary>
        /// Gets or sets the image shown as background of the button that opens the control's popup.
        /// </summary>
        private IImage SelectedImage
        {
            get => GetValue(SelectedImageProperty);
            set => SetValue(SelectedImageProperty, value);
        }

        /// <summary>
        /// Gets or sets the effective upgrade list, a concatenation of the placeholder modernization object and the <see cref="AvailableModernizations"/> property.
        /// </summary>
        private IReadOnlyList<Modernization> EffectiveModernizationsList
        {
            get => GetValue(EffectiveModernizationsListProperty);
            set => SetValue(EffectiveModernizationsListProperty, value);
        }

        /// <summary>
        /// Gets the popup that shows the list of available upgrades.
        /// </summary>
        private Popup UpgradePopup => this.FindControl<Popup>("UpgradeListPopup");

        /// <summary>
        /// Handles a change of the <see cref="SelectedIndexProperty"/> of an instance of this control.
        /// </summary>
        /// <param name="sender">The control that was changed.</param>
        /// <param name="beforeNotify">A bool indicating whether this method was called before the property changed notify.</param>
        private static void SelectedIndexChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var dropDown = (DropDownSelector)sender;
                Modernization newSelection = dropDown.EffectiveModernizationsList[dropDown.SelectedIndex];
                dropDown.SelectedImage = (IImage)Converter.Convert(newSelection, typeof(IImage), null!, CultureInfo.InvariantCulture);
                dropDown.UpgradePopup.IsOpen = false;
                dropDown.SelectedModernizationChanged?.Invoke(newSelection);
            }
        }

        /// <summary>
        /// Handles a change of the list of the <see cref="AvailableModernizationsProperty"/> of an instance of this control.
        /// </summary>
        /// <param name="sender">The control that was changed.</param>
        /// <param name="beforeNotify">A bool indicating whether this method was called before the property changed notify.</param>
        private static void ModernizationListChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var dropDown = (DropDownSelector)sender;
                dropDown.EffectiveModernizationsList = DataHelper.PlaceholderBaseList
                    .Concat(dropDown.AvailableModernizations)
                    .ToList();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // ReSharper disable twice UnusedParameter.Local
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            UpgradePopup.IsOpen = true;
        }
    }
}