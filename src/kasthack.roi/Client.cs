using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace kasthack.roi
{
    /// <summary>
    /// API client
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Gets transaction fees summary
        /// </summary>
        /// <returns></returns>
        Task<Petition> Petition(int id);
        /// <summary>
        /// Gets active petitions
        /// </summary>
        Task<ListPetition[]> Poll();
        /// <summary>
        /// Gets petitions going through advisement stage
        /// </summary>
        Task<ListPetition[]> Advisement();
        /// <summary>
        /// Gets petitions with comission verdicts
        /// </summary>
        Task<ListPetition[]> Complete();
        /// <summary>
        /// Gets archived petitions
        /// </summary>
        Task<ListPetition[]> Archive();
        /// <summary>
        /// Gets petition levels
        /// </summary>
        Task<IdTitle[]> Level();
        /// <summary>
        /// Gets petition statuses
        /// </summary>
        Task<IdTitle[]> Status();
    }
    /// <summary>
    /// API client for bitcoinfees.earn.com
    /// </summary>
    public class Client : IDisposable, IClient
    {
        private readonly HttpClient _client;
        private static readonly string _apiRoot = "https://www.roi.ru/api/";
        /// <summary>
        /// Default client instance
        /// </summary>
        public static IClient Default { get; } = new Client();
        /// <summary>
        /// Creates new client
        /// </summary>
        public Client() : this(null) { }
        /// <summary>
        /// Creates new client with custom httpClient
        /// </summary>
        /// <param name="client">Http client</param>
        public Client(HttpClient client) => _client = client ?? new HttpClient();
        /// <summary>
        /// IDisposable.Dispose
        /// </summary>
        public void Dispose() => _client.Dispose();
        ///<inheritdoc/>
        public async Task<IdTitle[]> Status() => await ExecuteAsync<IdTitle[]>("attributes/status.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<IdTitle[]> Level() => await ExecuteAsync<IdTitle[]>("attributes/level.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<ListPetition[]> Archive() => await ExecuteAsync<ListPetition[]>("petitions/archive.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<ListPetition[]> Complete() => await ExecuteAsync<ListPetition[]>("petitions/complete.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<ListPetition[]> Advisement() => await ExecuteAsync<ListPetition[]>("petitions/advisement.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<ListPetition[]> Poll() => await ExecuteAsync<ListPetition[]>("petitions/poll.json").ConfigureAwait(false);
        ///<inheritdoc/>
        public async Task<Petition> Petition(int id) => await ExecuteAsync<Petition>($"petition/{id}.json").ConfigureAwait(false);
        /// <summary>
        /// Get a strongly typed object from api
        /// </summary>
        private async Task<T> ExecuteAsync<T>(string uri)
        {
            string source;
            using (var response = await _client.GetAsync($"{_apiRoot}{uri}").ConfigureAwait(false))
            {
                source = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            //they send broken jsons sometimes
            {
                var sb = new StringBuilder(source);
                sb.Replace('\r', ' ');
                sb.Replace('\n', ' ');
                source = sb.ToString();
            }

            var data = JsonConvert.DeserializeObject<Response<T>>(source, new JsonSerializerSettings {
                
            });
            if (data.Error != null)
            {
                throw new RoiException(data.Error.Text, data.Error.Code);
            }
            return data.Data;
        }
    }
    
    /// <summary>
    /// ROI api exception
    /// </summary>
    public class RoiException : Exception {
        /// <summary>
        /// Creates ROI exception
        /// </summary>
        internal RoiException(string message, int code) : base(message) => Code = code;
        /// <summary>
        /// API error code
        /// </summary>
        public int Code { get; }
    }
    /// <summary>
    /// Response container
    /// </summary>
    internal class Response<T>
    {
        /// <summary>
        /// Actual data
        /// </summary>
        public T Data { get; set; }
        /// <summary>
        /// Error
        /// </summary>
        public Error Error { get; set; }
    }
    /// <summary>
    /// Guess what
    /// </summary>
    internal class Error
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Error description
        /// </summary>
        public string Text { get; set; }
    }
    /// <summary>
    /// Entity with id
    /// </summary>
    public class IdEntity
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public int Id { get; set; }
    }
    /// <summary>
    /// Entity with id and title
    /// </summary>
    public class IdTitle : IdEntity
    {
        /// <summary>
        /// Entity name
        /// </summary>
        public string Title { get; set; }
    }
    /// <summary>
    /// Basic petition info
    /// </summary>
    public class ListPetition : IdTitle
    {
        /// <summary>
        /// Petition level
        /// </summary>
        public IdEntity Level { get; set; }
        /// <summary>
        /// Petition status
        /// </summary>
        public IdEntity Status { get; set; }
    }
    /// <summary>
    /// Full petition
    /// </summary>
    public class Petition : IdTitle
    {
        /// <summary>
        /// Petition description
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Petition poll page
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Petition description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Petition issue description
        /// </summary>
        public string Prospective { get; set; }

        /// <summary>
        /// Petition level
        /// </summary>
        public IdTitle Level { get; set; }
        /// <summary>
        /// Petition status
        /// </summary>
        public IdTitle Status { get; set; }
        /// <summary>
        /// Petition category
        /// </summary>
        public IdTitle[] Category { get; set; }/// <summary>
                                               /// Petition status
                                               /// </summary>
        public IdTitle Result { get; set; }
        /// <summary>
        /// Petition vote state
        /// </summary>
        public Vote Vote { get; set; }
        /// <summary>
        /// Petition attachments
        /// </summary>
        public Dictionary<string, Attachment[]> Attachment { get; set; }

        /// <summary>
        /// Backing field for more convenient accessors
        /// </summary>
        [JsonProperty("decision")]
        internal Decision[] DecisionField { get { throw new InvalidOperationException(); } set => Decision = value.Select(a => a.Text).ToArray(); }
        /// <summary>
        /// Decision texts
        /// </summary>
        [JsonIgnore]
        public string[] Decision { get; set; }

        /// <summary>
        /// Backing field for more convenient accessors
        /// </summary>
        [JsonProperty]
        internal Dates Date { get; set; }
        /// <summary>
        /// Creates date backing field
        /// </summary>
        private void CreateDateIfNotExists()
        {
            if (Date == null) Date = new Dates();
            if (Date.Poll == null) Date.Poll = new Poll();
        }
        /// <summary>
        /// Poll begin date
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? Begin
        {
            get => Date?.Poll?.Begin;
            set
            {
                CreateDateIfNotExists();
                Date.Poll.Begin = value;
            }
        }
        /// <summary>
        /// Poll end date
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? End
        {
            get => Date?.Poll?.End;
            set
            {
                CreateDateIfNotExists();
                Date.Poll.End = value;
            }
        }

    }
    /// <summary>
    /// Petition dates DTO, only used internally
    /// </summary>
    internal class Dates
    {
        /// <summary>
        /// Petition poll dates
        /// </summary>
        public Poll Poll { get; set; }
    }
    /// <summary>
    /// Petition poll dates DTO, only used internally
    /// </summary>
    internal class Poll
    {
        /// <summary>
        /// Poll begin date
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset? Begin { get; set; }
        /// <summary>
        /// Poll end date
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset? End { get; set; }
    }
    /// <summary>
    /// Petition decision DTO, only used internally
    /// </summary>
    internal class Decision
    {
        /// <summary>
        /// Decision text
        /// </summary>
        public string Text { get; set; }
    }

    ///<summary>
    ///Petition vote state 
    ///</summary>
    public class Vote
    {
        ///<summary>
        ///Petition progress
        ///</summary>
        public decimal Progress { get; set; }
        ///<summary>
        ///Petition threshold 
        ///</summary>
        public int Threshold { get; set; }
        ///<summary>
        ///Number of affirmative votes
        ///</summary>
        public int Affirmative { get; set; }
        ///<summary>
        ///Number of negative votes
        ///</summary>
        public int Negative { get; set; }
    }

    ///<summary>
    ///Attachment
    ///</summary>
    public class Attachment
    {
        ///<summary>
        ///Document name
        ///</summary>
        public string Title { get; set; }
        ///<summary>
        ///Document url
        ///</summary>
        public string Url { get; set; }
    }
    internal class UnixTimeConverter : JsonConverter
    {
        private static readonly Type DTOType = typeof(DateTimeOffset);
        private static readonly Type DTONType = typeof(DateTimeOffset?);
        public override bool CanConvert(Type objectType) => objectType == DTONType || objectType == DTOType;
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer: return DateTimeOffset.FromUnixTimeSeconds((long)reader.Value);
                default: throw new FormatException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}