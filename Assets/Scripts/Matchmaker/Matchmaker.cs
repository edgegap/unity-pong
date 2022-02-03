using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum Mode { casual, rank };

public class MatchmakerConfiguration
{
    public string MatchmakerUrl => "https://supermatchmaker-a979815d099f47.edgegap.net";
    public string ApiToken => "ABCDEF12345";
}

public class Matchmaker
{
    private MatchmakerConfiguration _configuration;

    public Matchmaker(MatchmakerConfiguration configuration)
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

    public interface ITicket
    {
        string Id { get; }
    }

    public struct PendingTicket : ITicket
    {
        public string Id { get; private set; }
        public PendingTicket(string id)
        {
            Id = id;
        }
    }

    public struct ReadyTicket : ITicket
    {
        public string Id { get; private set; }
        public string Ip { get; private set; }
        public int Port { get; private set; }
        public ReadyTicket(string id, string serverIp, int serverPort)
        {
            Id = id;
            Ip = serverIp;
            Port = serverPort;
        }
    }


}


/// <summary>
/// I don't know why but if I put HttpClient in this class it's work, b ut not in matchmaker class. WTF?
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