namespace Example.OAuth
{
	using DotNetOpenAuth.OAuth;
	using MedisanaSpace.Model;
	using System;
	using System.Linq;
	using System.Web.UI;

	public partial class VitaDock : System.Web.UI.Page
	{
		private AccessToken AccessToken
		{
			get { return (AccessToken)(Session["VitaDockAccessToken"] ?? new AccessToken()); }
			set { Session["VitaDockAccessToken"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct =>
					{
						var vitaDock = new VitaDockConsumer();
						if (vitaDock.ConsumerKey != null)
						{
							this.MultiView1.ActiveViewIndex = 1;

							if (!IsPostBack)
							{
								// Is VitaDock calling back with authorization?
								var accessTokenResponse = await vitaDock.ProcessUserAuthorizationAsync(this.Request.Url);
								if (accessTokenResponse != null)
								{
									this.AccessToken = accessTokenResponse.AccessToken;
								}
								else
								{
									// If we don't yet have access, immediately request it.
									Uri redirectUri = await vitaDock.RequestUserAuthorizationAsync();

									this.Response.Redirect(redirectUri.AbsoluteUri);
								}
							}
						}
					}));
		}

		protected void Action_Click(object sender, EventArgs e)
		{
			this.RegisterAsyncTask(new PageAsyncTask(
					async ct =>
					{
						if (this.AccessToken.Token == null)
						{
							this.MultiView1.ActiveViewIndex = 1;
						}
						else
						{
							var vitaDockConsumer = new VitaDockConsumer();
							var list =
								await vitaDockConsumer
								.GetLast10DaysOfThermoDocksDataAsync(this.AccessToken);

							tempTXT.Text = string.Join(
								"\n",
								list.Where<Thermodock>(t => t.Active)
								.Select<Thermodock, string>(t =>
									t.MeasurementDate + ":" + t.BodyTemperature.ToString("F")));
						}
					}));
		}

		protected void LogThermoTestData(object sender, EventArgs e)
		{
			this.RegisterAsyncTask(new PageAsyncTask(
					async ct =>
					{
						if (this.AccessToken.Token == null)
						{
							this.MultiView1.ActiveViewIndex = 1;
						}
						else
						{
							var vitaDockConsumer = new VitaDockConsumer();
							string uuid =
								await vitaDockConsumer.LogBloodSugarTestData(this.AccessToken);
							tempTXT.Text = uuid.ToString();
						}
					}));
		}
	}
}