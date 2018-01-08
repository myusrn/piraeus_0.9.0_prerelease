using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NotificationTest.Notifications
{
    public class IoTHubSink : EventSink
    {
        /// <summary>
        /// Create IoTHub subscription for device client or service client
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="messageId"></param>
        /// <param name="metadata"></param>
        /// <remarks>For a service client the format is URI iothub://hostname?deviceId=id&key=sharedaccesskey&method=mymethod&propname=name&propvalue=value
        /// where propname, propvalue, and method are optional.  The use of "method" parameter indicates a direct message to the device ID. 
        /// The use of propname and propvalue indicates the service client send property with this name/value pair.
        /// For a device client the format is iothub://hostname?deviceId=id&key=sharedaccesskey&
        /// iothub://hostname?keyname=name&key=sharedaccesskey where the key parameter is the device </remarks>
        /// 
        public IoTHubSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            Uri uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            string keyName = nvc["keyname"];
            deviceId = nvc["deviceid"];
            methodName = nvc["method"];
            propertyName = nvc["propname"];
            propertyValue = nvc["propvalue"];

            if(String.IsNullOrEmpty(methodName))
            {
                deviceClient = DeviceClient.CreateFromConnectionString(String.Format("HostName={0};DeviceId={1};SharedAccessKey={2}", uri.Authority, deviceId, metadata.SymmetricKey));
            }
            else
            {
                serviceClient = ServiceClient.CreateFromConnectionString(String.Format("HostName={0};SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, metadata.SymmetricKey));
            }
        }

        private DeviceClient deviceClient;
        private ServiceClient serviceClient;
        private string deviceId;
        private string methodName;
        private string propertyName;
        private string propertyValue;

        public override async Task SendAsync(EventMessage message)
        {
            if (serviceClient != null) //send message to device
            {
                if (!String.IsNullOrEmpty(methodName)) //direct method to device
                {
                    if (message.ContentType == "application/json")
                    {
                        CloudToDeviceMethod method = new CloudToDeviceMethod(methodName);
                        method.SetPayloadJson(Encoding.UTF8.GetString(message.Message));
                        await serviceClient.InvokeDeviceMethodAsync(deviceId, method);
                    }
                    else
                    {
                        Trace.TraceWarning("Cannot send IoTHub device {0} direct message because content-type is not JSON.", deviceId);
                    }
                }
                else //command to device
                {
                    Microsoft.Azure.Devices.Message serviceMessage = new Microsoft.Azure.Devices.Message(message.Message);
                    serviceMessage.ContentType = message.ContentType;
                    serviceMessage.MessageId = message.MessageId;

                    if(!String.IsNullOrEmpty(propertyName))
                    {
                        serviceMessage.Properties.Add(propertyName, propertyValue);
                    }

                    await serviceClient.SendAsync(deviceId, serviceMessage);
                }
            }
            else if (deviceClient != null) //this subscription is a device and will send to IoTHub
            {
                Microsoft.Azure.Devices.Client.Message msg = new Microsoft.Azure.Devices.Client.Message(message.Message);
                msg.ContentType = message.ContentType;
                msg.MessageId = message.MessageId;
                if (!String.IsNullOrEmpty(propertyName))
                {
                    msg.Properties.Add(propertyName, propertyValue);
                }
                await deviceClient.SendEventAsync(msg);
            }
            else
            {
                Trace.TraceWarning("IoTHub subscription has neither Service or Device client");
            }


            







        }
    }

   
}
