﻿using System;
using System.Net;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.UmbracoCms.V9.Data.Models;
using Enterspeed.Source.UmbracoCms.V9.Exceptions;
using Enterspeed.Source.UmbracoCms.V9.Services;

namespace Enterspeed.Source.UmbracoCms.V9.Handlers
{
    public class EnterspeedDeletedEntityJobHandler : IEnterspeedJobHandler
    {
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedDeletedEntityJobHandler(
            IEnterspeedIngestService enterspeedIngestService,
            IEntityIdentityService entityIdentityService)
        {
            _enterspeedIngestService = enterspeedIngestService;
            _entityIdentityService = entityIdentityService;
        }

        public bool CanHandle(EnterspeedJob job)
        {
            return 
                (job.EntityType == EnterspeedJobEntityType.Content || job.EntityType == EnterspeedJobEntityType.Dictionary)
                && job.JobType == EnterspeedJobType.Delete;
        }

        public void Handle(EnterspeedJob job)
        {
            var id = _entityIdentityService.GetId(job.EntityId, job.Culture);
            var deleteResponse = _enterspeedIngestService.Delete(id);
            if (!deleteResponse.Success && deleteResponse.Status != HttpStatusCode.NotFound)
            {
                throw new JobHandlingException($"Failed deleting entity ({job.EntityId}/{job.Culture}). Message: {deleteResponse.Message}");
            }
        }
    }
}
