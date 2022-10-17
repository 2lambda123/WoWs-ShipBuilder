using ReactiveUI;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures.Ship.Components;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class FiringAngleViewModelBase : ViewModelBase
    {
        private IEnumerable<IGun> guns;

        private bool permaText = true;

        private string permaTextButton = Translation.FiringAngleWindow_PermaTextOff;

        private bool showAllText;

        private string showAllTextButton = Translation.FiringAngleWindow_ShowAll;

        public FiringAngleViewModelBase(IEnumerable<IGun> guns)
        {
            this.guns = guns;
        }

        public IEnumerable<IGun> Guns
        {
            get => guns;
            set => this.RaiseAndSetIfChanged(ref guns, value);
        }

        public bool ShowAllText
        {
            get => showAllText;
            set => this.RaiseAndSetIfChanged(ref showAllText, value);
        }

        public string ShowAllTextButton
        {
            get => showAllTextButton;
            set => this.RaiseAndSetIfChanged(ref showAllTextButton, value);
        }

        public bool PermaText
        {
            get => permaText;
            set => this.RaiseAndSetIfChanged(ref permaText, value);
        }

        public string PermaTextButton
        {
            get => permaTextButton;
            set => this.RaiseAndSetIfChanged(ref permaTextButton, value);
        }

        public void SetShowAll()
        {
            if (ShowAllText)
            {
                ShowAllText = false;
                ShowAllTextButton = Translation.FiringAngleWindow_ShowAll;
            }
            else
            {
                ShowAllText = true;
                ShowAllTextButton = Translation.FiringAngleWindow_HideAll;
            }
        }

        public void SetPermaText()
        {
            if (PermaText)
            {
                PermaText = false;
                PermaTextButton = Translation.FiringAngleWindow_PermaTextOn;
            }
            else
            {
                PermaText = true;
                PermaTextButton = Translation.FiringAngleWindow_PermaTextOff;
            }
        }
    }
}
