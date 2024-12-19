using System;

namespace Fanex.MailDeliver.MailClient.ParamModel;

public class GetListMailByDateParam
{
    public int AppId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int TotalRows { get; set; }

    public int Page { get; set; }

    public int Counter { get; set; }

    public int Status { get; set; }
}