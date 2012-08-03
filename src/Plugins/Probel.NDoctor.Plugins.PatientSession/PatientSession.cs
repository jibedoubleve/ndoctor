﻿/*
    This file is part of NDoctor.

    NDoctor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    NDoctor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with NDoctor.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace Probel.NDoctor.Plugins.PatientSession
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;

    using AutoMapper;

    using Probel.Helpers.Strings;
    using Probel.Mvvm.DataBinding;
    using Probel.NDoctor.Domain.DTO;
    using Probel.NDoctor.Domain.DTO.Objects;
    using Probel.NDoctor.Plugins.PatientSession.Properties;
    using Probel.NDoctor.Plugins.PatientSession.View;
    using Probel.NDoctor.Plugins.PatientSession.ViewModel;
    using Probel.NDoctor.View.Core.Helpers;
    using Probel.NDoctor.View.Plugins;
    using Probel.NDoctor.View.Plugins.Helpers;
    using Probel.NDoctor.View.Plugins.MenuData;

    [Export(typeof(IPlugin))]
    public class PatientSession : Plugin
    {
        #region Fields

        private const string uriIco = @"\Probel.NDoctor.Plugins.PatientSession;component/Images\{0}.ico";
        private const string uriPng = @"\Probel.NDoctor.Plugins.PatientSession;component/Images\{0}.png";

        private ICommand addCommand;
        private ICommand searchCommand;
        private ICommand showTopTenCommand;

        #endregion Fields

        #region Constructors

        [ImportingConstructor]
        public PatientSession([Import("version")] Version version)
            : base(version)
        {
            this.Validator = new PluginValidator("3.0.0.0", ValidationMode.Minimum);
            this.ConfigureAutoMapper();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Closes this plugin. That's unload all the data. Typically used when the connected user disconnect.
        /// </summary>
        public override void Close()
        {
            //Nothing to close. It is only InnerWindows
        }

        /// <summary>
        /// Initialises this plugin. Basicaly it should configure the menus into the PluginHost
        /// Every task that could throw exception should be in this method and not in the ctor.
        /// </summary>
        public override void Initialise()
        {
            this.BuildButtons();
        }

        private void BuildButtons()
        {
            #region Add
            var splitterExist = true;
            var splitter = PluginContext.Host.FindInHome("add", Groups.Tools);
            if (splitter == null || splitter.GetType() != typeof(RibbonMenuButtonData))
            {
                splitterExist = false;
                splitter = new RibbonMenuButtonData(Messages.Btn_Add, uriPng.FormatWith("Add"), null)
                {
                    Order = 1,
                    Name = "add",
                };
            }

            this.addCommand = new RelayCommand(() => this.NavigateAddPatient(), () => this.CanNavigateAddPatient());
            var addButton = new RibbonMenuItemData(Messages.Title_ButtonAddPatient, uriPng.FormatWith("Add"), this.addCommand)
            {
                Order = 2,
            };

            (splitter as RibbonMenuButtonData).Command = addCommand;
            (splitter as RibbonMenuButtonData).ControlDataCollection.Add(addButton);
            if (!splitterExist) PluginContext.Host.AddInHome((splitter as RibbonMenuButtonData), Groups.Tools);
            #endregion

            #region Search
            this.searchCommand = new RelayCommand(() => this.NavigateSearchPatient(), () => this.CanSearchPatient());
            var searchButton = new RibbonButtonData(Messages.Title_SearchPatient, this.searchCommand)
            {
                SmallImage = new Uri(uriPng.FormatWith("SearchSmall"), UriKind.Relative),
                Order = 0,
            };

            this.showTopTenCommand = new RelayCommand(() => this.NavigateTopTen(), () => this.CanSearchPatient());
            var topTenButton = new RibbonButtonData(Messages.Title_MostUsed, this.showTopTenCommand)
            {
                SmallImage = new Uri(uriPng.FormatWith("SearchSmall"), UriKind.Relative),
                Order = 0,
            };

            var searchSplitButton = new RibbonSplitButtonData(Messages.Title_ButtonSearch, uriIco.FormatWith("Search"), this.searchCommand)
            {
                Order = 0,
            };
            searchSplitButton.ControlDataCollection.Add(searchButton);
            searchSplitButton.ControlDataCollection.Add(topTenButton);

            PluginContext.Host.AddInHome(searchSplitButton, Groups.Tools);
            #endregion
        }

        private bool CanNavigateAddPatient()
        {
            return PluginContext.DoorKeeper.IsUserGranted(To.Write);
        }

        private bool CanSearchPatient()
        {
            return PluginContext.DoorKeeper.IsUserGranted(To.Read);
        }

        private void ConfigureAutoMapper()
        {
            Mapper.CreateMap<LightPatientDto, LightPatientViewModel>();
            Mapper.CreateMap<LightPatientViewModel, LightPatientDto>();
        }

        private void NavigateAddPatient()
        {
            InnerWindow.Show(Messages.Title_AddPatient, new AddPatientControl());
        }

        private void NavigateSearchPatient()
        {
            InnerWindow.Show(Messages.Title_SearchPatient, new SearchPatientControl());
        }

        private void NavigateTopTen()
        {
            InnerWindow.Show(Messages.Title_MostUsed, new TopTenControl());
        }

        #endregion Methods
    }
}