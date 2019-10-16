using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.ExternalSearch;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.Openfigi.Models;
using CluedIn.ExternalSearch.Providers.Openfigi.Vocabularies;

using RestSharp;
 
namespace CluedIn.ExternalSearch.Providers.Openfigi
{
    /// <summary>The openfigi graph external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class OpenfigiExternalSearchProvider : ExternalSearchProviderBase
    {
        public static readonly Guid ProviderId = Guid.Parse("ef9fa39c-1b6f-4b12-8e0c-9a7695f0fbc2");   // TODO: Replace value

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public OpenfigiExternalSearchProvider()
             : base(ProviderId, EntityType.Organization)
        {
            var nameBasedTokenProvider = new NameBasedTokenProvider("Openfigi");

            if (nameBasedTokenProvider.ApiToken != null)
                this.TokenProvider = new RoundRobinTokenProvider(nameBasedTokenProvider.ApiToken.Split(',', ';'));
        }

        public OpenfigiExternalSearchProvider(IList<string> tokens)
            : this(true)
        {
            this.TokenProvider = new RoundRobinTokenProvider(tokens);
        }

        public OpenfigiExternalSearchProvider(IExternalSearchTokenProvider tokenProvider)
            : this(true)
        {
            this.TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        private OpenfigiExternalSearchProvider(bool tokenProviderIsRequired)
            : base(ProviderId, EntityType.Organization)
        {
            this.TokenProviderIsRequired = tokenProviderIsRequired;
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;
 
            var existingResults = request.GetQueryResults<MappingResponse>(this).ToList();
 
            Func<string, bool> nameFilter = value => OrganizationFilters.NameFilter(context, value) || existingResults.Any(r => string.Equals(r.Data.Response.Name, value, StringComparison.InvariantCultureIgnoreCase));
 
            // Query Input
            //For companies use CluedInOrganization vocab, for people use CluedInPerson and so on for different types.
            var entityType       = request.EntityMetaData.EntityType;
            var organizationTicker = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.TickerSymbol, new HashSet<string>());
         
            if (organizationTicker != null)
            {
                var values = organizationTicker.Select(NameNormalization.Normalize).ToHashSet();
 
                foreach (var value in values.Where(v => !nameFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
            }
        }
 
        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var identifier = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();
            
            if (string.IsNullOrEmpty(identifier))
                yield break;
 
            var client = new RestClient("https://api.openfigi.com");

            //TODO: Request
            var mappingEndpoint = "/v2/mapping";
            var request = new RestRequest(mappingEndpoint, Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-OPENFIGI-APIKEY", TokenProvider.ApiToken);
            request.AddParameter("idType", "TICKER");
            request.AddParameter("idValue", identifier);
 
            var response = client.ExecuteTaskAsync<MappingResponse>(request).Result;
 
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Data != null)
                    yield return new ExternalSearchQueryResult<MappingResponse>(query, response.Data);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
        }
 
        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<MappingResponse>();
 
            var code = this.GetOriginEntityCode(resultItem);
 
            var clue = new Clue(code, context.Organization);
 
            this.PopulateMetadata(clue.Data.EntityData, resultItem);
 
            // TODO: If necessary, you can create multiple clues and return them.
 
            return new[] { clue };
        }
 
        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<MappingResponse>();
            return this.CreateMetadata(resultItem);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }
 
        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<MappingResponse> resultItem)
        {
            var metadata = new EntityMetadataPart();
 
            this.PopulateMetadata(metadata, resultItem);
 
            return metadata;
        }
 
        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<MappingResponse> resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.Response.UniqueID);
        }
 
        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("openfigi");
        }
 
        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<MappingResponse> resultItem)
        {
            var code = this.GetOriginEntityCode(resultItem);
            var vocab = new OpenfigiVocabulary();

            metadata.EntityType       = EntityType.Organization;
            metadata.Name             = resultItem.Data.Response.Name;
            metadata.Description      = resultItem.Data.Response.SecurityDescription;
            metadata.OriginEntityCode = code;
 
            metadata.Codes.Add(code);

            metadata.Properties[vocab.Figi] = resultItem.Data.Response.Figi.PrintIfAvailable();
            metadata.Properties[vocab.Name] = resultItem.Data.Response.Name.PrintIfAvailable();
            metadata.Properties[vocab.Ticker] = resultItem.Data.Response.Ticker.PrintIfAvailable();
            metadata.Properties[vocab.ExchCode] = resultItem.Data.Response.ExchCode.PrintIfAvailable();
            metadata.Properties[vocab.CompositeFIGI] = resultItem.Data.Response.CompositeFIGI.PrintIfAvailable();
            metadata.Properties[vocab.UniqueID] = resultItem.Data.Response.UniqueID.PrintIfAvailable();
            metadata.Properties[vocab.SecurityType] = resultItem.Data.Response.SecurityType.PrintIfAvailable();
            metadata.Properties[vocab.MarketSector] = resultItem.Data.Response.MarketSector.PrintIfAvailable();
            metadata.Properties[vocab.ShareClassFIGI] = resultItem.Data.Response.ShareClassFIGI.PrintIfAvailable();
            metadata.Properties[vocab.UniqueIDFutOpt] = resultItem.Data.Response.UniqueIDFutOpt.PrintIfAvailable();
            metadata.Properties[vocab.SecurityType2] = resultItem.Data.Response.SecurityType2.PrintIfAvailable();
            metadata.Properties[vocab.SecurityDescription] = resultItem.Data.Response.SecurityDescription.PrintIfAvailable();
        }
    }
}