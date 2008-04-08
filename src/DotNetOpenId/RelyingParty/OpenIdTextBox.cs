/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenId.Extensions;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Web;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdTextBox.EmbeddedLogoResourceName, "image/gif")]

namespace DotNetOpenId.RelyingParty
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:OpenIdTextBox runat=\"server\"></{0}:OpenIdTextBox>")]
	public class OpenIdTextBox : CompositeControl
	{
		public OpenIdTextBox()
		{
			InitializeControls();
		}

		internal const string EmbeddedLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.openid_login.gif";
		TextBox wrappedTextBox;
		protected TextBox WrappedTextBox
		{
			get { return wrappedTextBox; }
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Controls.Add(wrappedTextBox);
			if (ShouldBeFocused)
				WrappedTextBox.Focus();
		}

		protected virtual void InitializeControls()
		{
			wrappedTextBox = new TextBox();
			wrappedTextBox.ID = "wrappedTextBox";
			wrappedTextBox.CssClass = cssClassDefault;
			wrappedTextBox.Columns = columnsDefault;
			wrappedTextBox.Text = text;
			wrappedTextBox.TabIndex = TabIndexDefault;
		}

		protected bool ShouldBeFocused;
		public override void Focus()
		{
			if (Controls.Count == 0)
				ShouldBeFocused = true;
			else
				WrappedTextBox.Focus();
		}

		const string appearanceCategory = "Appearance";
		const string profileCategory = "Simple Registration";
		const string behaviorCategory = "Behavior";

		#region Properties
		const string textDefault = "";
		string text = textDefault;
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue("")]
		public string Text
		{
			get { return (WrappedTextBox != null) ? WrappedTextBox.Text : text; }
			set
			{
				text = value;
				if (WrappedTextBox != null) WrappedTextBox.Text = value;
			}
		}

		const string realmUrlViewStateKey = "RealmUrl";
		const string realmUrlDefault = "~/";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.Realm"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(realmUrlDefault)]
		public string RealmUrl
		{
			get { return (string)(ViewState[realmUrlViewStateKey] ?? realmUrlDefault); }
			set
			{
				if (Page != null && !DesignMode)
				{
					// Validate new value by trying to construct a Realm object based on it.
					new Realm(getResolvedRealm(value).ToString()); // throws an exception on failure.
				}
				else
				{
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://"))
					{
						new Uri(value.Replace("*.", "")); // make sure it's fully-qualified, but ignore wildcards
					}
					else if (value.StartsWith("~/", StringComparison.Ordinal))
					{
						// this is valid too
					}
					else
						throw new UriFormatException();
				}
				ViewState[realmUrlViewStateKey] = value; 
			}
		}

		const string immediateModeViewStateKey = "ImmediateMode";
		const bool immediateModeDefault = false;
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(immediateModeDefault)]
		public bool ImmediateMode {
			get { return (bool)(ViewState[immediateModeViewStateKey] ?? immediateModeDefault); }
			set { ViewState[immediateModeViewStateKey] = value; }
		}

		const string cssClassDefault = "openid";
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(cssClassDefault)]
		public override string CssClass
		{
			get { return WrappedTextBox.CssClass; }
			set { WrappedTextBox.CssClass = value; }
		}

		const string showLogoViewStateKey = "ShowLogo";
		const bool showLogoDefault = true;
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(showLogoDefault)]
		public bool ShowLogo {
			get { return (bool)(ViewState[showLogoViewStateKey] ?? showLogoDefault); }
			set { ViewState[showLogoViewStateKey] = value; }
		}

		const string usePersistentCookieViewStateKey = "UsePersistentCookie";
		protected const bool UsePersistentCookieDefault = false;
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(UsePersistentCookieDefault)]
		[Description("Whether to send a persistent cookie upon successful " +
			"login so the user does not have to log in upon returning to this site.")]
		public virtual bool UsePersistentCookie
		{
			get { return (bool)(ViewState[usePersistentCookieViewStateKey] ?? UsePersistentCookieDefault); }
			set { ViewState[usePersistentCookieViewStateKey] = value; }
		}

		const int columnsDefault = 40;
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(columnsDefault)]
		public int Columns
		{
			get { return WrappedTextBox.Columns; }
			set { WrappedTextBox.Columns = value; }
		}

		protected const short TabIndexDefault = 0;
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(TabIndexDefault)]
		public override short TabIndex {
			get { return WrappedTextBox.TabIndex; }
			set { WrappedTextBox.TabIndex = value; }
		}

		const string requestNicknameViewStateKey = "RequestNickname";
		const SimpleRegistrationRequest requestNicknameDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestNicknameDefault)]
		public SimpleRegistrationRequest RequestNickname
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestNicknameViewStateKey] ?? requestNicknameDefault); }
			set { ViewState[requestNicknameViewStateKey] = value; }
		}

		const string requestEmailViewStateKey = "RequestEmail";
		const SimpleRegistrationRequest requestEmailDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestEmailDefault)]
		public SimpleRegistrationRequest RequestEmail
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestEmailViewStateKey] ?? requestEmailDefault); }
			set { ViewState[requestEmailViewStateKey] = value; }
		}

		const string requestFullNameViewStateKey = "RequestFullName";
		const SimpleRegistrationRequest requestFullNameDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestFullNameDefault)]
		public SimpleRegistrationRequest RequestFullName
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestFullNameViewStateKey] ?? requestFullNameDefault); }
			set { ViewState[requestFullNameViewStateKey] = value; }
		}

		const string requestBirthDateViewStateKey = "RequestBirthday";
		const SimpleRegistrationRequest requestBirthDateDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestBirthDateDefault)]
		public SimpleRegistrationRequest RequestBirthDate
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestBirthDateViewStateKey] ?? requestBirthDateDefault); }
			set { ViewState[requestBirthDateViewStateKey] = value; }
		}

		const string requestGenderViewStateKey = "RequestGender";
		const SimpleRegistrationRequest requestGenderDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestGenderDefault)]
		public SimpleRegistrationRequest RequestGender
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestGenderViewStateKey] ?? requestGenderDefault); }
			set { ViewState[requestGenderViewStateKey] = value; }
		}

		const string requestPostalCodeViewStateKey = "RequestPostalCode";
		const SimpleRegistrationRequest requestPostalCodeDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestPostalCodeDefault)]
		public SimpleRegistrationRequest RequestPostalCode
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestPostalCodeViewStateKey] ?? requestPostalCodeDefault); }
			set { ViewState[requestPostalCodeViewStateKey] = value; }
		}

		const string requestCountryViewStateKey = "RequestCountry";
		const SimpleRegistrationRequest requestCountryDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestCountryDefault)]
		public SimpleRegistrationRequest RequestCountry
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestCountryViewStateKey] ?? requestCountryDefault); }
			set { ViewState[requestCountryViewStateKey] = value; }
		}

		const string requestLanguageViewStateKey = "RequestLanguage";
		const SimpleRegistrationRequest requestLanguageDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestLanguageDefault)]
		public SimpleRegistrationRequest RequestLanguage
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestLanguageViewStateKey] ?? requestLanguageDefault); }
			set { ViewState[requestLanguageViewStateKey] = value; }
		}

		const string requestTimeZoneViewStateKey = "RequestTimeZone";
		const SimpleRegistrationRequest requestTimeZoneDefault = SimpleRegistrationRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestTimeZoneDefault)]
		public SimpleRegistrationRequest RequestTimeZone
		{
			get { return (SimpleRegistrationRequest)(ViewState[requestTimeZoneViewStateKey] ?? requestTimeZoneDefault); }
			set { ViewState[requestTimeZoneViewStateKey] = value; }
		}

		const string policyUrlViewStateKey = "PolicyUrl";
		const string policyUrlDefault = "";
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(policyUrlDefault)]
		public string PolicyUrl
		{
			get { return (string)ViewState[policyUrlViewStateKey] ?? policyUrlDefault; }
			set {
				ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[policyUrlViewStateKey] = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri")]
		internal static void ValidateResolvableUrl(Page page, bool designMode, string value) {
			if (string.IsNullOrEmpty(value)) return;
			if (page != null && !designMode) {
				// Validate new value by trying to construct a Realm object based on it.
				new Uri(page.Request.Url, page.ResolveUrl(value)); // throws an exception on failure.
			} else {
				// We can't fully test it, but it should start with either ~/ or a protocol.
				if (Regex.IsMatch(value, @"^https?://")) {
					new Uri(value); // make sure it's fully-qualified, but ignore wildcards
				} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
					// this is valid too
				} else
					throw new UriFormatException();
			}
		}

		const string enableRequestProfileViewStateKey = "EnableRequestProfile";
		const bool enableRequestProfileDefault = true;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(enableRequestProfileDefault)]
		public bool EnableRequestProfile
		{
			get { return (bool)(ViewState[enableRequestProfileViewStateKey] ?? enableRequestProfileDefault); }
			set { ViewState[enableRequestProfileViewStateKey] = value; }
		}
		#endregion

		#region Properties to hide
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle
		{
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override FontInfo Font
		{
			get { return null; }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Height
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Width
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string ToolTip
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string SkinID
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming
		{
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!Enabled) return;
			var consumer = new OpenIdRelyingParty();
			if (consumer.Response != null) {
				switch (consumer.Response.Status) {
					case AuthenticationStatus.Canceled:
						OnCanceled(consumer.Response);
						break;
					case AuthenticationStatus.Authenticated:
						OnLoggedIn(consumer.Response);
						break;
					case AuthenticationStatus.SetupRequired:
						OnSetupRequired(consumer.Response);
						break;
					case AuthenticationStatus.Failed:
						OnFailed(consumer.Response);
						break;
					default:
						throw new InvalidOperationException("Unexpected response status code.");
				}
			}
		}

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (ShowLogo)
			{
				string logoUrl = Page.ClientScript.GetWebResourceUrl(
					typeof(OpenIdTextBox), EmbeddedLogoResourceName);
				WrappedTextBox.Style["background"] = string.Format(CultureInfo.InvariantCulture,
					"url({0}) no-repeat", logoUrl);
				WrappedTextBox.Style["background-position"] = "0 50%";
				WrappedTextBox.Style[HtmlTextWriterStyle.PaddingLeft] = "18px";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderStyle] = "solid";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderWidth] = "1px";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderColor] = "lightgray";
			}
		}

		protected IAuthenticationRequest Request;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected void PrepareAuthenticationRequest() {
			if (string.IsNullOrEmpty(Text))
				throw new InvalidOperationException(DotNetOpenId.Strings.OpenIdTextBoxEmpty);

			try {
				var consumer = new OpenIdRelyingParty();

				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = getResolvedRealm(RealmUrl);
				realm.Scheme = Page.Request.Url.Scheme;
				realm.Port = Page.Request.Url.Port;

				// Initiate openid request
				// Note: we must use realm.ToString() because trustRoot.Uri throws when wildcards are present.
				Request = consumer.CreateRequest(Text, realm.ToString());
				Request.Mode = ImmediateMode ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
				if (EnableRequestProfile) addProfileArgs(Request);
			} catch (WebException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			} catch (OpenIdException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public void LogOn()
		{
			if (Request == null)
				PrepareAuthenticationRequest();
			if (Request != null)
				Request.RedirectToProvider();
		}

		void addProfileArgs(IAuthenticationRequest request)
		{
			new SimpleRegistrationRequestFields() {
				Nickname = RequestNickname,
				Email = RequestEmail,
				FullName = RequestFullName,
				BirthDate = RequestBirthDate,
				Gender = RequestGender,
				PostalCode = RequestPostalCode,
				Country = RequestCountry,
				Language = RequestLanguage,
				TimeZone = RequestTimeZone,
				PolicyUrl = string.IsNullOrEmpty(PolicyUrl) ? 
					null : new Uri(Page.Request.Url, Page.ResolveUrl(PolicyUrl)),
			}.AddToRequest(request);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.Realm")]
		UriBuilder getResolvedRealm(string realm)
		{
			Debug.Assert(Page != null, "Current HttpContext required to resolve URLs.");
			// Allow for *. realm notation, as well as ASP.NET ~/ shortcuts.

			// We have to temporarily remove the *. notation if it's there so that
			// the rest of our URL manipulation will succeed.
			bool foundWildcard = false;
			// Note: we don't just use string.Replace because poorly written URLs
			// could potentially have multiple :// sequences in them.
			string realmNoWildcard = Regex.Replace(realm, @"^(\w+://)\*\.",
				delegate(Match m) {
					foundWildcard = true;
					return m.Groups[1].Value;
				});

			UriBuilder fullyQualifiedRealm = new UriBuilder(
				new Uri(Page.Request.Url, Page.ResolveUrl(realmNoWildcard)));

			if (foundWildcard)
			{
				fullyQualifiedRealm.Host = "*." + fullyQualifiedRealm.Host;
			}

			// Is it valid?
			// Note: we MUST use ToString.  Uri property throws if wildcard is present.
			new Realm(fullyQualifiedRealm.ToString()); // throws if not valid

			return fullyQualifiedRealm;
		}

		#region Events
		/// <summary>
		/// Fired upon completion of a successful login.
		/// </summary>
		[Description("Fired upon completion of a successful login.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		protected virtual void OnLoggedIn(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Authenticated);
			var loggedIn = LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(response);
			if (loggedIn != null)
				loggedIn(this, args);
			if (!args.Cancel)
				FormsAuthentication.RedirectFromLoginPage(
					response.ClaimedIdentifier.ToString(), UsePersistentCookie);
		}

		/// <summary>
		/// Fired when a login attempt fails.
		/// </summary>
		[Description("Fired when a login attempt fails.")]
		public event EventHandler<OpenIdEventArgs> Failed;
		protected virtual void OnFailed(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Failed);

			var failed = Failed;
			if (failed != null)
				failed(this, new OpenIdEventArgs(response));
		}

		/// <summary>
		/// Fired when an authentication attempt is canceled at the OpenID Provider.
		/// </summary>
		[Description("Fired when an authentication attempt is canceled at the OpenID Provider.")]
		public event EventHandler<OpenIdEventArgs> Canceled;
		protected virtual void OnCanceled(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Canceled);

			var canceled = Canceled;
			if (canceled != null)
				canceled(this, new OpenIdEventArgs(response));
		}

		/// <summary>
		/// Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.
		/// </summary>
		[Description("Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.")]
		public event EventHandler<OpenIdEventArgs> SetupRequired;
		protected virtual void OnSetupRequired(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.SetupRequired);
			// Why are we firing Failed when we're OnSetupRequired?  Backward compatibility.
			var setupRequired = SetupRequired;
			if (setupRequired != null)
				setupRequired(this, new OpenIdEventArgs(response));
		}

		#endregion
	}

	public class OpenIdEventArgs : EventArgs {
		/// <summary>
		/// Constructs an object with minimal information of an incomplete or failed
		/// authentication attempt.
		/// </summary>
		internal OpenIdEventArgs(Identifier claimedIdentifier) {
			ClaimedIdentifier = claimedIdentifier;
		}
		/// <summary>
		/// Constructs an object with information on a completed authentication attempt
		/// (whether that attempt was successful or not).
		/// </summary>
		internal OpenIdEventArgs(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			Response = response;
			ClaimedIdentifier = response.ClaimedIdentifier;
			ProfileFields = SimpleRegistrationFieldValues.ReadFromResponse(response);
		}
		/// <summary>
		/// Cancels the OpenID authentication and/or login process.
		/// </summary>
		public bool Cancel { get; set; }
		public Identifier ClaimedIdentifier { get; private set; }

		/// <summary>
		/// Gets the details of the OpenId authentication response.
		/// </summary>
		public IAuthenticationResponse Response { get; private set; }
		/// <summary>
		/// Gets the simple registration (sreg) extension fields given
		/// by the provider, if any.
		/// </summary>
		public SimpleRegistrationFieldValues ProfileFields { get; private set; }
	}
}
