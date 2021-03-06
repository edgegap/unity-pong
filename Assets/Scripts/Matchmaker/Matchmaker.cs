using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class ArbitriumMatchmakerConfiguration
{
    public string MatchmakerUrl => "https://supermatchmaker-a979815d099f47.edgegap.net";
    public string ApiToken => "ABCDEF12345";
}

public class ArbitriumMatchmaker : IMatchmaker
{
    private ArbitriumMatchmakerConfiguration _configuration;

    public ArbitriumMatchmaker(ArbitriumMatchmakerConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Create a ticket by calling the matchmaker. If the call fails or the status code < 200 or > 299 it returns null
    /// </summary>
    public async Task<PendingTicket> CreateTicket(Mode mode)
    {
        // Setup the post data
        var dataObject = new
        {
            edgegap_profile_id = "pong",
            matchmaking_data = new
            {
                selector_data = new
                {
                    mode = mode.ToString(),
                }
            }

        };
        string json = JsonConvert.SerializeObject(dataObject);
        StringContent postData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Make the HTTP POST
        MatchmakerUtility.HttpClient.DefaultRequestHeaders.Add("Authorization", _configuration.ApiToken);
        HttpResponseMessage response = await MatchmakerUtility.HttpClient.PostAsync($"{_configuration.MatchmakerUrl}/v1/tickets", postData).ConfigureAwait(false);
        TicketData? parsedData = null;

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            parsedData = JsonConvert.DeserializeObject<Response<TicketData>>(result).data;
        }

        if (parsedData?.Id is null)
        {
            throw new Exception($"Could not create ticket. {await response.Content.ReadAsStringAsync()}");
        }

        return new PendingTicket(parsedData?.Id);
    }

    public async Task<ReadyTicket> ResolveTicket(PendingTicket ticket, int timeout)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        ReadyTicket? result = null;


        while (watch.ElapsedMilliseconds / 1000 < timeout)
        {
            // Make the HTTP GET
            HttpResponseMessage response = await MatchmakerUtility.HttpClient.GetAsync($"{_configuration.MatchmakerUrl}/v1/tickets/{ticket.Id}").ConfigureAwait(false);

            TicketData? parsedData = null;

            if (response.IsSuccessStatusCode)
            {
                string responseResult = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.Log(responseResult);
                parsedData = JsonConvert.DeserializeObject<Response<TicketData>>(responseResult).data;
            }

            if (parsedData?.Assignment?.ServerHost != null)
            {
                string[] splitHost = parsedData?.GetConnection().Split(':');
                result = new ReadyTicket(ticket.Id, splitHost[0], Convert.ToInt32(splitHost[1]));
                break;

            }

            Thread.Sleep(1000);
        }

        watch.Stop();

        if (!result.HasValue)
        {
            throw new Exception("Could not resolve ticket");
        }


        return result.Value;
    }

    public async Task DeleteTicket(ITicket ticket)
    {
        // Make the HTTP DELETE
        HttpResponseMessage response = await MatchmakerUtility.HttpClient.DeleteAsync($"{_configuration.MatchmakerUrl}/v1/tickets/{ticket.Id}").ConfigureAwait(false);
    }

    public struct TicketAssignment
    {
        [JsonProperty("server_host")]
        public string ServerHost { get; set; }
    }
    public struct TicketData
    {
        [JsonProperty("ticket_id")]
        public string Id { get; set; }

        [JsonProperty("assignment")]
        public TicketAssignment? Assignment { get; set; }

        public string GetConnection() => Assignment?.ServerHost;
    }

    public struct Response<T>
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("data")]
        public T data { get; set; }
    }
}


/// <summary>
/// I don't know why but if I put HttpClient in this class it works, but not in matchmaker class. WTF?
/// </summary>
static class MatchmakerUtility
{
    public static readonly HttpClient HttpClient = new HttpClient();


    static MatchmakerUtility()
    {
        // Only if your matchmaker is in staging and the TLS certificate is self signed
        // Disabling certificate validation can expose you to a man-in-the-middle attack
        // which may allow your encrypted message to be read by an attacker.
        // Works for .NET framework not .NET core
        ServicePointManager.ServerCertificateValidationCallback += (s, certificate, chain, sslPolicyErrors) => true;
        HttpClient.Timeout = TimeSpan.FromSeconds(30);
    }
}