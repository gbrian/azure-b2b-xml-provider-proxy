# azure-b2b-xml-provider-proxy
Converter between custom-highly-compressed protocol and the B2B xml endpoint.

Objective is to be able to allocate the proxy close to the provider's server using Azure Functions and reduce 
the amount of traffic generated by xml endpoints.

The flow is:
 * Convert from custom protocol to provider's XML
 * Communicate with provider
 * Receive provider's response
 * Converto from XML to custom protocol (removing all not needed data)
 
# Setup
 * Azure billing: https://azure.microsoft.com/en-us/offers/ms-azr-0022p/
 * Visual studio: https://blogs.msdn.microsoft.com/webdev/2016/12/01/visual-studio-tools-for-azure-functions/
 * Continuous deployment: https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment
 * CORS: Remember to set * to test anywhere
 * Quota: Even going with the credits a quota is a good idea
 * Run functions locally: https://www.npmjs.com/package/azure-functions-cli
 
