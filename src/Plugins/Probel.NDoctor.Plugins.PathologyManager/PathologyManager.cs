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
namespace Probel.NDoctor.Plugins.PathologyManager
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;
    using System.Windows.Media;

    using AutoMapper;

    using Probel.Helpers.Assertion;
    using Probel.Helpers.Strings;
    using Probel.Mvvm.DataBinding;
    using Probel.NDoctor.Domain.Components;
    using Probel.NDoctor.Domain.DAL.Components;
    using Probel.NDoctor.Domain.DTO;
    using Probel.NDoctor.Domain.DTO.Components;
    using Probel.NDoctor.Domain.DTO.Objects;
    using Probel.NDoctor.Plugins.PathologyManager.Properties;
    using Probel.NDoctor.Plugins.PathologyManager.View;
    using Probel.NDoctor.Plugins.PathologyManager.ViewModel;
    using Probel.NDoctor.View.Core.Helpers;
    using Probel.NDoctor.View.Plugins;
    using Probel.NDoctor.View.Plugins.Helpers;
    using Probel.NDoctor.View.Plugins.MenuData;

    [Export(typeof(IPlugin))]
    public class PathologyManager : Plugin
    {
        #region Fields

        private const string imgUri = @"\Probel.NDoctor.Plugins.PathologyManager;component/Images\{0}.png";

        private ICommand navigateCommand = null;
        private Workbench workbench;

        #endregion Fields

        #region Constructors

        [ImportingConstructor]
        public PathologyManager([Import("version")] Version version)
            : base(version)
        {
            this.Validator = new PluginValidator("3.0.0.0", ValidationMode.Minimum);

            this.ConfigureAutoMapper();
        }

        #endregion Constructors

        #region Properties

        private WorkbenchViewModel ViewModel
        {
            get
            {
                Assert.IsNotNull(PluginContext.Host, string.Format(
                    "The IPluginHost is not set. It is impossible to setup the data context of the workbench of the plugin '{0}'", this.GetType().Name));
                if (this.workbench.DataContext == null) this.workbench.DataContext = new WorkbenchViewModel();
                return this.workbench.DataContext as WorkbenchViewModel;
            }
            set
            {
                Assert.IsNotNull(this.workbench.DataContext);
                this.workbench.DataContext = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialises this plugin. Basicaly it should configure the menus into the PluginHost
        /// Every task that could throw exception should be in this method and not in the ctor.
        /// </summary>
        public override void Initialise()
        {
            Assert.IsNotNull(PluginContext.Host, "To initialise the plugin, IPluginHost should be set.");

            PluginContext.Host.Invoke(() =>
            {
                workbench = new Workbench();
                this.BuildButtons();
                this.BuildContextMenu();
            });
        }

        /// <summary>
        /// Builds the buttons for the main menu.
        /// </summary>
        private void BuildButtons()
        {
            this.navigateCommand = new RelayCommand(() => this.Navigate(), () => this.CanNavigate());

            var navigateButton = new RibbonButtonData(Messages.Title_PathologyManager
                , imgUri.FormatWith("PathologyManager")
                , navigateCommand) { Order = 3 };

            PluginContext.Host.AddInHome(navigateButton, Groups.Managers);
        }

        /// <summary>
        /// Builds the context menu of this plugin.
        /// </summary>
        private void BuildContextMenu()
        {
            var cgroup = new RibbonGroupData(Messages.Menu_Actions, 1);
            var tab = new RibbonTabData() { Header = Messages.Menu_File, ContextualTabGroupHeader = Messages.Title_PathologyManager };

            tab.GroupDataCollection.Add(cgroup);
            this.contextualMenu = new RibbonContextualTabGroupData(Messages.Title_PathologyManager, tab) { Background = Brushes.OrangeRed, IsVisible = false, };
            PluginContext.Host.AddContextualMenu(this.contextualMenu);
            PluginContext.Host.AddTab(tab);

            ICommand addPeriodCommand = new RelayCommand(() => InnerWindow.Show(Messages.Title_Add, new AddIllnessPeriodView())
                , () => PluginContext.DoorKeeper.IsUserGranted(To.Write));
            cgroup.ButtonDataCollection.Add(new RibbonButtonData(Messages.Title_AddPeriods, imgUri.FormatWith("Add"), addPeriodCommand) { Order = 1, });

            ICommand addPathologyCommand = new RelayCommand(() => InnerWindow.Show(Messages.Title_AddPathology, new AddPathologyView())
                , () => PluginContext.DoorKeeper.IsUserGranted(To.Write));
            cgroup.ButtonDataCollection.Add(new RibbonButtonData(Messages.Title_AddPathology, imgUri.FormatWith("Add"), addPathologyCommand) { Order = 2 });

            ICommand addPathologyCategory = new RelayCommand(() => InnerWindow.Show(Messages.Title_AddPathologyCategory, new AddPathologyCategoryView())
                , () => PluginContext.DoorKeeper.IsUserGranted(To.Write));
            cgroup.ButtonDataCollection.Add(new RibbonButtonData(Messages.Title_AddPathologyCategory, imgUri.FormatWith("Add"), addPathologyCategory) { Order = 3 });
        }

        private bool CanNavigate()
        {
            return PluginContext.Host.SelectedPatient != null;
        }

        private void ConfigureAutoMapper()
        {
            Mapper.CreateMap<IllnessPeriodViewModel, IllnessPeriodDto>();
            Mapper.CreateMap<IllnessPeriodDto, IllnessPeriodViewModel>();

            Mapper.CreateMap<PathologyDto, IllnessPeriodToAddViewModel>()
                .ForMember(src => src.Pathology, opt => opt.MapFrom(dest => dest));
        }

        private void Navigate()
        {
            try
            {
                this.ViewModel.Refresh();
                PluginContext.Host.WriteStatusReady();
                PluginContext.Host.Navigate(this.workbench);

                this.ShowContextMenu();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, Messages.Msg_FailToLoadPathologyManager);
            }
        }

        #endregion Methods
    }
}