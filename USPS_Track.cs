using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Xml;

public partial class USPS_track : System.Web.UI.Page
{
    private string OrderId;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.Cache.SetExpires(DateTime.MinValue);
        Response.Cache.SetAllowResponseInBrowserHistory(false);
    }
       
    protected void Page_Load(object sender, EventArgs e)
    {
        OrderId = Request.QueryString["OrderId"];
        
        string TrackingNum = Request.QueryString["TrackingNumber"];

        UspsTrackDetail.InnerHtml = "<h3>Tracking details for Order # " + OrderId + "</h3>";

        CreateTrackRequest(TrackingNum);

    }

    protected void CreateTrackRequest(string tracknum)
    {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)768 | (System.Net.SecurityProtocolType)3072;

        string sUserID = "YOUR USER ID";
        

        string strRequest = "";
        strRequest += "<TrackRequest USERID=" + "\"" + sUserID + "\"" + ">";
        strRequest += "<TrackID ID=" + "\"" + tracknum + "\"" + ">";
        strRequest += "</TrackID>";
        strRequest += "</TrackRequest>";


        WebRequest request = WebRequest.Create("https://secure.shippingapis.com/ShippingAPI.dll?API=TrackV2&XML=" + strRequest);

        request.Method = "GET";

        WebResponse response = request.GetResponse();

        XmlDocument xmldoc = new XmlDocument();

        xmldoc.Load(response.GetResponseStream());


        XmlNodeList NodeList = xmldoc.SelectNodes("TrackResponse/TrackInfo");


        string uspsXMLText = String.Empty;


        foreach (XmlNode Node in NodeList)
        {

            XmlNode TrackSummaryNode = Node.SelectSingleNode("TrackSummary");
            uspsXMLText += "Tracking Summary: " + TrackSummaryNode.InnerText + "<br /><br />";
            
            XmlNodeList TrackDetailNodeList = Node.SelectNodes("TrackDetail");

            foreach (XmlNode dNode in TrackDetailNodeList)
            {

                uspsXMLText += "Tracking Details: " + dNode.InnerText + "<br /><br />";

            }


        }

        UspsText.InnerHtml = uspsXMLText;

    }
}