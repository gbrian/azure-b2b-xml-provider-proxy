# azure-b2b-xml-provider-proxy
Converter between custom-highly-compressed protocol and the B2B xml endpoint.

Objective is to be able to allocate the proxy close to the provider's server using Azure Functions and reduce 
the amount of traffic generated by xml endpoints.

The flow is:
 * Convert from custom protocol to provider's XML
 * Communicate with provider
 * Receive provider's response
 * Converto from XML to custom protocol (removing all not needed data)
 
 
Links & refs:

* http://stackoverflow.com/questions/814001/how-to-convert-json-to-xml-or-xml-to-json