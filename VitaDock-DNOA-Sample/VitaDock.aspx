<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="VitaDock.aspx.cs" Inherits="Example.OAuth.VitaDock" Async="true" %>

<!DOCTYPE html>
<html>
<head>
    <title></title>
</head>
<body>
    <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            <h2>VitaDock setup</h2>
            <p>A VitaDock client app must be endorsed by a VitaDock user. </p>
            <ol>
                <li><a target="_blank" href="https://vitacloud.medisanaspace.com/">Visit VitaDock and create
					a client app</a>. </li>
                <li>Modify your web.config file to include your consumer key and consumer secret.</li>
            </ol>
        </asp:View>
        <asp:View runat="server">
            <form runat="server">

                <h2>Updates</h2>
                <p>
                    Ok, VitaDock has authorized us to download your data. Notice how we never asked
				you for your VitaDock username or password.
                </p>
                <p>
                    We could show data from VitaDock here...
                </p>
                <p>
                    Click &#39;Get updates&#39; to download updates to this sample.
                </p>
                <p>
                    <asp:TextBox ID="tempTXT" TextMode="MultiLine" runat="server">...</asp:TextBox>
                </p>
                <asp:Button ID="downloadUpdates" runat="server" Text="Get updates" OnClick="Action_Click" />
                <asp:Button ID="logThermoTestData" runat="server" Text="Log temperatur test data" OnClick="LogThermoTestData" />

                <asp:PlaceHolder runat="server" ID="resultsPlaceholder" />
            </form>
        </asp:View>
    </asp:MultiView>
</body>
</html>