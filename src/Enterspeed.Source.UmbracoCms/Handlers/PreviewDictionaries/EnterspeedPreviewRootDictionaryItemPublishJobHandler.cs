﻿using System;
using System.Text.Json;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.UmbracoCms.Data.Models;
using Enterspeed.Source.UmbracoCms.Exceptions;
using Enterspeed.Source.UmbracoCms.Models;
using Enterspeed.Source.UmbracoCms.Models.Api;
using Enterspeed.Source.UmbracoCms.Providers;
using Enterspeed.Source.UmbracoCms.Services;


namespace Enterspeed.Source.UmbracoCms.Handlers.PreviewDictionaries
{
    public class EnterspeedPreviewRootDictionaryItemPublishJobHandler : IEnterspeedJobHandler
    {
          private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedConnectionProvider _enterspeedConnectionProvider;

        public EnterspeedPreviewRootDictionaryItemPublishJobHandler(
            IEnterspeedIngestService enterspeedIngestService,
            IEntityIdentityService entityIdentityService,
            IEnterspeedConnectionProvider enterspeedConnectionProvider)
        {
            _enterspeedIngestService = enterspeedIngestService;
            _entityIdentityService = entityIdentityService;
            _enterspeedConnectionProvider = enterspeedConnectionProvider;
        }

        public bool CanHandle(EnterspeedJob job)
        {
            return UmbracoDictionariesRootEntity.EntityId.Equals(job.EntityId, StringComparison.InvariantCultureIgnoreCase)
                &&  _enterspeedConnectionProvider.GetConnection(ConnectionType.Preview) != null
                && job.EntityType == EnterspeedJobEntityType.Dictionary
                && job.ContentState == EnterspeedContentState.Preview
                && job.JobType == EnterspeedJobType.Publish;
        }

        public void Handle(EnterspeedJob job)
        {
            var umbracoData = CreateUmbracoDictionaryEntity(job);
            Ingest(umbracoData, job);
        }

        private UmbracoDictionariesRootEntity CreateUmbracoDictionaryEntity(EnterspeedJob job)
        {
            try
            {
                return new UmbracoDictionariesRootEntity(
                    _entityIdentityService, job.Culture);
            }
            catch (Exception e)
            {
                throw new JobHandlingException(
                    $"Failed creating entity ({job.EntityId}/{job.Culture}). Message: {e.Message}. StackTrace: {e.StackTrace}");
            }
        }

        private void Ingest(IEnterspeedEntity umbracoData, EnterspeedJob job)
        {
            var ingestResponse = _enterspeedIngestService.Save(umbracoData, _enterspeedConnectionProvider.GetConnection(ConnectionType.Preview));
            if (!ingestResponse.Success)
            {
                var message = JsonSerializer.Serialize(new ErrorResponse(ingestResponse));
                throw new JobHandlingException(
                    $"Failed ingesting entity ({job.EntityId}/{job.Culture}). Message: {message}");
            }
        }
    }
}