namespace kasthack.roi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// API client.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Gets transaction fees summary.
        /// </summary>
        Task<Petition> Petition(int id);

        /// <summary>
        /// Gets active petitions.
        /// </summary>
        Task<ListPetition[]> Poll();

        /// <summary>
        /// Gets petitions going through advisement stage.
        /// </summary>
        Task<ListPetition[]> Advisement();

        /// <summary>
        /// Gets petitions with comission verdicts.
        /// </summary>
        Task<ListPetition[]> Complete();

        /// <summary>
        /// Gets archived petitions.
        /// </summary>
        Task<ListPetition[]> Archive();

        /// <summary>
        /// Gets petition levels.
        /// </summary>
        Task<IdTitle[]> Level();

        /// <summary>
        /// Gets petition statuses.
        /// </summary>
        Task<IdTitle[]> Status();
    }

    /// <summary>
    /// API client for bitcoinfees.earn.com.
    /// </summary>
    public class Client : IDisposable, IClient
    {
        private static readonly string ApiRoot = "https://www.roi.ru/api/";

        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// Creates new client.
        /// </summary>
        public Client()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// Creates new client with custom httpClient.
        /// </summary>
        /// <param name="handler">Http client handler.</param>
        public Client(HttpClientHandler handler) => this.client = handler == default ? new HttpClient() : new HttpClient(handler);

        /// <summary>
        /// Gets default client instance.
        /// </summary>
        public static IClient Default { get; } = new Client();

        /// <summary>
        /// IDisposable.Dispose.
        /// </summary>
        public void Dispose() => this.client.Dispose();

        /// <inheritdoc/>
        public async Task<IdTitle[]> Status() => await this.ExecuteAsync<IdTitle[]>("attributes/status.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<IdTitle[]> Level() => await this.ExecuteAsync<IdTitle[]>("attributes/level.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ListPetition[]> Archive() => await this.ExecuteAsync<ListPetition[]>("petitions/archive.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ListPetition[]> Complete() => await this.ExecuteAsync<ListPetition[]>("petitions/complete.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ListPetition[]> Advisement() => await this.ExecuteAsync<ListPetition[]>("petitions/advisement.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ListPetition[]> Poll() => await this.ExecuteAsync<ListPetition[]>("petitions/poll.json").ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<Petition> Petition(int id) => await this.ExecuteAsync<Petition>($"petition/{id}.json").ConfigureAwait(false);

        /// <summary>
        /// Get a strongly typed object from api.
        /// </summary>
        private async Task<T> ExecuteAsync<T>(string uri)
        {
            string source;
            using (var response = await this.client.GetAsync($"{ApiRoot}{uri}").ConfigureAwait(false))
            {
                source = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            // they send broken jsons sometimes
            {
                var sb = new StringBuilder(source);
                sb.Replace('\r', ' ');
                sb.Replace('\n', ' ');
                source = sb.ToString();
            }

            var data = JsonConvert.DeserializeObject<Response<T>>(source, new JsonSerializerSettings());
            return data.Error != null ? throw new RoiException(data.Error.Text, data.Error.Code) : data.Data;
        }
    }

    /// <summary>
    /// ROI api exception.
    /// </summary>
    public class RoiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoiException"/> class.
        /// </summary>
        internal RoiException(string message, int code)
            : base(message) => this.Code = code;

        /// <summary>
        /// Gets aPI error code.
        /// </summary>
        public int Code { get; }
    }

    /// <summary>
    /// Entity with id.
    /// </summary>
    public class IdEntity
    {
        /// <summary>
        /// Gets or sets entity ID.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Entity with id and title.
    /// </summary>
    public class IdTitle : IdEntity
    {
        /// <summary>
        /// Gets or sets entity name.
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// Basic petition info.
    /// </summary>
    public class ListPetition : IdTitle
    {
        /// <summary>
        /// Gets or sets petition level.
        /// </summary>
        public IdEntity Level { get; set; }

        /// <summary>
        /// Gets or sets petition status.
        /// </summary>
        public IdEntity Status { get; set; }
    }

    /// <summary>
    /// Full petition.
    /// </summary>
    public class Petition : IdTitle
    {
        /// <summary>
        /// Gets or sets petition description.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets petition poll page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets petition description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets petition issue description.
        /// </summary>
        public string Prospective { get; set; }

        /// <summary>
        /// Gets or sets petition level.
        /// </summary>
        public IdTitle Level { get; set; }

        /// <summary>
        /// Gets or sets petition status.
        /// </summary>
        public IdTitle Status { get; set; }

        /// <summary>
        /// Gets or sets petition category.
        /// </summary>
        public IdTitle[] Category { get; set; }

        /// <summary>
        /// Gets or sets petition status.
        /// </summary>
        public IdTitle Result { get; set; }

        /// <summary>
        /// Gets or sets petition vote state.
        /// </summary>
        public Vote Vote { get; set; }

        /// <summary>
        /// Gets or sets petition attachments.
        /// </summary>
        public Dictionary<string, Attachment[]> Attachment { get; set; }

        /// <summary>
        /// Gets or sets decision texts.
        /// </summary>
        [JsonIgnore]
        public string[] Decision { get; set; }

        /// <summary>
        /// Gets or sets poll begin date.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? Begin
        {
            get => this.Date?.Poll?.Begin;
            set
            {
                this.CreateDateIfNotExists();
                this.Date.Poll.Begin = value;
            }
        }

        /// <summary>
        /// Gets or sets poll end date.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? End
        {
            get => this.Date?.Poll?.End;
            set
            {
                this.CreateDateIfNotExists();
                this.Date.Poll.End = value;
            }
        }

        /// <summary>
        /// Gets or sets backing field for more convenient accessors.
        /// </summary>
        [JsonProperty]
        internal Dates Date { get; set; }

        /// <summary>
        /// Gets or sets backing field for more convenient accessors.
        /// </summary>
        [JsonProperty("decision")]
        internal Decision[] DecisionField { get => this.Decision?.Select(a => new Decision { Text = a }).ToArray(); set => this.Decision = value.Select(a => a.Text).ToArray(); }

        /// <summary>
        /// Creates date backing field.
        /// </summary>
        private void CreateDateIfNotExists()
        {
            if (this.Date == null)
            {
                this.Date = new Dates();
            }

            if (this.Date.Poll == null)
            {
                this.Date.Poll = new Poll();
            }
        }
    }

    /// <summary>
    /// Petition vote state.
    /// </summary>
    public class Vote
    {
        /// <summary>
        /// Gets or sets petition progress.
        /// </summary>
        public decimal Progress { get; set; }

        /// <summary>
        /// Gets or sets petition threshold.
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// Gets or sets number of affirmative votes.
        /// </summary>
        public int Affirmative { get; set; }

        /// <summary>
        /// Gets or sets number of negative votes.
        /// </summary>
        public int Negative { get; set; }
    }

    /// <summary>
    /// Attachment.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Gets or sets document name.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets document url.
        /// </summary>
        public string Url { get; set; }
    }

    /// <summary>
    /// Response container.
    /// </summary>
    internal class Response<T>
    {
        /// <summary>
        /// Gets or sets actual data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets error.
        /// </summary>
        public Error Error { get; set; }
    }

    /// <summary>
    /// Guess what.
    /// </summary>
    internal class Error
    {
        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets error description.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Petition dates DTO, only used internally.
    /// </summary>
    internal class Dates
    {
        /// <summary>
        /// Gets or sets petition poll dates.
        /// </summary>
        public Poll Poll { get; set; }
    }

    /// <summary>
    /// Petition poll dates DTO, only used internally.
    /// </summary>
    internal class Poll
    {
        /// <summary>
        /// Gets or sets poll begin date.
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset? Begin { get; set; }

        /// <summary>
        /// Gets or sets poll end date.
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset? End { get; set; }
    }

    /// <summary>
    /// Petition decision DTO, only used internally.
    /// </summary>
    internal class Decision
    {
        /// <summary>
        /// Gets or sets decision text.
        /// </summary>
        public string Text { get; set; }
    }

    internal class UnixTimeConverter : JsonConverter
    {
        private static readonly Type DTOType = typeof(DateTimeOffset);
        private static readonly Type DTONType = typeof(DateTimeOffset?);

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == DTONType || objectType == DTOType;

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
