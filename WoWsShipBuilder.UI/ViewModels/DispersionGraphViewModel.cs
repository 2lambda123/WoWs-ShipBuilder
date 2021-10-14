using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class DispersionGraphViewModel : ViewModelBase
    {
        private readonly DispersionGraphsWindow self;

        public DispersionGraphViewModel(DispersionGraphsWindow win, Dispersion disp, double maxRange, string shipName)
        {
            self = win;
            var hModel = InitializeBaseModel("Horizontal Dispersion");
            var hDisp = CreateHorizontalDispersionSeries(disp, maxRange, shipName);
            hModel.Series.Add(hDisp);
            HorizontalModel = hModel;

            var vModel = InitializeBaseModel("Vertical Dispersion");
            var vDisp = CreateVerticalDispersionSeries(disp, maxRange, shipName);
            vModel.Series.Add(vDisp);
            VerticalModel = vModel;
        }

        private AvaloniaList<string> shipNames = new();

        public AvaloniaList<string> ShipNames
        {
            get => shipNames;
            set => this.RaiseAndSetIfChanged(ref shipNames, value);
        }

        private PlotModel? horizontalModel;

        public PlotModel? HorizontalModel
        {
            get => horizontalModel;
            set => this.RaiseAndSetIfChanged(ref horizontalModel, value);
        }

        private PlotModel? verticalModel;

        public PlotModel? VerticalModel
        {
            get => verticalModel;
            set => this.RaiseAndSetIfChanged(ref verticalModel, value);
        }

        public async void AddShip()
        {
            ShipSummary result = await ShipSelectionWindow.ShowShipSelection(self);
            Ship? ship = AppDataHelper.Instance.GetShipFromSummary(result);
            if (ship != null)
            {
                if (ship.MainBatteryModuleList.Count > 0)
                {
                    var guns = ship.MainBatteryModuleList.Values.First();
                    var hSeries = CreateHorizontalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, ship.Index);
                    var vSeries = CreateVerticalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, ship.Index);
                    HorizontalModel!.Series.Add(hSeries);
                    VerticalModel!.Series.Add(vSeries);
                    HorizontalModel!.InvalidatePlot(true);
                    verticalModel!.InvalidatePlot(true);
                    this.RaisePropertyChanged(nameof(ShipNames));
                }
                else
                {
                    var noGunMessage = Translation.ResourceManager.GetString("MessageBox_ShipNoGun", Translation.Culture);
                    var errorTitle = Translation.ResourceManager.GetString("MessageBox_Error", Translation.Culture);
                    await MessageBox.Show(self, noGunMessage!, errorTitle!, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        public async void RemoveShip()
        {
            var result = await DispersionShipRemovalDialog.ShowShipRemoval(self, shipNames.ToList());
            if (result.Count > 0)
            {
                shipNames.RemoveAll(result);
                var hTemp = new List<Series>();
                foreach (var serie in HorizontalModel!.Series)
                {
                    if (!result.Contains(serie.Title))
                    {
                        hTemp.Add(serie);
                    }
                }

                var vTemp = new List<Series>();
                foreach (var serie in VerticalModel!.Series)
                {
                    if (!result.Contains(serie.Title))
                    {
                        vTemp.Add(serie);
                    }
                }

                HorizontalModel.Series.Clear();
                VerticalModel.Series.Clear();
                foreach (var serie in hTemp)
                {
                    HorizontalModel.Series.Add(serie);
                }

                foreach (var serie in hTemp)
                {
                    VerticalModel.Series.Add(serie);
                }

                HorizontalModel!.InvalidatePlot(true);
                this.RaisePropertyChanged(nameof(ShipNames));
            }
        }

        [DependsOn(nameof(ShipNames))]
        public bool CanRemoveShip(object parameter) => shipNames.Count > 0;

        // This are all default values to make the graph looks nice
        private PlotModel InitializeBaseModel(string name)
        {
            var foreground = ConvertColorFromResource("ThemeForegroundColor");
            var foregroundLow = ConvertColorFromResource("ThemeForegroundLowColor");
            var background = ConvertColorFromResource("ThemeBackgroundColor");

            PlotModel model = new PlotModel();
            model.Title = name;
            model.TextColor = foreground;
            model.PlotAreaBorderColor = foreground;
            model.LegendPosition = LegendPosition.TopLeft;
            model.LegendBorder = foreground;
            model.LegendBorderThickness = 1;
            model.LegendBackground = background;
            model.LegendFontSize = 13;          

            var xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Range",
                IsPanEnabled = false,
                Unit = "Km",
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
            };

            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Dispersion",
                IsPanEnabled = false,
                Unit = "m",
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
            };
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            return model;
        }

        private FunctionSeries CreateHorizontalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            Func<double, double> dispFunc = (range) =>
            {
                double baseValue = range * 1000 * ((dispersion.IdealRadius * 30) - (dispersion.MinRadius * 30)) / (dispersion.IdealDistance * 30);
                if (range * 1000 <= dispersion.TaperDist)
                {
                    return baseValue + (dispersion.MinRadius * 30 * ((range * 1000) / dispersion.TaperDist));
                }
                else
                {
                    return baseValue + (dispersion.MinRadius * 30);
                }
            };
            var shipName = Localizer.Instance[$"{name}_FULL"].Localization;
            shipNames.Add(shipName);
            var dispSeries = new FunctionSeries(dispFunc, 0, (maxRange * 1.5) / 1000, 0.01, shipName);
            dispSeries.TrackerFormatString = "{0}\n{1}: {1}: {2:#.00} Km\n{3}: {4:#.00} m";
            dispSeries.StrokeThickness = 4;
            return dispSeries;
        }

        private FunctionSeries CreateVerticalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            Func<double, double> dispFunc = (range) =>
             {
                 double baseValue = range * 1000 * ((dispersion.IdealRadius * 30) - (dispersion.MinRadius * 30)) / (dispersion.IdealDistance * 30);
                 double hDisp = 0;
                 if (range * 1000 <= dispersion.TaperDist)
                 {
                     hDisp = baseValue + (dispersion.MinRadius * 30 * ((range * 1000) / dispersion.TaperDist));
                 }
                 else
                 {
                     hDisp = baseValue + (dispersion.MinRadius * 30);
                 }

                 double vCoeff = 0;
                 double delimDist = dispersion.Delim * maxRange;
                 if (range * 1000 < delimDist)
                 {
                     vCoeff = dispersion.RadiusOnZero + ((dispersion.RadiusOnDelim - dispersion.RadiusOnZero) * (range * 1000 / delimDist));
                 }
                 else
                 {
                     vCoeff = dispersion.RadiusOnDelim + ((dispersion.RadiusOnMax - dispersion.RadiusOnDelim) * ((range * 1000) - delimDist) / (maxRange - delimDist));
                 }

                 return vCoeff * hDisp;
             };

            var shipName = Localizer.Instance[$"{name}_FULL"].Localization;
            shipNames.Add(shipName);
            var dispSeries = new FunctionSeries(dispFunc, 0, (maxRange * 1.5) / 1000, 0.01, shipName);
            dispSeries.TrackerFormatString = "{0}\n{1}: {1}: {2:#.00} Km\n{3}: {4:#.00} m";
            dispSeries.StrokeThickness = 4;
            return dispSeries;
        }

        private OxyColor ConvertColorFromResource(string resourceKey)
        {
            var color = self.FindResource(resourceKey) as Color?;
            return OxyColor.FromUInt32(color!.Value.ToUint32());
        }
    }
}
