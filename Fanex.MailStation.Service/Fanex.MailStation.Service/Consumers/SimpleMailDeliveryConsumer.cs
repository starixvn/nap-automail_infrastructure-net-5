namespace Fanex.MailStation.Service.Consumers;

using Fanex.MailDelivery.Services.Services;
using Logging.Extensions;
using MassTransit;
using RabbitMQ.Client;
using SimpleMailDelivery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileExtension = MailDelivery.Core.Models.FileExtension;
using LegacyAttachment = MailDelivery.Core.Models.MailAttachment;
using LegacyMail = MailDelivery.Core.Models.Mail;
using LegacyMailLinkedResource = MailDelivery.Core.Models.MailLinkedResource;

public class SimpleMailDeliveryConsumer(MailService mailService, UMService umService) : IConsumer<Mail>
{
    public async Task Consume(ConsumeContext<Mail> context)
    {
        var mail = context.Message;
        mail.AlternativeViews ??= [];
        mail.Attachments ??= [];

        var legacyMail = new LegacyMail
        {
            ApplicationSendMailId = mail.ApplicationSendMailId,
            Body = mail.Body,
            Id = mail.Id,
            IsEncrypt = mail.IsEncrypt,
            LicenseeId = mail.LicenseeId,
            MailingListId = mail.MailingListId,
            SiteId = mail.SiteId,
            Subject = mail.Subject,
            Title = mail.Title,
            AlternativeViews = mail.AlternativeViews.Select(a => new LegacyMailLinkedResource
            {
                Id = a.Id,
                ContentId = a.ContentId,
                FileContent = a.FileContent,
                FileExtension = (FileExtension)a.FileExtension,
                FileName = a.FileName,
                MailId = a.MailId
            }).ToList(),
            Attachments = mail.Attachments.Select(a => new LegacyAttachment
            {
                FileContent = a.FileContent,
                FileExtension = (FileExtension)a.FileExtension,
                FileName = a.FileName,
                Id = a.Id,
                MailId = a.MailId
            }).ToList()
        };
        await umService.WaitForUMOff();
        await mailService.SendMail(legacyMail);
    }
}

public class SimpleMailDeliveryConsumerDefinition : ConsumerDefinition<SimpleMailDeliveryConsumer>
{
    private const int RetryInterval = 1000;

    private const int RetryCount = 1;

    private const int FetchItemNumber = 1;

    public SimpleMailDeliveryConsumerDefinition()
    {
        EndpointName = "simple-mail-delivery";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SimpleMailDeliveryConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        if (endpointConfigurator is not IRabbitMqReceiveEndpointConfigurator rmq)
        {
            return;
        }

        rmq.PrefetchCount = FetchItemNumber;
        rmq.ExchangeType = ExchangeType.Topic;
        rmq.DiscardSkippedMessages();
        rmq.DiscardFaultedMessages();
        rmq.Consumer<ProcessingFaultConsumerService>();
        rmq.UseMessageRetry(retryConfig =>
            {
                retryConfig.Ignore<ArgumentNullException>();
                retryConfig.Interval(RetryCount, TimeSpan.FromMilliseconds(RetryInterval));
            }
        );
    }
}

public class ProcessingFaultConsumerService : IConsumer<Fault<Mail>>
{
    public async Task Consume(ConsumeContext<Fault<Mail>> context)
    {
        var originalMessage = context.Message;
        var mail = originalMessage.Message;
        var exception = originalMessage.Exceptions;
        var errorMessage = exception.Aggregate(string.Empty, (current, ex) => current + $"{ex.Message}\n");

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            await HandleLog(errorMessage, mail);
        }

        await Task.CompletedTask;
    }

    private static async Task HandleLog(string errorMessage, Mail mail)
    {
        var info = new Dictionary<string, object>
        {
            {"ApplicationSendMailId",mail.ApplicationSendMailId},
            {"Body",mail.Body},
            {"Id",mail.Id},
            {"IsEncrypt",mail.IsEncrypt},
            {"LicenseeId",mail.LicenseeId},
            {"MailingListId",mail.MailingListId},
            {"SiteId",mail.SiteId},
            {"Subject",mail.Subject},
            {"Title",mail.Title},
        };

        var logMessage = new LogMessage(errorMessage)
            .AddSection("Error Info", info)
            .ToString();

        await Logging.LogService.ErrorAsync(logMessage);
    }
}