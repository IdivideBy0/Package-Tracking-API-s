using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;

public partial class UPS_track : System.Web.UI.Page
{
    private string OrderId;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        //initialize variables
        OrderId = Request.QueryString["OrderId"];

        string TrackingNum = Request.QueryString["TrackingNumber"];

        UpsTrackDetail.InnerHtml = "<h3>Tracking details for Order # " + OrderId + "</h3>";

        CreateTrackRequest(TrackingNum);


    }

    protected void CreateTrackRequest(string tracknum)
    {

        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)768 | (System.Net.SecurityProtocolType)3072;

        string sAccessLicenseNumber = "YOUR LICENSE NUMBER";
        string sUserID = "YOUR UPS USER ID";
        string sPassword = "YOUR UPS PASSWORD";

        //string xpciVersion = "1.0001";
        string requestOption = "1";


        string strPayLoad = "";

//Build XML Access Request String

        strPayLoad = "<?xml version=\"1.0\"?>";
        strPayLoad = strPayLoad + "<AccessRequest xml:lang=\"en-US\">";
        strPayLoad = strPayLoad + "<AccessLicenseNumber>" + sAccessLicenseNumber + "</AccessLicenseNumber>";
        strPayLoad = strPayLoad + "<UserId>" + sUserID + "</UserId>";
        strPayLoad = strPayLoad + "<Password>" + sPassword + "</Password>";
        strPayLoad = strPayLoad + "</AccessRequest>";

//Build the tracking request

        strPayLoad = strPayLoad + "<?xml version=\"1.0\"?>";
        strPayLoad = strPayLoad + "<TrackRequest xml:lang=\"en-US\">";
        strPayLoad = strPayLoad + "<Request>";
        strPayLoad = strPayLoad + "<TransactionReference>";
        strPayLoad = strPayLoad + "<CustomerContext>PRP Ups Tracking</CustomerContext>";
        //strPayLoad = strPayLoad + "<XpciVersion>" + xpciVersion + "</XpciVersion>";
        strPayLoad = strPayLoad + "</TransactionReference>";
        strPayLoad = strPayLoad + "<RequestAction>Track</RequestAction>";
        strPayLoad = strPayLoad + "<RequestOption>" + requestOption + "</RequestOption>";
        strPayLoad = strPayLoad + "</Request>";
        if (tracknum != String.Empty)
        {
	        strPayLoad = strPayLoad + "<TrackingNumber>" + tracknum + "</TrackingNumber>";
        }
        else
        {
	        strPayLoad = strPayLoad + "<ReferenceNumber><Value>" + OrderId + "</Value></ReferenceNumber>";
        }
        strPayLoad = strPayLoad + "</TrackRequest>";

        // Post the XML data.

        string serverResponse = String.Empty;

        // Create a request using a URL that can receive a post. 
        WebRequest request = WebRequest.Create("https://onlinetools.ups.com/ups.app/xml/Track");
        // Set the Method property of the request to POST.
        request.Method = "POST";
        // Create POST data and convert it to a byte array.
        //string postData = "This is a test that posts this string to a Web server.";
        string postData = strPayLoad;
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        // Set the ContentType property of the WebRequest.
        request.ContentType = "application/x-www-form-urlencoded";
        // Set the ContentLength property of the WebRequest.
        request.ContentLength = byteArray.Length;
        // Get the request stream.
        Stream dataStream = request.GetRequestStream();
        // Write the data to the request stream.
        dataStream.Write(byteArray, 0, byteArray.Length);
        // Close the Stream object.
        dataStream.Close();


        // Get the response.


        WebResponse response = request.GetResponse();

        XmlDocument xmldoc = new XmlDocument();

        xmldoc.Load(response.GetResponseStream());

        XmlNodeList NodeList = xmldoc.SelectNodes("TrackResponse/Shipment/Package/Activity");

        string upsXMLText = String.Empty;

        foreach (XmlNode Node in NodeList)
        {
           
            // time stamp nodes
            XmlNode DateNode = Node.SelectSingleNode("Date");
            if (DateNode != null)
            {               
                DateTime datetime = DateTime.ParseExact(DateNode.InnerText, "yyyyMMdd", CultureInfo.InvariantCulture);
                upsXMLText += "Date: " + datetime.ToShortDateString() + " ";
            }
            
            XmlNode TimeNode = Node.SelectSingleNode("Time");
            if (TimeNode != null)
            {
                DateTime dateTime = DateTime.ParseExact(TimeNode.InnerText, "HHmmss", CultureInfo.InvariantCulture);              
                upsXMLText += "Time: " + dateTime.ToShortTimeString() + " ";
            }

            // Address nodes
            XmlNode AddrNode = Node.SelectSingleNode("ActivityLocation/Address/City");
            if (AddrNode != null)
            upsXMLText += AddrNode.InnerText + " ";

            XmlNode AddrNode2 = Node.SelectSingleNode("ActivityLocation/Address/StateProvinceCode");
            if (AddrNode2 != null)
            upsXMLText += AddrNode2.InnerText + " ";

            XmlNode AddrNode3 = Node.SelectSingleNode("ActivityLocation/Address/PostalCode");
            if(AddrNode3 != null)
            upsXMLText += AddrNode3.InnerText + " ";

            XmlNode AddrNode4 = Node.SelectSingleNode("ActivityLocation/Address/CountryCode");
            if (AddrNode4 != null)
            upsXMLText += AddrNode4.InnerText + " ";

            //description nodes

            XmlNode DescrNode1 = Node.SelectSingleNode("ActivityLocation/Description");
            if (DescrNode1 != null)
            upsXMLText += DescrNode1.InnerText + " ";
            XmlNode DescrNode2 = Node.SelectSingleNode("ActivityLocation/SignedForByName");
            if(DescrNode2 != null)
            upsXMLText += DescrNode2.InnerText + " ";

            // activity status nodes

            XmlNode StatusNode = Node.SelectSingleNode("Status/StatusType/Description");
            if (StatusNode != null)
                upsXMLText += StatusNode.InnerText + "<br /><br />";

        }

        UpsText.InnerHtml = upsXMLText;

    }
}