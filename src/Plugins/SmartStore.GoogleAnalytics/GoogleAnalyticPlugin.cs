using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.GoogleAnalytics
{
	/// <summary>
	/// Google Analytics Plugin
	/// </summary>
	public class GoogleAnalyticPlugin : BasePlugin, IWidget, IConfigurable, ICookiePublisher
	{
		#region Scripts

		private const string TRACKING_SCRIPT = @"<!-- Google code for Analytics tracking -->
<script>
	{OPTOUTCOOKIE}

    (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
    (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
    m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
    })(window,document,'script','//www.google-analytics.com/analytics.js','ga');

    ga('create', '{GOOGLEID}', 'auto');
	ga('set', 'anonymizeIp', true); 
    ga('send', 'pageview');

	{STORAGETYPE}

    {ECOMMERCE}
</script>";

		private const string ECOMMERCE_SCRIPT = @"ga('require', 'ecommerce');
ga('ecommerce:addTransaction', {
    'id': '{ORDERID}',
    'affiliation': '{SITE}',
    'revenue': '{TOTAL}',
    'shipping': '{SHIP}',
    'tax': '{TAX}',
    'currency': '{CURRENCY}'
});

{DETAILS}

ga('ecommerce:send');";

		private const string ECOMMERCE_DETAIL_SCRIPT = @"ga('ecommerce:addItem', {
    'id': '{ORDERID}',
    'name': '{PRODUCTNAME}',
    'sku': '{PRODUCTSKU}',
    'category': '{CATEGORYNAME}',
    'price': '{UNITPRICE}',
    'quantity': '{QUANTITY}'
});";

		#endregion

		private readonly ISettingService _settingService;
		private readonly GoogleAnalyticsSettings _googleAnalyticsSettings;
		private readonly ILocalizationService _localizationService;
		private readonly IWidgetService _widgetService;
		private readonly WidgetSettings _widgetSettings;

		public GoogleAnalyticPlugin(ISettingService settingService,
			GoogleAnalyticsSettings googleAnalyticsSettings,
			ILocalizationService localizationService,
			IWidgetService widgetService,
			WidgetSettings widgetSettings)
		{
			_settingService = settingService;
			_googleAnalyticsSettings = googleAnalyticsSettings;
			_localizationService = localizationService;
			_widgetService = widgetService;
			_widgetSettings = widgetSettings;
		}

		/// <summary>
		/// Gets widget zones where this widget should be rendered
		/// </summary>
		/// <returns>Widget zones</returns>
		public IList<string> GetWidgetZones()
		{
			var zones = new List<string> { "head_html_tag" };
			if (_googleAnalyticsSettings.WidgetZone.HasValue())
			{
				zones = new List<string>
				{
					_googleAnalyticsSettings.WidgetZone
				};
			}

			return zones;
		}

		/// <summary>
		/// Gets a route for provider configuration
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "WidgetsGoogleAnalytics";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.GoogleAnalytics" } };
		}

		/// <summary>
		/// Gets a route for displaying widget
		/// </summary>
		/// <param name="widgetZone">Widget zone where it's displayed</param>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "PublicInfo";
			controllerName = "WidgetsGoogleAnalytics";
			routeValues = new RouteValueDictionary()
			{
				{"area", "SmartStore.GoogleAnalytics"},
				{"widgetZone", widgetZone}
			};
		}

		/// <summary>
		/// Gets CookieInfos for display in CookieManager dialog.
		/// </summary>
		/// <returns>CookieInfo containing plugin name, cookie purpose description & cookie type</returns>
		public CookieInfo GetCookieInfo()
		{
			var widget = _widgetService.LoadWidgetBySystemName("SmartStore.GoogleAnalytics");
			if (!widget.IsWidgetActive(_widgetSettings))
				return null;

			var cookieInfo = new CookieInfo
			{
				Name = _localizationService.GetResource("Plugins.FriendlyName.SmartStore.GoogleAnalytics"),
				Description = _localizationService.GetResource("Plugins.Widgets.GoogleAnalytics.CookieInfo"),
				CookieType = CookieType.Analytics
			};

			return cookieInfo;
		}

		public override void Install()
		{
			var settings = new GoogleAnalyticsSettings
			{
				GoogleId = "UA-0000000-0",
				TrackingScript = TRACKING_SCRIPT,
				EcommerceScript = ECOMMERCE_SCRIPT,
				EcommerceDetailScript = ECOMMERCE_DETAIL_SCRIPT
			};

			_settingService.SaveSetting(settings);
			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.GoogleAnalytics", false);
			_settingService.DeleteSetting<GoogleAnalyticsSettings>();

			base.Uninstall();
		}
	}
}
