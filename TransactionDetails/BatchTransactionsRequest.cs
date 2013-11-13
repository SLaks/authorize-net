﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using AuthorizeNetLite.Options;
using AuthorizeNetLite.Transactions;

namespace AuthorizeNetLite.TransactionDetails {
  [Serializable]
  [XmlRoot("getTransactionListRequest", Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
  public sealed class BatchTransactionsRequest {
    [XmlElement("merchantAuthentication")]
    public Authentication Credentials { get; set; }
    [XmlElement("batchId")]
    public long BatchID { get; set; }

    [XmlIgnore]
    public BatchTransactionsResponse Response { get; set; }

    public void Post(GatewayUrl url) {
      string xml = "";

      var serializer = new XmlSerializer(this.GetType());
      var xn = new XmlSerializerNamespaces();
      xn.Add("", "");
      using (MemoryStream ms = new MemoryStream()) {
        using (StreamWriter sw = new StreamWriter(ms)) {
          serializer.Serialize(sw, this);
          ms.Position = 0;
          xml = Encoding.UTF8.GetString(ms.ToArray());
        }
      }

      try {
        HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(StringEnum.GetValue(url));
        authRequest.Method = "POST";
        authRequest.ContentLength = xml.Length;
        authRequest.ContentType = "text/xml";

        using (StreamWriter sw = new StreamWriter(authRequest.GetRequestStream())) {
          sw.Write(xml);
        }

        this.Response = null;

        HttpWebResponse authResponse = (HttpWebResponse)authRequest.GetResponse();

        using (StreamReader sr = new StreamReader(authResponse.GetResponseStream())) {
          xml = sr.ReadToEnd();
          try {
            var ser = new XmlSerializer(typeof(BatchTransactionsResponse));
            this.Response = (BatchTransactionsResponse)ser.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
          }
          catch (Exception e) {
            this.Response = null;
          }
        }
      }
      catch (WebException w) {
        Console.WriteLine(w.Message);
      }
    }
  }
}