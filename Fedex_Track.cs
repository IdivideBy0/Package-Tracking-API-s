using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services.Protocols;
using System.Data;
using System.Text;
using TrackWebServiceClient.TrackServiceWebReference;


public partial class FedEx_track : System.Web.UI.Page
{
    private string OrderId;
    
    protected void Page_Load(object sender, EventArgs e)
    {

        //initialize variables
        OrderId = Request.QueryString["OrderId"];
       
        string TrackingNum = Request.QueryString["TrackingNumber"];
        
        DoTrackingStuff(OrderId, TrackingNum);

        

    }

    private void DoTrackingStuff(string orderid, string trackingnum)
    {
       
        string UserCredentialKey = "YOUR USER CREDENTIAL KEY";
        string UserCredentialPassword = "YOUR PASSWORD";
        string ClientDetailAccountNumber = "YOUR ACCOUNT NUMBER";
        string ClientDetailMeterNumber = "YOUR METER NUMBER";

        TrackRequest request = CreateTrackRequest(UserCredentialKey, UserCredentialPassword, UserCredentialKey,
                                       UserCredentialPassword, ClientDetailAccountNumber, ClientDetailMeterNumber, trackingnum, orderid);


        TrackService service = new TrackService();

        StringBuilder sb = new StringBuilder();

        
        try
        {
            // Call the Track web service passing in a TrackRequest and returning a TrackReply
            TrackReply reply = service.track(request);
            if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
            {
                //fedexTrackDetail.InnerHtml = ShowTrackReply(reply);
                sb.Append(ShowTrackReply(reply));
              
            }
            //fedexTrackDetail.InnerHtml = ShowNotifications(reply);
            //sb.Append(ShowNotifications(reply));
            fedexTrackDetail.InnerHtml = sb.ToString();
        }
        catch (SoapException se)
        {
            //Console.WriteLine(se.Detail.InnerText);
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
        }


      


    }

    private static TrackRequest CreateTrackRequest(string uCredKey, string uCredPass, string pCredKey, string pCredPass, 
        string cDetailAcct, string cDetailMeter, string tracknum, string orderid)
    {
        
        
        // Build the TrackRequest
        TrackRequest request = new TrackRequest();
        //
        request.WebAuthenticationDetail = new WebAuthenticationDetail();
        request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
        request.WebAuthenticationDetail.UserCredential.Key = uCredKey; // Replace "XXX" with the Key
        request.WebAuthenticationDetail.UserCredential.Password = uCredPass; // Replace "XXX" with the Password
        request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
        request.WebAuthenticationDetail.ParentCredential.Key = uCredKey; // Replace "XXX" with the Key
        request.WebAuthenticationDetail.ParentCredential.Password = uCredPass; // Replace "XXX"
       
        request.ClientDetail = new ClientDetail();
        request.ClientDetail.AccountNumber = cDetailAcct; // Replace "XXX" with the client's account number
        request.ClientDetail.MeterNumber = cDetailMeter; // Replace "XXX" with the client's meter number
        
        //
        request.TransactionDetail = new TransactionDetail();
        request.TransactionDetail.CustomerTransactionId = orderid;  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
        //
        request.Version = new VersionId();
        //
        // Tracking information
        request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
        request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
        request.SelectionDetails[0].PackageIdentifier.Value = tracknum; // Replace "XXX" with tracking number or door tag
        
        request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
        //
        // Date range is optional.
        // If omitted, set to false
       
        request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
        request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
        //
        // Include detailed scans is optional.
        // If omitted, set to false
        request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
        request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
        return request;
    }


