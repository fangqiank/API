using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Routing.Api.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All,Inherited = true,AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute:Attribute,IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly MediaTypeCollection _mediaTypes= new MediaTypeCollection();

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
            string mediaType,params string[] otherMediaTypes)
        {
            this._requestHeaderToMatch = requestHeaderToMatch??
                                         throw new ArgumentNullException(nameof(requestHeaderToMatch));
            if (MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                _mediaTypes.Add(parsedMediaType);
            }
            else
            {
                throw new ArgumentException(nameof(mediaType));
            }

            foreach (var mType in otherMediaTypes)
            {
                if (MediaTypeHeaderValue.TryParse(mType, out MediaTypeHeaderValue parsedOtherMediaType))
                {
                    _mediaTypes.Add(parsedOtherMediaType);
                }
                else
                {
                    throw new ArgumentException(nameof(mType));
                }
            }
            
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
                return false;
            

            var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);
            foreach (var mediaType in _mediaTypes)
            {
                var parsedMediaType = new MediaType(mediaType);

                if (parsedRequestMediaType.Equals(parsedMediaType))
                    return true;
            }

            return false;
        }

        public int Order => 0;
    }
}
