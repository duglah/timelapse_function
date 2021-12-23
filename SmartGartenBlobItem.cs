using System;
using Azure.Storage.Blobs.Models;

namespace PhilippsSmartGarten.DailyInstagramPost;

public class SmartGartenBlobItem
{
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public BlobItem BlobItem { get; set; }
}