    //private static string ShowTrackReply(TrackReply reply)
    private string ShowTrackReply(TrackReply reply)
    {
        
        //ContentPlaceHolder mpContentPlaceHolder;

         //mpContentPlaceHolder =  (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");

         //Label labelTrackNum = (Label)mpContentPlaceHolder.FindControl("lblTrackingNum");

       
        
        
        StringBuilder sb = new StringBuilder();
        
        
        // Track details for each package
        foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
        {
            foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
            {
                sb.Append("<h3>Tracking details for Order # " + OrderId + "</h3>");
                //sb.Append("<p>" + ShowNotification(trackDetail.Notification) + "</p>");
                sb.Append("<p style=\"background-color: #E7E7FF\">");
                sb.Append(String.Format(" Tracking number: {0}", trackDetail.TrackingNumber));

                //labelTrackNum.Text = trackDetail.TrackingNumber.ToString();

                
                //sb.Append(String.Format(" Tracking number unique identifier: {0}", trackDetail.TrackingNumberUniqueIdentifier));
                sb.Append(String.Format(" Track Status: {0} ({1})", trackDetail.StatusDetail.Description, trackDetail.StatusDetail.Code));
                //sb.Append(String.Format(" Carrier code: {0}", trackDetail.CarrierCode));
                sb.Append("</p>");

                if (trackDetail.OtherIdentifiers != null)
                {
                    foreach (TrackOtherIdentifierDetail identifier in trackDetail.OtherIdentifiers)
                    {
                        sb.Append("<p>");
                        sb.Append(String.Format("Other Identifier: {0} {1}", identifier.PackageIdentifier.Type, identifier.PackageIdentifier.Value));
                        sb.Append("</p>");
                    }
                }
                if (trackDetail.Service != null)
                {
                    sb.Append("<p>");
                    sb.Append(String.Format("ServiceInfo: {0}", trackDetail.Service.Description));
                    sb.Append("</p>");
                }
                if (trackDetail.PackageWeight != null)
                {
                    sb.Append("<p style=\"background-color: #E7E7FF\">");
                    sb.Append(String.Format("Package weight: {0} {1}", trackDetail.PackageWeight.Value, trackDetail.PackageWeight.Units));
                    sb.Append("</p>");
                }
                if (trackDetail.ShipmentWeight != null)
                {
                    sb.Append("<p>");
                    sb.Append(String.Format("Shipment weight: {0} {1}", trackDetail.ShipmentWeight.Value, trackDetail.ShipmentWeight.Units));
                    sb.Append("</p>");
                }
                if (trackDetail.Packaging != null)
                {
                    sb.Append("<p style=\"background-color: #E7E7FF\">");
                    sb.Append(String.Format("Packaging: {0}", trackDetail.Packaging));
                    sb.Append("</p>");
                }
                
                sb.Append(String.Format("<p>Package Sequence Number: {0}", trackDetail.PackageSequenceNumber)+ "     ");
                sb.Append(String.Format("Package Count: {0} </p>", trackDetail.PackageCount));
                if (trackDetail.ShipTimestampSpecified)
                {
                    sb.Append("<p style=\"background-color: #E7E7FF\">");
                    sb.Append(String.Format("Ship timestamp: {0}", trackDetail.ShipTimestamp));
                    sb.Append("</p>");
                }
                if (trackDetail.DestinationAddress != null)
                {
                    sb.Append("<p>");
                    sb.Append(String.Format("Destination: {0}, {1}", trackDetail.DestinationAddress.City, trackDetail.DestinationAddress.StateOrProvinceCode));
                    sb.Append("</p>");
                }
                if (trackDetail.ActualDeliveryTimestampSpecified)
                {
                    sb.Append("<p style=\"background-color: #E7E7FF\">");
                    sb.Append(String.Format("Actual delivery timestamp: {0}", trackDetail.ActualDeliveryTimestamp));
                    sb.Append("</p>");
                }
                /*if (trackDetail.AvailableImages != null)
                {
                    sb.Append("<p>");
                    foreach (AvailableImageType ImageType in trackDetail.AvailableImages)
                    {
                        sb.Append(String.Format("Image availability: {0}", ImageType));
                    }
                    sb.Append("</p>");
                }
                if (trackDetail.NotificationEventsAvailable != null)
                {
                    sb.Append("<p>");
                    foreach (EMailNotificationEventType notificationEventType in trackDetail.NotificationEventsAvailable)
                    {
                        sb.Append(String.Format("EmailNotificationEvent type : {0}", notificationEventType));
                    }
                    sb.Append("</p>");
                }*/

                //Events
                //Console.WriteLine();
                sb.Append("&nbsp");

                if (trackDetail.Events != null)
                {
                    sb.Append(String.Format("<h3>Track Events:</h3>" ));
                    /*foreach (TrackEvent trackevent in trackDetail.Events)
                    {
                        sb.Append("<p>");
                        if (trackevent.TimestampSpecified)
                        {
                            
                            sb.Append(String.Format("Timestamp: {0}", trackevent.Timestamp) + "     ");
                           
                        }
                        sb.Append(String.Format("Event: {0} ({1})", trackevent.EventDescription, trackevent.EventType));
                        sb.Append("***");
                        sb.Append("</p>");
                    }*/
                    // Here we will populate the Datalist item
                    DataList1.DataSource = trackDetail.Events;
                    DataList1.DataBind();



                    //sb.Append("&nbsp");
                }
                //sb.Append("**************************************");
            }
        }
        return sb.ToString();
    }


    private string ShowNotification(Notification notification)
    {
        //string strNotification = "";
        StringBuilder sb = new StringBuilder();

        sb.Append(String.Format(" Severity: {0}", notification.Severity));
        sb.Append(String.Format(" Code: {0}", notification.Code));
        sb.Append(String.Format(" Message: {0}", notification.Message));
        sb.Append(String.Format(" Source: {0}", notification.Source));

        return sb.ToString();
    }
    private string ShowNotifications(TrackReply reply)
    {
        StringBuilder sb = new StringBuilder();
        
        sb.Append("Notifications");
        for (int i = 0; i < reply.Notifications.Length; i++)
        {
            Notification notification = reply.Notifications[i];
            sb.Append(String.Format("Notification no. {0}", i));
            ShowNotification(notification);
        }

        return sb.ToString();
    }


}