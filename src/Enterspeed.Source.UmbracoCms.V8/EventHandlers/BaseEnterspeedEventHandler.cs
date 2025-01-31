using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.UmbracoCms.V8.Data.Models;
using Enterspeed.Source.UmbracoCms.V8.Data.Repositories;
using Enterspeed.Source.UmbracoCms.V8.Services;
using Umbraco.Core.Scoping;
using Umbraco.Web;

namespace Enterspeed.Source.UmbracoCms.V8.EventHandlers
{
    public abstract class BaseEnterspeedEventHandler
    { 
        protected IUmbracoContextFactory UmbracoContextFactory { get; }
        protected IEnterspeedJobRepository EnterspeedJobRepository { get; }
        protected IEnterspeedJobsHandlingService JobsHandlingService { get; }
        protected IEnterspeedConfigurationService ConfigurationService { get; }
        protected IScopeProvider ScopeProvider { get; set; }

        protected BaseEnterspeedEventHandler(
            IUmbracoContextFactory umbracoContextFactory,
            IEnterspeedJobRepository enterspeedJobRepository,
            IEnterspeedJobsHandlingService jobsHandlingService,
            IEnterspeedConfigurationService configurationService,
            IScopeProvider scopeProvider)
        {
            UmbracoContextFactory = umbracoContextFactory;
            EnterspeedJobRepository = enterspeedJobRepository;
            JobsHandlingService = jobsHandlingService;
            ConfigurationService = configurationService;
            ScopeProvider = scopeProvider;
        }

        protected void EnqueueJobs(IList<EnterspeedJob> jobs)
        {
            if (!jobs.Any())
            {
                return;
            }

            EnterspeedJobRepository.Save(jobs);

            using (UmbracoContextFactory.EnsureUmbracoContext())
            {
                if (ScopeProvider.Context != null)
                {
                    var key = $"UpdateEnterspeed_{DateTime.Now.Ticks}";
                    // Add a callback to the current Scope which will execute when it's completed
                    ScopeProvider.Context.Enlist(key, scopeCompleted => HandleJobs(scopeCompleted, jobs));
                }
            }
        }

        protected void HandleJobs(bool scopeCompleted, IList<EnterspeedJob> jobs)
        {
            // Do not continue if the scope did not complete - the transaction may have been canceled and rolled back
            if (scopeCompleted)
            {
                JobsHandlingService.HandleJobs(jobs);
            }
        }

        protected bool IsPublishConfigured()
        {
            return ConfigurationService.IsPublishConfigured();
        }

        protected bool IsPreviewConfigured()
        {
            return ConfigurationService.IsPreviewConfigured();
        }
    }
}