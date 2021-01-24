namespace kasthack.roi.test
{
    using System;
    using System.Threading.Tasks;

    using kasthack.roi;

    using NUnit.Framework;

    [TestFixture]
    internal class ClientTests
    {
        private IClient Client { get; } = new Client();

        [Test]
        public async Task StatusWorks() => this.ValidateIdTitles(await this.Client.Status().ConfigureAwait(false));

        [Test]
        public async Task LevelWorks() => this.ValidateIdTitles(await this.Client.Level().ConfigureAwait(false));

        [Test]
        public async Task ArchiveWorks() => this.ValidatePetitions(await this.Client.Archive().ConfigureAwait(false));

        [Test]
        public async Task PollWorks() => this.ValidatePetitions(await this.Client.Poll().ConfigureAwait(false));

        [Test]
        public async Task CompleteWorks() => this.ValidatePetitions(await this.Client.Complete().ConfigureAwait(false));

        [Test]
        public async Task AdvisementWorks() => this.ValidatePetitions(await this.Client.Advisement().ConfigureAwait(false), false); // sometimes there are no petitions on advisement

        [Test]
        public void ThrowsOnError() => Assert.ThrowsAsync<RoiException>(async () => await this.Client.Petition(99999999).ConfigureAwait(false)); // bad request, petition does not exist

        [Test]
        public async Task FullPetitionWorks()
        {
            // well-known data:
            const int petitionId = 759;
            const int beginTimestamp = 1365105600;
            const int endTimestamp = 1373875200;

            var petition = await this.Client.Petition(petitionId).ConfigureAwait(false);
            this.ValidateIdTitle(petition);

            this.ValidateString(petition.Url, nameof(petition.Url));
            this.ValidateString(petition.Code, nameof(petition.Code));
            this.ValidateString(petition.Title, nameof(petition.Title));
            this.ValidateString(petition.Description, nameof(petition.Description));
            this.ValidateString(petition.Prospective, nameof(petition.Prospective));

            this.ValidateIdTitles(petition.Category);

            this.ValidateIdTitle(petition.Level);
            this.ValidateIdTitle(petition.Status);
            this.ValidateIdTitle(petition.Result, throwOnZeroId: false);

            Assert.IsNotNull(petition.Attachment);
            Assert.IsNotEmpty(petition.Attachment);

            foreach (var group in petition.Attachment)
            {
                this.ValidateString(group.Key, nameof(group.Key));
                Assert.IsNotNull(group.Value);
                Assert.IsNotEmpty(group.Value);
                foreach (var document in group.Value)
                {
                    Assert.IsNotNull(document);
                    this.ValidateString(document.Url, nameof(document.Title));
                    this.ValidateString(document.Title, nameof(document.Title));
                }
            }

            Assert.AreEqual((DateTimeOffset?)DateTimeOffset.FromUnixTimeSeconds(beginTimestamp), petition.Begin);
            Assert.AreEqual((DateTimeOffset?)DateTimeOffset.FromUnixTimeSeconds(endTimestamp), petition.End);
        }

        /// <summary>
        /// Validates array of petitions: null/empty check + checks every entity.
        /// </summary>
        private void ValidatePetitions(ListPetition[] items, bool throwOnEmpty = true)
        {
            this.ValidateIdTitles(items, throwOnEmpty);
            foreach (var item in items)
            {
                this.ValidateIdEntity(item.Level);
                this.ValidateIdEntity(item.Status);
            }
        }

        /// <summary>
        /// Validates array of IdTitles: null/empty checks + checks every item.
        /// </summary>
        private void ValidateIdTitles(IdTitle[] items, bool throwOnEmpty = true)
        {
            Assert.IsNotNull(items);
            if (throwOnEmpty)
            {
                Assert.IsNotEmpty(items);
            }

            foreach (var item in items)
            {
                this.ValidateIdTitle(item);
            }
        }

        /// <summary>
        /// Validates IdTitle entity: Id entity check + checks if title is not null / empty.
        /// </summary>
        private void ValidateIdTitle(IdTitle item, bool throwOnZeroId = true)
        {
            this.ValidateIdEntity(item, throwOnZeroId);
            this.ValidateString(item.Title, nameof(item.Title));
        }

        private void ValidateString(string value, string name)
        {
            Assert.That(value, Is.Not.Null, $"{name} must not be null");
            Assert.That(value, Is.Not.Empty, $"{name} must not be empty");
        }

        /// <summary>
        /// Validates ID entity: null-check + zero-check for id.
        /// </summary>
        private void ValidateIdEntity(IdEntity item, bool throwOnZeroId = true)
        {
            Assert.NotNull(item);
            if (throwOnZeroId)
            {
                Assert.NotZero(item.Id);
            }
        }
    }
}
