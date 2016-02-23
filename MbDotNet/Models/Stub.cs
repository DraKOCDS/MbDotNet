﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using MbDotNet.Enums;
using MbDotNet.Interfaces;
using MbDotNet.Models.Predicates;
using MbDotNet.Models.Responses;
using Newtonsoft.Json;

namespace MbDotNet.Models
{
    public class Stub : IStub
    {
        [JsonProperty("responses")]
        public ICollection<IResponse> Responses { get; private set; }

        [JsonProperty("predicates")]
        public ICollection<IPredicate> Predicates { get; private set; }

        public Stub()
        {
            Responses = new List<IResponse>();
            Predicates = new List<IPredicate>();
        }

        public IStub ReturnsStatus(HttpStatusCode statusCode)
        {
            var response = new IsResponse(statusCode, null, null);
            return Returns(response);
        }

        public IStub ReturnsJson<T>(HttpStatusCode statusCode, T responseObject)
        {
            return Returns(statusCode, new Dictionary<string, string> { { "Content-Type", "application/json" } }, responseObject);
        }

        public IStub ReturnsXml<T>(HttpStatusCode statusCode, T responseObject)
        {
            var responseObjectXml = ConvertResponseObjectToXml(responseObject);

            return Returns(statusCode, new Dictionary<string, string> { {"Content-Type", "application/xml"} }, responseObjectXml);
        }

        private static string ConvertResponseObjectToXml<T>(T objectToSerialize)
        {
            var serializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();

            using (var writer = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(writer, objectToSerialize);
                return stringWriter.ToString();
            }
        }

        public IStub Returns(HttpStatusCode statusCode, IDictionary<string, string> headers, object responseObject)
        {
            var response = new IsResponse(statusCode, responseObject, headers);
            return Returns(response);
        }

        public IStub Returns(IResponse response)
        {
            Responses.Add(response);
            return this;
        }

        public IStub OnPathEquals(string path)
        {
            var predicate = new EqualsPredicate(path, null, null, null, null);
            return On(predicate);
        }

        public IStub OnPathAndMethodEqual(string path, Method method)
        {
            var predicate = new EqualsPredicate(path, method, null, null, null);
            return On(predicate);
        }

        public IStub On(IPredicate predicate)
        {
            Predicates.Add(predicate);
            return this;
        }
    }
}
